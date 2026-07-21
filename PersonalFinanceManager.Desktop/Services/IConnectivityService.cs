namespace PersonalFinanceManager.Desktop.Services;

public interface IConnectivityService
{
    bool IsOnline { get; }
    Task<bool> CheckAsync(CancellationToken ct = default);
}
