namespace PersonalFinanceManager.Mobile.Services;

public class ConnectivityService : IConnectivityService
{
    private readonly IApiClient _api;
    private bool _online = false;

    public ConnectivityService(IApiClient api) => _api = api;

    public bool IsOnline => _online;

    public async Task<bool> CheckAsync(CancellationToken ct = default)
    {
        _online = await _api.PingAsync(ct);
        return _online;
    }
}
