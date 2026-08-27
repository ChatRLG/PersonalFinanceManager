using System.Collections.ObjectModel;
using PersonalFinanceManager.Application.Contracts.Accounts;
using PersonalFinanceManager.Application.Contracts.Dashboard;
using PersonalFinanceManager.Application.Contracts.Transactions;
using PersonalFinanceManager.Mobile.Services;

namespace PersonalFinanceManager.Mobile.ViewModels;

/// <summary>
/// Read-only reports summary. Unlike Desktop's ReportsViewModel, CSV export is not
/// ported — Desktop's ICsvService depends on Microsoft.Win32.SaveFileDialog
/// (Windows-only); a MAUI-correct replacement (Share/FileSystem.CacheDirectory) is
/// deferred to Phase 6b (see docs/ROADMAP.md).
/// </summary>
public class ReportsViewModel : ViewModelBase
{
    private readonly IApiClient _api;

    private decimal _totalBalance;
    private decimal _monthlyIncome;
    private decimal _monthlyExpenses;
    private decimal _monthlySavings;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public ReportsViewModel(IApiClient api)
    {
        _api = api;
        LoadCommand = new AsyncRelayCommand(() => LoadAsync());
    }

    public decimal TotalBalance { get => _totalBalance; set => SetProperty(ref _totalBalance, value); }
    public decimal MonthlyIncome { get => _monthlyIncome; set => SetProperty(ref _monthlyIncome, value); }
    public decimal MonthlyExpenses { get => _monthlyExpenses; set => SetProperty(ref _monthlyExpenses, value); }
    public decimal MonthlySavings { get => _monthlySavings; set => SetProperty(ref _monthlySavings, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<AccountDto> Accounts { get; } = new();
    public ObservableCollection<TransactionDto> RecentTransactions { get; } = new();
    public ObservableCollection<CategorySpendingSummary> SpendingByCategory { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }

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

            SpendingByCategory.Clear();
            foreach (var s in dto.SpendingByCategory) SpendingByCategory.Add(s);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
