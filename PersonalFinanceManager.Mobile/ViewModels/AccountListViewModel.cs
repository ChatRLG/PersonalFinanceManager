using System.Collections.ObjectModel;
using PersonalFinanceManager.Application.Contracts.Accounts;
using PersonalFinanceManager.Mobile.Services;

namespace PersonalFinanceManager.Mobile.ViewModels;

public class AccountListViewModel : ViewModelBase
{
    private readonly IApiClient _api;

    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private AccountDto? _selectedAccount;

    // Create form fields
    private string _newName = string.Empty;
    private string _newType = "Checking";
    private string _newCurrency = "USD";
    private decimal _newInitialBalance;
    private string? _newDescription;
    private bool _showCreateForm;

    public AccountListViewModel(IApiClient api)
    {
        _api = api;
        LoadCommand = new AsyncRelayCommand(() => LoadAsync());
        CreateCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(NewName));
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedAccount is not null && !IsBusy);
        ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => SelectedAccount is not null && !IsBusy);
        ShowCreateFormCommand = new RelayCommand(() => ShowCreateForm = true);
        CancelCreateCommand = new RelayCommand(() => { ShowCreateForm = false; ResetForm(); });
    }

    public ObservableCollection<AccountDto> Accounts { get; } = new();
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public AccountDto? SelectedAccount { get => _selectedAccount; set => SetProperty(ref _selectedAccount, value); }
    public bool ShowCreateForm { get => _showCreateForm; set => SetProperty(ref _showCreateForm, value); }

    // Form bindings
    public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
    public string NewType { get => _newType; set => SetProperty(ref _newType, value); }
    public string NewCurrency { get => _newCurrency; set => SetProperty(ref _newCurrency, value); }
    public decimal NewInitialBalance { get => _newInitialBalance; set => SetProperty(ref _newInitialBalance, value); }
    public string? NewDescription { get => _newDescription; set => SetProperty(ref _newDescription, value); }

    public IReadOnlyList<string> AccountTypes { get; } = new[] { "Checking", "Savings", "CreditCard", "Investment", "Cash" };
    public IReadOnlyList<string> Currencies { get; } = new[] { "USD", "EUR", "GBP", "AUD", "CAD" };

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public AsyncRelayCommand ToggleActiveCommand { get; }
    public RelayCommand ShowCreateFormCommand { get; }
    public RelayCommand CancelCreateCommand { get; }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var list = await _api.GetAccountsAsync(ct);
            Accounts.Clear();
            foreach (var a in list) Accounts.Add(a);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task CreateAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var req = new CreateAccountRequest
            {
                Name = NewName,
                Type = NewType,
                Currency = NewCurrency,
                InitialBalance = NewInitialBalance,
                Description = NewDescription
            };
            var created = await _api.CreateAccountAsync(req);
            Accounts.Add(created);
            ShowCreateForm = false;
            ResetForm();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync()
    {
        if (SelectedAccount is null) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _api.DeleteAccountAsync(SelectedAccount.Id);
            Accounts.Remove(SelectedAccount);
            SelectedAccount = null;
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedAccount is null) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            if (SelectedAccount.IsActive)
                await _api.DeactivateAccountAsync(SelectedAccount.Id);
            else
                await _api.ActivateAccountAsync(SelectedAccount.Id);

            await LoadAsync(); // refresh
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private void ResetForm()
    {
        NewName = string.Empty;
        NewType = "Checking";
        NewCurrency = "USD";
        NewInitialBalance = 0m;
        NewDescription = null;
    }
}
