using System.Text.Json;
using Microsoft.Maui.Storage;
using PersonalFinanceManager.Application.Contracts.Auth;

namespace PersonalFinanceManager.Mobile.Services;

/// <summary>
/// Persists the JWT token using MAUI's SecureStorage (Android Keystore-backed).
/// Replaces Desktop's Windows-DPAPI-based TokenStore — same public intent
/// (cache + persist an AuthResult), but async since SecureStorage is async.
/// </summary>
public class TokenStore
{
    private const string Key = "pfm_auth_result";

    private AuthResult? _cached;

    /// <summary>Saves the auth result to secure storage (encrypted) and caches it in memory.</summary>
    public async Task SaveAsync(AuthResult result)
    {
        _cached = result;
        var json = JsonSerializer.Serialize(result);
        await SecureStorage.Default.SetAsync(Key, json);
    }

    /// <summary>Loads the auth result from secure storage. Returns null if not present or expired.</summary>
    public async Task<AuthResult?> LoadAsync()
    {
        if (_cached is not null && _cached.Expiration > DateTime.UtcNow.AddMinutes(1))
            return _cached;

        try
        {
            var json = await SecureStorage.Default.GetAsync(Key);
            if (string.IsNullOrEmpty(json)) return null;

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
    public async Task<string?> GetTokenAsync() => (await LoadAsync())?.Token;

    /// <summary>Clears the cached token and removes it from secure storage.</summary>
    public void Clear()
    {
        _cached = null;
        SecureStorage.Default.Remove(Key);
    }
}
