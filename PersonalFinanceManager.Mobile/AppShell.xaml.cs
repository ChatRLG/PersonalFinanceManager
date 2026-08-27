using PersonalFinanceManager.Mobile.Views;

namespace PersonalFinanceManager.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// LoginPage is deliberately not part of the TabBar — it's a full-screen
		// route navigated to explicitly (from App.InitializeAsync when no valid
		// token is persisted, or after Logout).
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
	}
}
