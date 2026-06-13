using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceManager.Web.Services;

namespace PersonalFinanceManager.Web.Auth;

/// <summary>
/// Custom AuthenticationStateProvider that reads the JWT from local storage
/// and exposes the user's claims to the Blazor component tree.
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
	private readonly ILocalStorageService _localStorage;
	private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

	public JwtAuthStateProvider(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		try
		{
			var token = await _localStorage.GetItemAsync("authToken");

			if (string.IsNullOrWhiteSpace(token))
				return new AuthenticationState(_anonymous);

			token = token.Trim('"');

			var handler = new JwtSecurityTokenHandler();

			if (!handler.CanReadToken(token))
				return new AuthenticationState(_anonymous);

			var jwtToken = handler.ReadJwtToken(token);

			if (jwtToken.ValidTo < DateTime.UtcNow)
			{
				await _localStorage.RemoveItemAsync("authToken");
				return new AuthenticationState(_anonymous);
			}

			var claims = jwtToken.Claims.ToList();

			if (!claims.Any(c => c.Type == ClaimTypes.Name))
			{
				var emailClaim = claims.FirstOrDefault(c =>
					c.Type == "email" || c.Type == ClaimTypes.Email);
				if (emailClaim != null)
					claims.Add(new Claim(ClaimTypes.Name, emailClaim.Value));
			}

			var identity = new ClaimsIdentity(claims, "jwt");
			var user = new ClaimsPrincipal(identity);

			return new AuthenticationState(user);
		}
		catch
		{
			return new AuthenticationState(_anonymous);
		}
	}

	public void NotifyUserAuthentication(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		var jwtToken = handler.ReadJwtToken(token);
		var claims = jwtToken.Claims.ToList();

		if (!claims.Any(c => c.Type == ClaimTypes.Name))
		{
			var emailClaim = claims.FirstOrDefault(c =>
				c.Type == "email" || c.Type == ClaimTypes.Email);
			if (emailClaim != null)
				claims.Add(new Claim(ClaimTypes.Name, emailClaim.Value));
		}

		var identity = new ClaimsIdentity(claims, "jwt");
		var user = new ClaimsPrincipal(identity);

		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
	}

	public void NotifyUserLogout()
	{
		NotifyAuthenticationStateChanged(
			Task.FromResult(new AuthenticationState(_anonymous)));
	}
}
