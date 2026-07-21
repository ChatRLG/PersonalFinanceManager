using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceManager.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
