using System.Collections.ObjectModel;
using PersonalFinanceManager.Application.Contracts.Categories;
using PersonalFinanceManager.Application.Contracts.Transactions;
using PersonalFinanceManager.Desktop.Data;
using PersonalFinanceManager.Desktop.Data.Entities;
using PersonalFinanceManager.Desktop.Services;

namespace PersonalFinanceManager.Desktop.ViewModels;

public class TransactionListViewModel : ViewModelBase
{
    private readonly IApiClient _api;
    private readonly IOfflineTransactionRepository _offlineRepo;
    private readonly IConnectivityService _connectivity;

    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount;

    // Create form
    private bool _showCreateForm;
    private decimal _newAmount;
    private string _newType = "Expense";
    private string _newDescription = string.Empty;
    private string? _newNotes;
    private DateTime _newDate = DateTime.Today;
    private Guid _newAccountId;
    private Guid _newCategoryId;
    private Guid? _newDestAccountId;

    public TransactionListViewModel(
        IApiClient api,
        IOfflineTransactionRepository offlineRepo,
        IConnectivityService connectivity)
    {
        _api = api;
        _offlineRepo = offlineRepo;
        _connectivity = connectivity;

        LoadCommand = new AsyncRelayCommand(() => LoadAsync());
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => _currentPage < _totalPages);
        PrevPageCommand = new AsyncRelayCommand(PrevPageAsync, () => _currentPage > 1);
        CreateCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy);
        DeleteCommand = new AsyncRelayCommand<TransactionDto?>(DeleteAsync);
        ShowCreateFormCommand = new RelayCommand(() => ShowCreateForm = true);
        CancelCreateCommand = new RelayCommand(() => { ShowCreateForm = false; ResetForm(); });
    }

    public ObservableCollection<TransactionDto> Transactions { get; } = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new();
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public int CurrentPage { get => _currentPage; private set => SetProperty(ref _currentPage, value); }
    public int TotalPages { get => _totalPages; private set => SetProperty(ref _totalPages, value); }
    public int TotalCount { get => _totalCount; private set => SetProperty(ref _totalCount, value); }
    public bool ShowCreateForm { get => _showCreateForm; set => SetProperty(ref _showCreateForm, value); }

    // Form bindings
    public decimal NewAmount { get => _newAmount; set => SetProperty(ref _newAmount, value); }
    public string NewType { get => _newType; set => SetProperty(ref _newType, value); }
    public string NewDescription { get => _newDescription; set => SetProperty(ref _newDescription, value); }
    public string? NewNotes { get => _newNotes; set => SetProperty(ref _newNotes, value); }
    public DateTime NewDate { get => _newDate; set => SetProperty(ref _newDate, value); }
    public Guid NewAccountId { get => _newAccountId; set => SetProperty(ref _newAccountId, value); }
    public Guid NewCategoryId { get => _newCategoryId; set => SetProperty(ref _newCategoryId, value); }
    public Guid? NewDestAccountId { get => _newDestAccountId; set => SetProperty(ref _newDestAccountId, value); }

    public IReadOnlyList<string> TransactionTypes { get; } = new[] { "Expense", "Income", "Transfer" };

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PrevPageCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }
    public AsyncRelayCommand<TransactionDto?> DeleteCommand { get; }
    public RelayCommand ShowCreateFormCommand { get; }
    public RelayCommand CancelCreateCommand { get; }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _connectivity.CheckAsync(ct);

            // Load categories for the create form.
            if (Categories.Count == 0)
            {
                var cats = await _api.GetCategoriesAsync(ct);
                Categories.Clear();
                foreach (var c in cats) Categories.Add(c);
                if (cats.Count > 0) NewCategoryId = cats[0].Id;
            }

            var paged = await _api.GetTransactionsPagedAsync(page: _currentPage, ct: ct);
            TotalCount = paged.TotalCount;
            TotalPages = paged.TotalPages;

            Transactions.Clear();
            foreach (var t in paged.Items) Transactions.Add(t);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadAsync();
    }

    private async Task PrevPageAsync()
    {
        CurrentPage--;
        await LoadAsync();
    }

    private async Task CreateAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var req = new CreateTransactionRequest
            {
                Amount = NewAmount,
                Type = NewType,
                Description = NewDescription,
                Notes = NewNotes,
                TransactionDate = NewDate,
                AccountId = NewAccountId,
                CategoryId = NewCategoryId,
                DestinationAccountId = NewType == "Transfer" ? NewDestAccountId : null
            };

            if (_connectivity.IsOnline)
            {
                await _api.CreateTransactionAsync(req);
            }
            else
            {
                // Save locally; sync service will push it when online.
                var offline = new OfflineTransaction
                {
                    Amount = req.Amount,
                    Currency = req.Currency,
                    Type = req.Type,
                    Description = req.Description,
                    Notes = req.Notes,
                    TransactionDate = req.TransactionDate,
                    AccountId = req.AccountId,
                    CategoryId = req.CategoryId,
                    DestinationAccountId = req.DestinationAccountId
                };
                await _offlineRepo.AddAsync(offline);
                ErrorMessage = "Saved offline — will sync when reconnected.";
            }

            ShowCreateForm = false;
            ResetForm();
            await LoadAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync(TransactionDto? tx)
    {
        if (tx is null) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _api.DeleteTransactionAsync(tx.Id);
            Transactions.Remove(tx);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private void ResetForm()
    {
        NewAmount = 0m;
        NewType = "Expense";
        NewDescription = string.Empty;
        NewNotes = null;
        NewDate = DateTime.Today;
        NewDestAccountId = null;
    }
}
