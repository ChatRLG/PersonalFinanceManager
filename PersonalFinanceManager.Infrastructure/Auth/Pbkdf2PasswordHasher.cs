using System.Security.Cryptography;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Infrastructure.Auth;

/// <summary>
/// Password hasher using PBKDF2 (Rfc2898) with HMAC-SHA256. No external dependency.
/// Stored format: "{iterations}.{saltBase64}.{hashBase64}".
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
	private const int SaltSize = 16;        // 128-bit salt
	private const int KeySize = 32;         // 256-bit subkey
	private const int Iterations = 100_000;
	private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
	private const char Delimiter = '.';

	public string Hash(string password)
	{
		if (string.IsNullOrWhiteSpace(password))
			throw new ArgumentException("Password cannot be empty.", nameof(password));

		var salt = RandomNumberGenerator.GetBytes(SaltSize);
		var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

		return string.Join(Delimiter,
			Iterations,
			Convert.ToBase64String(salt),
			Convert.ToBase64String(hash));
	}

	public bool Verify(string password, string hash)
	{
		if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
			return false;

		var parts = hash.Split(Delimiter);
		if (parts.Length != 3)
			return false;

		if (!int.TryParse(parts[0], out var iterations))
			return false;

		byte[] salt, expected;
		try
		{
			salt = Convert.FromBase64String(parts[1]);
			expected = Convert.FromBase64String(parts[2]);
		}
		catch (FormatException)
		{
			return false;
		}

		var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

		// Constant-time comparison to avoid timing attacks.
		return CryptographicOperations.FixedTimeEquals(actual, expected);
	}
}
