using PersonalFinanceManager.Application.Contracts.Auth;

namespace PersonalFinanceManager.Mobile.Services;

public interface IAuthService
{
    AuthResult? CurrentUser { get; }
    bool IsAuthenticated { get; }

    /// <summary>Loads any previously persisted token from secure storage. Call once at app startup.</summary>
    Task InitializeAsync(CancellationToken ct = default);

    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    void Logout();
}
