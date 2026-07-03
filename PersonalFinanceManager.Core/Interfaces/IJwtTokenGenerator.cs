using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Core.Interfaces;

/// <summary>
/// Issues signed JWT access tokens for authenticated users. Implemented in Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
	/// <summary>
	/// Generates a signed token for the given user and returns it together with
	/// its UTC expiry.
	/// </summary>
	(string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
