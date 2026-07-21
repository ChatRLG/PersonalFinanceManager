namespace PersonalFinanceManager.Desktop.Services;

public interface ISyncService
{
    Task SyncAsync(CancellationToken ct = default);
}
