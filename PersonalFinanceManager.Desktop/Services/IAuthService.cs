using PersonalFinanceManager.Application.Contracts.Auth;

namespace PersonalFinanceManager.Desktop.Services;

public interface IAuthService
{
    AuthResult? CurrentUser { get; }
    bool IsAuthenticated { get; }

    Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    void Logout();
}
