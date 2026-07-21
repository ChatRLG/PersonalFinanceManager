using System.Collections.ObjectModel;
using PersonalFinanceManager.Application.Contracts.Budgets;
using PersonalFinanceManager.Application.Contracts.Categories;
using PersonalFinanceManager.Desktop.Services;

namespace PersonalFinanceManager.Desktop.ViewModels;

public class BudgetListViewModel : ViewModelBase
{
    private readonly IApiClient _api;

    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _showCreateForm;

    // Create form
    private string _newName = string.Empty;
    private decimal _newLimit;
    private string _newCurrency = "USD";
    private string _newPeriod = "Monthly";
    private DateTime _newStartDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _newEndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
    private Guid _newCategoryId;

    public BudgetListViewModel(IApiClient api)
    {
        _api = api;
        LoadCommand = new AsyncRelayCommand(() => LoadAsync());
        CreateCommand = new AsyncRelayCommand(CreateAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(NewName));
        DeleteCommand = new AsyncRelayCommand<BudgetDto?>(DeleteAsync);
        ShowCreateFormCommand = new RelayCommand(() => ShowCreateForm = true);
        CancelCreateCommand = new RelayCommand(() => { ShowCreateForm = false; ResetForm(); });
    }

    public ObservableCollection<BudgetDto> Budgets { get; } = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new();
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public bool ShowCreateForm { get => _showCreateForm; set => SetProperty(ref _showCreateForm, value); }

    // Form bindings
    public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
    public decimal NewLimit { get => _newLimit; set => SetProperty(ref _newLimit, value); }
    public string NewCurrency { get => _newCurrency; set => SetProperty(ref _newCurrency, value); }
    public string NewPeriod { get => _newPeriod; set => SetProperty(ref _newPeriod, value); }
    public DateTime NewStartDate { get => _newStartDate; set => SetProperty(ref _newStartDate, value); }
    public DateTime NewEndDate { get => _newEndDate; set => SetProperty(ref _newEndDate, value); }
    public Guid NewCategoryId { get => _newCategoryId; set => SetProperty(ref _newCategoryId, value); }

    public IReadOnlyList<string> Periods { get; } = new[] { "Daily", "Weekly", "Monthly", "Quarterly", "Yearly", "Custom" };
    public IReadOnlyList<string> Currencies { get; } = new[] { "USD", "EUR", "GBP", "AUD", "CAD" };

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }
    public AsyncRelayCommand<BudgetDto?> DeleteCommand { get; }
    public RelayCommand ShowCreateFormCommand { get; }
    public RelayCommand CancelCreateCommand { get; }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var budgetsTask = _api.GetBudgetsAsync(ct);
            var catsTask = _api.GetCategoriesAsync(ct);
            await Task.WhenAll(budgetsTask, catsTask);

            Budgets.Clear();
            foreach (var b in await budgetsTask) Budgets.Add(b);

            Categories.Clear();
            foreach (var c in await catsTask) Categories.Add(c);
            if (Categories.Count > 0 && NewCategoryId == default)
                NewCategoryId = Categories[0].Id;
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
            var req = new CreateBudgetRequest
            {
                Name = NewName,
                Limit = NewLimit,
                Currency = NewCurrency,
                Period = NewPeriod,
                StartDate = NewStartDate,
                EndDate = NewEndDate,
                CategoryId = NewCategoryId
            };
            var created = await _api.CreateBudgetAsync(req);
            Budgets.Add(created);
            ShowCreateForm = false;
            ResetForm();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync(BudgetDto? budget)
    {
        if (budget is null) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _api.DeleteBudgetAsync(budget.Id);
            Budgets.Remove(budget);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    private void ResetForm()
    {
        NewName = string.Empty;
        NewLimit = 0m;
        NewPeriod = "Monthly";
        NewStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        NewEndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
    }
}
