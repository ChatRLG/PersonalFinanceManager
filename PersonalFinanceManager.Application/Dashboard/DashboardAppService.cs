using PersonalFinanceManager.Application.Accounts.Dtos;
using PersonalFinanceManager.Application.Budgets.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Application.Dashboard.Dtos;
using PersonalFinanceManager.Application.Transactions.Dtos;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Dashboard;

public class DashboardAppService
{
	private readonly IUnitOfWork _uow;
	private readonly ICurrentUser _currentUser;

	public DashboardAppService(IUnitOfWork uow, ICurrentUser currentUser)
	{
		_uow = uow;
		_currentUser = currentUser;
	}

	public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
	{
		var userId = RequireUserId();

		// Run independent queries concurrently.
		var accountsTask = _uow.Accounts.GetByUserIdAsync(userId, ct);
		var activeBudgetsTask = _uow.Budgets.GetActiveBudgetsAsync(userId, ct);
		var totalBalanceTask = _uow.Accounts.GetTotalBalanceAsync(userId, ct);

		await Task.WhenAll(accountsTask, activeBudgetsTask, totalBalanceTask);

		var accounts = (await accountsTask).ToList();
		var activeBudgets = (await activeBudgetsTask).ToList();
		var totalBalance = await totalBalanceTask;

		// Monthly income/expense totals: sum across all user accounts.
		var now = DateTime.UtcNow;
		var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		var monthEnd = monthStart.AddMonths(1);

		decimal monthlyIncome = 0m, monthlyExpenses = 0m;
		var recentTransactions = new List<PersonalFinanceManager.Core.Entities.Transaction>();

		foreach (var account in accounts)
		{
			monthlyIncome += await _uow.Transactions.GetTotalByTypeAndDateRangeAsync(
				account.Id, TransactionType.Income, monthStart, monthEnd, ct);

			monthlyExpenses += await _uow.Transactions.GetTotalByTypeAndDateRangeAsync(
				account.Id, TransactionType.Expense, monthStart, monthEnd, ct);

			var txns = await _uow.Transactions.GetByAccountIdAsync(account.Id, ct);
			recentTransactions.AddRange(txns);
		}

		var recent = recentTransactions
			.OrderByDescending(t => t.TransactionDate)
			.Take(10)
			.Select(TransactionDto.FromEntity)
			.ToList();

		// Spending breakdown by category for the current month.
		var spendingByCategory = activeBudgets
			.GroupBy(b => b.Category?.Name ?? "Other")
			.Select(g => new CategorySpendingSummary
			{
				CategoryName = g.Key,
				Amount = g.Sum(b => b.CurrentSpend),
				Colour = g.First().Category?.Colour
			})
			.OrderByDescending(s => s.Amount)
			.ToList();

		// Calculate percentages.
		var totalSpend = spendingByCategory.Sum(s => s.Amount);
		foreach (var item in spendingByCategory)
			item.Percentage = totalSpend > 0 ? Math.Round(item.Amount / totalSpend * 100, 2) : 0;

		return new DashboardDto
		{
			TotalBalance = totalBalance,
			MonthlyIncome = monthlyIncome,
			MonthlyExpenses = monthlyExpenses,
			Accounts = accounts.Select(AccountDto.FromEntity).ToList(),
			RecentTransactions = recent,
			ActiveBudgets = activeBudgets.Select(b => BudgetDto.FromEntity(b)).ToList(),
			SpendingByCategory = spendingByCategory
		};
	}

	private Guid RequireUserId() =>
		_currentUser.UserId ?? throw new UnauthorizedException("User identity could not be resolved.");
}
