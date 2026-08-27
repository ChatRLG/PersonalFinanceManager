using PersonalFinanceManager.Mobile.ViewModels;

namespace PersonalFinanceManager.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        vm.LoginSucceeded += OnLoginSucceeded;
    }

    private async void OnLoginSucceeded()
        => await Shell.Current.GoToAsync("//DashboardPage");
}
