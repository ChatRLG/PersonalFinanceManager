using System.Net.Http;
using System.Net.Http.Headers;

namespace PersonalFinanceManager.Desktop.Services;

/// <summary>
/// DelegatingHandler that injects the stored bearer token into every outgoing API request.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _store;

    public AuthTokenHandler(TokenStore store) => _store = store;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _store.GetToken();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
