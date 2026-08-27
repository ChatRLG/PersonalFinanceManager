using PersonalFinanceManager.Mobile.ViewModels;

namespace PersonalFinanceManager.Mobile.Views;

public partial class BudgetListPage : ContentPage
{
    private readonly BudgetListViewModel _vm;

    public BudgetListPage(BudgetListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
