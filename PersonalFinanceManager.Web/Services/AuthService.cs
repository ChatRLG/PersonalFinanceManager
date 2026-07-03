using PersonalFinanceManager.Web.Auth;
using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class AuthService : IAuthService
{
	private readonly IApiClient _api;
	private readonly ILocalStorageService _localStorage;
	private readonly JwtAuthStateProvider _authStateProvider;

	public AuthService(
		IApiClient api,
		ILocalStorageService localStorage,
		JwtAuthStateProvider authStateProvider)
	{
		_api = api;
		_localStorage = localStorage;
		_authStateProvider = authStateProvider;
	}

	public async Task<ApiResult<AuthResponseModel>> LoginAsync(LoginModel model)
	{
		var result = await _api.PostAsync<LoginModel, AuthResponseModel>("api/auth/login", model);

		if (result.IsSuccess && result.Data != null)
		{
			await _localStorage.SetItemAsync("authToken", result.Data.Token);
			_authStateProvider.NotifyUserAuthentication(result.Data.Token);
		}

		return result;
	}

	public async Task<ApiResult<AuthResponseModel>> RegisterAsync(RegisterModel model)
	{
		var result = await _api.PostAsync<RegisterModel, AuthResponseModel>("api/auth/register", model);

		if (result.IsSuccess && result.Data != null)
		{
			await _localStorage.SetItemAsync("authToken", result.Data.Token);
			_authStateProvider.NotifyUserAuthentication(result.Data.Token);
		}

		return result;
	}

	public async Task LogoutAsync()
	{
		await _localStorage.RemoveItemAsync("authToken");
		_authStateProvider.NotifyUserLogout();
	}
}