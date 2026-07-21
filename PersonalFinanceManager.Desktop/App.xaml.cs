using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalFinanceManager.Desktop.Data;
using PersonalFinanceManager.Desktop.Services;
using PersonalFinanceManager.Desktop.ViewModels;
using PersonalFinanceManager.Desktop.Views;
using System.IO;
using WpfApp = System.Windows.Application;

namespace PersonalFinanceManager.Desktop;

public partial class App : WpfApp
{
    private IHost? _host;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                      .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
            })
            .ConfigureServices((ctx, services) =>
            {
                ConfigureServices(ctx.Configuration, services);
            })
            .Build();

        await _host.StartAsync();

        // Ensure the local SQLite DB is created / migrated on startup.
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
            await db.Database.MigrateAsync();
        }

        // Show login window; it will launch MainWindow on success.
        var loginVm = _host.Services.GetRequiredService<LoginViewModel>();
        var loginWin = new LoginWindow { DataContext = loginVm };

        loginVm.LoginSucceeded += () =>
        {
            var mainVm = _host.Services.GetRequiredService<MainViewModel>();
            var mainWin = new MainWindow { DataContext = mainVm };
            MainWindow = mainWin;
            mainWin.Show();
            loginWin.Close();
        };

        loginWin.Show();
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static void ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        // ── API client ────────────────────────────────────────────────────────
        var apiBase = config["ApiSettings:BaseUrl"] ?? "http://localhost:5122/";
        services.AddSingleton<TokenStore>();
        services.AddTransient<AuthTokenHandler>();

        services.AddHttpClient<IApiClient, ApiClient>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        // ── Auth + connectivity ────────────────────────────────────────────────
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();

        // ── Offline store ────────────────────────────────────────────────────
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PFM", "offline.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<OfflineDbContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IOfflineTransactionRepository, OfflineTransactionRepository>();

        // ── Sync ─────────────────────────────────────────────────────────────
        services.AddScoped<ISyncService, SyncService>();
        services.AddHostedService<BackgroundSyncService>();

        // ── CSV import/export ─────────────────────────────────────────────────
        services.AddSingleton<ICsvService, CsvService>();

        // ── ViewModels ────────────────────────────────────────────────────────
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AccountListViewModel>();
        services.AddTransient<TransactionListViewModel>();
        services.AddTransient<BudgetListViewModel>();
        services.AddTransient<ReportsViewModel>();
    }
}
