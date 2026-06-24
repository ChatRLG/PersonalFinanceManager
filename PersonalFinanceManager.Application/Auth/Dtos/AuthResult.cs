namespace PersonalFinanceManager.Application.Auth.Dtos;

/// <summary>
/// Result returned on successful register/login. Field shape intentionally
/// matches the Web client's AuthResponseModel so it deserializes directly.
/// </summary>
public class AuthResult
{
	public string Token { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public DateTime Expiration { get; set; }
}
