using PersonalFinanceManager.Application.Auth.Dtos;

namespace PersonalFinanceManager.Application.Auth;

/// <summary>Application service for authentication use-cases.</summary>
public interface IAuthAppService
{
	Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
	Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
