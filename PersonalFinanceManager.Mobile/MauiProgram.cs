using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalFinanceManager.Mobile.Data;
using PersonalFinanceManager.Mobile.Services;
using PersonalFinanceManager.Mobile.ViewModels;
using PersonalFinanceManager.Mobile.Views;

namespace PersonalFinanceManager.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		ConfigureServices(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		// ── API client ───────────────────────────────────────────────────────
		// 10.0.2.2 is the Android emulator's special-cased loopback to the host
		// machine's localhost. A physical device needs the host's LAN IP instead.
		const string apiBase = "http://10.0.2.2:5122/";

		services.AddSingleton<TokenStore>();
		services.AddTransient<AuthTokenHandler>();

		services.AddHttpClient<IApiClient, ApiClient>(c =>
		{
			c.BaseAddress = new Uri(apiBase);
			c.Timeout = TimeSpan.FromSeconds(30);
		})
		.AddHttpMessageHandler<AuthTokenHandler>();

		// ── Auth + connectivity ──────────────────────────────────────────────
		services.AddSingleton<IAuthService, AuthService>();
		services.AddSingleton<IConnectivityService, ConnectivityService>();

		// ── Offline store ────────────────────────────────────────────────────
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "offline.db");
		services.AddDbContext<OfflineDbContext>(opt =>
			opt.UseSqlite($"Data Source={dbPath}"));

		services.AddScoped<IOfflineTransactionRepository, OfflineTransactionRepository>();

		// ── Sync ─────────────────────────────────────────────────────────────
		services.AddScoped<ISyncService, SyncService>();
		services.AddHostedService<BackgroundSyncService>();

		// ── ViewModels ───────────────────────────────────────────────────────
		services.AddTransient<LoginViewModel>();
		services.AddTransient<DashboardViewModel>();
		services.AddTransient<AccountListViewModel>();
		services.AddTransient<TransactionListViewModel>();
		services.AddTransient<BudgetListViewModel>();
		services.AddTransient<ReportsViewModel>();

		// ── Pages (constructor-injected, resolved by Shell via DI) ──────────
		services.AddTransient<LoginPage>();
		services.AddTransient<DashboardPage>();
		services.AddTransient<AccountListPage>();
		services.AddTransient<TransactionListPage>();
		services.AddTransient<BudgetListPage>();
		services.AddTransient<ReportsPage>();
		services.AddSingleton<AppShell>();
	}
}
