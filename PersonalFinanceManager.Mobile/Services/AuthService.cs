using PersonalFinanceManager.Application.Contracts.Auth;

namespace PersonalFinanceManager.Mobile.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _api;
    private readonly TokenStore _store;

    public AuthService(IApiClient api, TokenStore store)
    {
        _api = api;
        _store = store;
    }

    public AuthResult? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null && CurrentUser.Expiration > DateTime.UtcNow;

    public async Task InitializeAsync(CancellationToken ct = default)
        => CurrentUser = await _store.LoadAsync();

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var result = await _api.LoginAsync(new LoginRequest { Email = email, Password = password }, ct);
        await _store.SaveAsync(result);
        CurrentUser = result;
        return result;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var result = await _api.RegisterAsync(request, ct);
        await _store.SaveAsync(result);
        CurrentUser = result;
        return result;
    }

    public void Logout()
    {
        CurrentUser = null;
        _store.Clear();
    }
}
