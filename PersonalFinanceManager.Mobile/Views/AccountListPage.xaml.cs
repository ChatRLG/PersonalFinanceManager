using PersonalFinanceManager.Mobile.ViewModels;

namespace PersonalFinanceManager.Mobile.Views;

public partial class AccountListPage : ContentPage
{
    private readonly AccountListViewModel _vm;

    public AccountListPage(AccountListViewModel vm)
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
