using PersonalFinanceManager.Application.Accounts.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Accounts;

public class AccountAppService
{
	private readonly IUnitOfWork _uow;
	private readonly ICurrentUser _currentUser;

	public AccountAppService(IUnitOfWork uow, ICurrentUser currentUser)
	{
		_uow = uow;
		_currentUser = currentUser;
	}

	public async Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken ct = default)
	{
		var userId = RequireUserId();
		var accounts = await _uow.Accounts.GetByUserIdAsync(userId, ct);
		return accounts.Select(AccountDto.FromEntity);
	}

	public async Task<AccountDto> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		var account = await RequireOwnedAccountAsync(id, ct);
		return AccountDto.FromEntity(account);
	}

	public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
	{
		var userId = RequireUserId();

		// Load the user so the aggregate factory can enforce uniqueness.
		var user = await _uow.Users.GetWithAccountsAsync(userId, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.User), userId);

		var account = user.AddAccount(request.Name, request.Type, request.Currency, request.InitialBalance);

		if (request.Description is not null)
			account.UpdateDetails(account.Name, request.Description);

		await _uow.Accounts.AddAsync(account, ct);
		await _uow.SaveChangesAsync(ct);
		return AccountDto.FromEntity(account);
	}

	public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
	{
		var account = await RequireOwnedAccountAsync(id, ct);
		account.UpdateDetails(request.Name, request.Description);
		await _uow.Accounts.UpdateAsync(account, ct);
		await _uow.SaveChangesAsync(ct);
		return AccountDto.FromEntity(account);
	}

	public async Task ActivateAsync(Guid id, CancellationToken ct = default)
	{
		var account = await RequireOwnedAccountAsync(id, ct);
		account.Activate();
		await _uow.Accounts.UpdateAsync(account, ct);
		await _uow.SaveChangesAsync(ct);
	}

	public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
	{
		var account = await RequireOwnedAccountAsync(id, ct);
		account.Deactivate();
		await _uow.Accounts.UpdateAsync(account, ct);
		await _uow.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var account = await RequireOwnedAccountAsync(id, ct);
		await _uow.Accounts.DeleteAsync(account.Id, ct);
		await _uow.SaveChangesAsync(ct);
	}

	// ── Helpers ──────────────────────────────────────────────────────────

	private Guid RequireUserId() =>
		_currentUser.UserId ?? throw new UnauthorizedException("User identity could not be resolved.");

	private async Task<Core.Entities.Account> RequireOwnedAccountAsync(Guid id, CancellationToken ct)
	{
		var account = await _uow.Accounts.GetByIdAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.Account), id);

		if (account.UserId != RequireUserId())
			throw new EntityNotFoundException(nameof(Core.Entities.Account), id); // mask ownership

		return account;
	}
}
