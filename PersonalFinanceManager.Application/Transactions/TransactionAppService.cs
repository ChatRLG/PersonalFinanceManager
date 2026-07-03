using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Application.Transactions.Dtos;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Transactions;

/// <summary>
/// Orchestrates transaction creation/deletion so that account balances and budget spend
/// are always updated in the same database transaction (Design Rule #1).
/// </summary>
public class TransactionAppService
{
	private readonly IUnitOfWork _uow;
	private readonly ICurrentUser _currentUser;

	public TransactionAppService(IUnitOfWork uow, ICurrentUser currentUser)
	{
		_uow = uow;
		_currentUser = currentUser;
	}

	public async Task<PagedResult<TransactionDto>> GetPaginatedAsync(
		Guid? accountId, int page, int pageSize, CancellationToken ct = default)
	{
		var userId = RequireUserId();
		page = Math.Max(1, page);
		pageSize = Math.Clamp(pageSize, 1, 100);

		// Scope to the user's own account.
		if (accountId.HasValue)
			await RequireOwnedAccountAsync(accountId.Value, ct);

		var targetAccountId = accountId
			?? (await _uow.Accounts.GetByUserIdAsync(userId, ct)).FirstOrDefault()?.Id;

		if (targetAccountId is null)
			return new PagedResult<TransactionDto> { Page = page, PageSize = pageSize };

		var (items, total) = await _uow.Transactions.GetPaginatedAsync(targetAccountId.Value, page, pageSize, ct);

		return new PagedResult<TransactionDto>
		{
			Items = items.Select(TransactionDto.FromEntity),
			TotalCount = total,
			Page = page,
			PageSize = pageSize
		};
	}

	public async Task<IEnumerable<TransactionDto>> GetByAccountAsync(Guid accountId, CancellationToken ct = default)
	{
		await RequireOwnedAccountAsync(accountId, ct);
		var txns = await _uow.Transactions.GetByAccountIdAsync(accountId, ct);
		return txns.Select(TransactionDto.FromEntity);
	}

	public async Task<IEnumerable<TransactionDto>> GetRecentAsync(int count, CancellationToken ct = default)
	{
		var userId = RequireUserId();
		count = Math.Clamp(count, 1, 50);

		var accounts = (await _uow.Accounts.GetByUserIdAsync(userId, ct)).Select(a => a.Id).ToHashSet();

		// Gather recent transactions across all user accounts.
		var all = new List<Transaction>();
		foreach (var accId in accounts)
		{
			var txns = await _uow.Transactions.GetByAccountIdAsync(accId, ct);
			all.AddRange(txns);
		}

		return all
			.OrderByDescending(t => t.TransactionDate)
			.Take(count)
			.Select(TransactionDto.FromEntity);
	}

