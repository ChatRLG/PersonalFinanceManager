using PersonalFinanceManager.Mobile.ViewModels;

namespace PersonalFinanceManager.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        vm.LoggedOut += OnLoggedOut;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnLoggedOut()
        => await Shell.Current.GoToAsync("//LoginPage");
}
