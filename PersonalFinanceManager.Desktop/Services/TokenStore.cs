using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PersonalFinanceManager.Application.Contracts.Auth;

namespace PersonalFinanceManager.Desktop.Services;

/// <summary>
/// Persists the JWT token to disk using Windows DPAPI (ProtectedData.Protect).
/// The encrypted blob is stored in %AppData%\PFM\token.bin.
/// </summary>
public class TokenStore
{
    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PFM", "token.bin");

    private AuthResult? _cached;

    /// <summary>Saves the auth result to disk (encrypted) and caches it in memory.</summary>
    public void Save(AuthResult result)
    {
        _cached = result;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var json = JsonSerializer.Serialize(result);
        var plain = Encoding.UTF8.GetBytes(json);
        var cipher = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_path, cipher);
    }

    /// <summary>Loads the auth result from disk. Returns null if not present or expired.</summary>
    public AuthResult? Load()
    {
        if (_cached is not null && _cached.Expiration > DateTime.UtcNow.AddMinutes(1))
            return _cached;

        if (!File.Exists(_path)) return null;

        try
        {
            var cipher = File.ReadAllBytes(_path);
            var plain = ProtectedData.Unprotect(cipher, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(plain);
            var result = JsonSerializer.Deserialize<AuthResult>(json);

            if (result is null || result.Expiration <= DateTime.UtcNow.AddMinutes(1))
            {
                Clear();
                return null;
            }

            _cached = result;
            return result;
        }
        catch
        {
            Clear();
            return null;
        }
    }

    /// <summary>Returns the bearer token string if valid, null otherwise.</summary>
    public string? GetToken() => Load()?.Token;

    /// <summary>Clears the cached token and deletes the on-disk file.</summary>
    public void Clear()
    {
        _cached = null;
        if (File.Exists(_path)) File.Delete(_path);
    }
}
