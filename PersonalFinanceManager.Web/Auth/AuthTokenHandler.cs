using System.Net.Http.Headers;
using PersonalFinanceManager.Web.Services;

namespace PersonalFinanceManager.Web.Auth;

/// <summary>
/// DelegatingHandler that attaches the JWT token to every outgoing HTTP request.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
	private readonly ILocalStorageService _localStorage;

	public AuthTokenHandler(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var token = await _localStorage.GetItemAsync("authToken");

		if (!string.IsNullOrWhiteSpace(token))
		{
			token = token.Trim('"');
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		return await base.SendAsync(request, cancellationToken);
	}
}