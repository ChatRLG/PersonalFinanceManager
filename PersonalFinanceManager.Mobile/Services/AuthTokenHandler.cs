using System.Net.Http;
using System.Net.Http.Headers;

namespace PersonalFinanceManager.Mobile.Services;

/// <summary>
/// DelegatingHandler that injects the stored bearer token into every outgoing API request.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _store;

    public AuthTokenHandler(TokenStore store) => _store = store;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _store.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
