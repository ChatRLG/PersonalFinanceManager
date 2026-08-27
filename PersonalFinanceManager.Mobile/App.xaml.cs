using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceManager.Mobile.Data;
using PersonalFinanceManager.Mobile.Services;
using PersonalFinanceManager.Mobile.Views;

namespace PersonalFinanceManager.Mobile;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		_services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(_services.GetRequiredService<AppShell>());
		window.Created += (_, _) => _ = InitializeAsync();
		return window;
	}

	/// <summary>
	/// Creates the local offline DB (see docs/ROADMAP.md Phase 6a for the
	/// EnsureCreatedAsync-vs-migrations trade-off) and checks for a persisted,
	/// still-valid JWT so a returning user skips the Login page — an enhancement
	/// over Desktop, which always shows Login first, made cheap by SecureStorage
	/// already persisting the token across launches.
	/// </summary>
	private async Task InitializeAsync()
	{
		using (var scope = _services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
			await db.Database.EnsureCreatedAsync();
		}

		var auth = _services.GetRequiredService<IAuthService>();
		await auth.InitializeAsync();

		if (!auth.IsAuthenticated)
			await Shell.Current.GoToAsync("//LoginPage");
		// else: stay on the TabBar's default (DashboardPage) tab.
	}
}
