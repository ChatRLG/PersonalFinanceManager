using PersonalFinanceManager.Desktop.Services;

namespace PersonalFinanceManager.Desktop.ViewModels;

/// <summary>
/// Shell ViewModel: owns the active page ViewModel and nav commands.
/// The MainWindow ContentControl binds to CurrentPage.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    private readonly IAuthService _auth;
    private readonly IConnectivityService _connectivity;

    private ViewModelBase? _currentPage;
    private string _statusMessage = string.Empty;
    private bool _isOnline;

    public MainViewModel(IServiceProvider sp, IAuthService auth, IConnectivityService connectivity)
    {
        _sp = sp;
        _auth = auth;
        _connectivity = connectivity;

        NavDashboardCommand = new AsyncRelayCommand(NavDashboardAsync);
        NavAccountsCommand = new AsyncRelayCommand(NavAccountsAsync);
        NavTransactionsCommand = new AsyncRelayCommand(NavTransactionsAsync);
        NavBudgetsCommand = new AsyncRelayCommand(NavBudgetsAsync);
        NavReportsCommand = new AsyncRelayCommand(NavReportsAsync);
        LogoutCommand = new RelayCommand(Logout);

        // Default page on load.
        _ = NavDashboardAsync();
        _ = CheckConnectivityAsync();
    }

    public ViewModelBase? CurrentPage { get => _currentPage; private set => SetProperty(ref _currentPage, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsOnline { get => _isOnline; set => SetProperty(ref _isOnline, value); }
    public string UserName => _auth.CurrentUser is { } u ? $"{u.FirstName} {u.LastName}" : string.Empty;

    public AsyncRelayCommand NavDashboardCommand { get; }
    public AsyncRelayCommand NavAccountsCommand { get; }
    public AsyncRelayCommand NavTransactionsCommand { get; }
    public AsyncRelayCommand NavBudgetsCommand { get; }
    public AsyncRelayCommand NavReportsCommand { get; }
    public RelayCommand LogoutCommand { get; }

    private async Task NavDashboardAsync()
    {
        var vm = (DashboardViewModel)_sp.GetService(typeof(DashboardViewModel))!;
        CurrentPage = vm;
        await vm.LoadAsync();
    }

    private async Task NavAccountsAsync()
    {
        var vm = (AccountListViewModel)_sp.GetService(typeof(AccountListViewModel))!;
        CurrentPage = vm;
        await vm.LoadAsync();
    }

    private async Task NavTransactionsAsync()
    {
        var vm = (TransactionListViewModel)_sp.GetService(typeof(TransactionListViewModel))!;
        CurrentPage = vm;
        await vm.LoadAsync();
    }

    private async Task NavBudgetsAsync()
    {
        var vm = (BudgetListViewModel)_sp.GetService(typeof(BudgetListViewModel))!;
        CurrentPage = vm;
        await vm.LoadAsync();
    }

    private async Task NavReportsAsync()
    {
        var vm = (ReportsViewModel)_sp.GetService(typeof(ReportsViewModel))!;
        CurrentPage = vm;
        await vm.LoadAsync();
    }

    private void Logout()
    {
        _auth.Logout();
        // Restart the app to return to the login screen.
        System.Windows.Application.Current.Shutdown();
    }

    private async Task CheckConnectivityAsync()
    {
        IsOnline = await _connectivity.CheckAsync();
        StatusMessage = IsOnline ? "Connected" : "Offline — changes will sync when reconnected";
    }
}
