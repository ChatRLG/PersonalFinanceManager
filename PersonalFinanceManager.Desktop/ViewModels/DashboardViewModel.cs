using System.Collections.ObjectModel;
using PersonalFinanceManager.Application.Contracts.Accounts;
using PersonalFinanceManager.Application.Contracts.Budgets;
using PersonalFinanceManager.Application.Contracts.Dashboard;
using PersonalFinanceManager.Application.Contracts.Transactions;
using PersonalFinanceManager.Desktop.Services;

namespace PersonalFinanceManager.Desktop.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IApiClient _api;

    private decimal _totalBalance;
    private decimal _monthlyIncome;
    private decimal _monthlyExpenses;
    private decimal _monthlySavings;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public DashboardViewModel(IApiClient api) => _api = api;

    public decimal TotalBalance { get => _totalBalance; set => SetProperty(ref _totalBalance, value); }
    public decimal MonthlyIncome { get => _monthlyIncome; set => SetProperty(ref _monthlyIncome, value); }
    public decimal MonthlyExpenses { get => _monthlyExpenses; set => SetProperty(ref _monthlyExpenses, value); }
    public decimal MonthlySavings { get => _monthlySavings; set => SetProperty(ref _monthlySavings, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<AccountDto> Accounts { get; } = new();
    public ObservableCollection<TransactionDto> RecentTransactions { get; } = new();
    public ObservableCollection<BudgetDto> ActiveBudgets { get; } = new();
    public ObservableCollection<CategorySpendingSummary> SpendingByCategory { get; } = new();

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var dto = await _api.GetDashboardAsync(ct);

            TotalBalance = dto.TotalBalance;
            MonthlyIncome = dto.MonthlyIncome;
            MonthlyExpenses = dto.MonthlyExpenses;
            MonthlySavings = dto.MonthlySavings;

            Accounts.Clear();
            foreach (var a in dto.Accounts) Accounts.Add(a);

            RecentTransactions.Clear();
            foreach (var t in dto.RecentTransactions) RecentTransactions.Add(t);

            ActiveBudgets.Clear();
            foreach (var b in dto.ActiveBudgets) ActiveBudgets.Add(b);

            SpendingByCategory.Clear();
            foreach (var s in dto.SpendingByCategory) SpendingByCategory.Add(s);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
