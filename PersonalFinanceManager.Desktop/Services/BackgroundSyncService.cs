using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PersonalFinanceManager.Desktop.Services;

/// <summary>
/// Background service that runs a sync cycle every 60 seconds.
/// Uses a scoped <see cref="ISyncService"/> so each cycle gets its own
/// <see cref="Data.OfflineDbContext"/> (scoped EF context).
/// </summary>
public class BackgroundSyncService : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

    private readonly IServiceProvider _sp;
    private readonly ILogger<BackgroundSyncService> _logger;

    public BackgroundSyncService(IServiceProvider sp, ILogger<BackgroundSyncService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small initial delay so the UI can load before the first sync attempt.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var sync = scope.ServiceProvider.GetRequiredService<ISyncService>();
                await sync.SyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background sync cycle failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