	public async Task<TransactionDto> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		var txn = await RequireOwnedTransactionAsync(id, ct);
		return TransactionDto.FromEntity(txn);
	}

	/// <summary>
	/// Creates a transaction and atomically updates account balance(s) and any
	/// matching active budget's CurrentSpend.
	/// </summary>
	public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken ct = default)
	{
		var userId = RequireUserId();

		// Validate ownership.
		var account = await RequireOwnedAccountAsync(request.AccountId, ct);
		var category = await RequireOwnedCategoryAsync(request.CategoryId, userId, ct);

		Transaction txn;

		if (request.Type == TransactionType.Transfer)
		{
			if (!request.DestinationAccountId.HasValue)
				throw new ArgumentException("DestinationAccountId is required for Transfer transactions.");

			var dest = await RequireOwnedAccountAsync(request.DestinationAccountId.Value, ct);

			// Domain behaviour: debit source, credit destination.
			account.Debit(request.Amount);
			dest.Credit(request.Amount);

			txn = new Transaction(
				request.Amount, request.Currency, request.Description,
				request.TransactionDate, account.Id, dest.Id, request.CategoryId, request.Notes);

			await _uow.Accounts.UpdateAsync(account, ct);
			await _uow.Accounts.UpdateAsync(dest, ct);
		}
		else
		{
			// Income: credit the account. Expense: debit the account.
			if (request.Type == TransactionType.Income)
				account.Credit(request.Amount);
			else
				account.Debit(request.Amount);

			txn = new Transaction(
				request.Amount, request.Currency, request.Type, request.Description,
				request.TransactionDate, account.Id, request.CategoryId, request.Notes);

			await _uow.Accounts.UpdateAsync(account, ct);
		}

		await _uow.Transactions.AddAsync(txn, ct);

		// Update budget spend if an active budget covers this expense category.
		if (request.Type == TransactionType.Expense)
			await RecordBudgetSpendAsync(userId, request.CategoryId, request.Amount, request.TransactionDate, ct);

		await _uow.SaveChangesAsync(ct);

		// Reload with navigation properties for the response.
		var saved = await _uow.Transactions.GetByIdAsync(txn.Id, ct) ?? txn;
		return TransactionDto.FromEntity(saved);
	}

	/// <summary>
	/// Deletes a transaction and atomically reverses the account balance and budget spend.
	/// </summary>
	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var txn = await RequireOwnedTransactionAsync(id, ct);
		var account = await _uow.Accounts.GetByIdAsync(txn.AccountId, ct)!;

		// Reverse the balance effect.
		if (txn.Type == TransactionType.Income)
			account!.Debit(txn.Amount);
		else if (txn.Type == TransactionType.Expense)
			account!.Credit(txn.Amount);
		else if (txn.Type == TransactionType.Transfer && txn.DestinationAccountId.HasValue)
		{
			account!.Credit(txn.Amount); // restore source
			var dest = await _uow.Accounts.GetByIdAsync(txn.DestinationAccountId.Value, ct);
			if (dest is not null)
			{
				dest.Debit(txn.Amount); // restore destination
				await _uow.Accounts.UpdateAsync(dest, ct);
			}
		}

		if (account is not null)
			await _uow.Accounts.UpdateAsync(account, ct);

		// Reverse budget spend for expenses.
		if (txn.Type == TransactionType.Expense)
			await ReverseBudgetSpendAsync(RequireUserId(), txn.CategoryId, txn.Amount, txn.TransactionDate, ct);

		await _uow.Transactions.DeleteAsync(id, ct);
		await _uow.SaveChangesAsync(ct);
	}

	// ── Budget helpers ────────────────────────────────────────────────────

	private async Task RecordBudgetSpendAsync(Guid userId, Guid categoryId, decimal amount, DateTime date, CancellationToken ct)
	{
		var budget = await _uow.Budgets.GetByCategoryAndDateAsync(userId, categoryId, date, ct);
		if (budget is null) return;

		// allowExceed = true: we record the spend even past the limit (the budget entity tracks IsExceeded).
		budget.RecordSpending(amount, allowExceed: true);
		await _uow.Budgets.UpdateAsync(budget, ct);
	}

	private async Task ReverseBudgetSpendAsync(Guid userId, Guid categoryId, decimal amount, DateTime date, CancellationToken ct)
	{
		var budget = await _uow.Budgets.GetByCategoryAndDateAsync(userId, categoryId, date, ct);
		if (budget is null) return;

		budget.ReverseSpending(amount);
		await _uow.Budgets.UpdateAsync(budget, ct);
	}

	// ── Ownership guards ──────────────────────────────────────────────────

	private Guid RequireUserId() =>
		_currentUser.UserId ?? throw new UnauthorizedException("User identity could not be resolved.");

	private async Task<Core.Entities.Account> RequireOwnedAccountAsync(Guid id, CancellationToken ct)
	{
		var userId = RequireUserId();
		var account = await _uow.Accounts.GetByIdAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.Account), id);
		if (account.UserId != userId)
			throw new EntityNotFoundException(nameof(Core.Entities.Account), id);
		return account;
	}

	private async Task<Core.Entities.Category> RequireOwnedCategoryAsync(Guid id, Guid userId, CancellationToken ct)
	{
		var category = await _uow.Categories.GetByIdAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.Category), id);
		if (category.UserId != userId)
			throw new EntityNotFoundException(nameof(Core.Entities.Category), id);
		return category;
	}

	private async Task<Transaction> RequireOwnedTransactionAsync(Guid id, CancellationToken ct)
	{
		var userId = RequireUserId();
		var txn = await _uow.Transactions.GetByIdAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Transaction), id);
		var account = await _uow.Accounts.GetByIdAsync(txn.AccountId, ct);
		if (account is null || account.UserId != userId)
			throw new EntityNotFoundException(nameof(Transaction), id);
		return txn;
	}
}
