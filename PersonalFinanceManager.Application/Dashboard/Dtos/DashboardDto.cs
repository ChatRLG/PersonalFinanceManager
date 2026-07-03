using PersonalFinanceManager.Application.Accounts.Dtos;
using PersonalFinanceManager.Application.Budgets.Dtos;
using PersonalFinanceManager.Application.Transactions.Dtos;

namespace PersonalFinanceManager.Application.Dashboard.Dtos;

/// <summary>Aggregated data for the dashboard view. Matches the Web DashboardDto.</summary>
public class DashboardDto
{
	public decimal TotalBalance { get; set; }
	public decimal MonthlyIncome { get; set; }
	public decimal MonthlyExpenses { get; set; }
	public decimal MonthlySavings => MonthlyIncome - MonthlyExpenses;

	public List<AccountDto> Accounts { get; set; } = new();
	public List<TransactionDto> RecentTransactions { get; set; } = new();
	public List<BudgetDto> ActiveBudgets { get; set; } = new();
	public List<CategorySpendingSummary> SpendingByCategory { get; set; } = new();
}

public class CategorySpendingSummary
{
	public string CategoryName { get; set; } = string.Empty;
	public decimal Amount { get; set; }
	public string? Colour { get; set; }
	public decimal Percentage { get; set; }
}
