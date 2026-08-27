using PersonalFinanceManager.Mobile.Services;

namespace PersonalFinanceManager.Mobile.ViewModels;

/// <summary>
/// Handles login. Raises <see cref="LoginSucceeded"/> on success;
/// LoginPage.xaml.cs subscribes and navigates to the main Shell.
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
    }

    public event Action? LoginSucceeded;

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return;
        }

        IsBusy = true;
        try
        {
            await _auth.LoginAsync(Email, Password);
            LoginSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message.Contains("401") || ex.Message.Contains("Unauthorized")
                ? "Invalid email or password."
                : $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
