namespace PersonalFinanceManager.Core.Interfaces;

/// <summary>
/// Hashes and verifies user passwords. Implemented in Infrastructure.
/// </summary>
public interface IPasswordHasher
{
	/// <summary>Produces a salted hash suitable for storage.</summary>
	string Hash(string password);

	/// <summary>Verifies a plain-text password against a stored hash.</summary>
	bool Verify(string password, string hash);
}
