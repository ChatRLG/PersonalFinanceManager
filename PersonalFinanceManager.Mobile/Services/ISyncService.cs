namespace PersonalFinanceManager.Mobile.Services;

public interface ISyncService
{
    Task SyncAsync(CancellationToken ct = default);
}
