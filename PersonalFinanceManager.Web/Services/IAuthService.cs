using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IAuthService
{
	Task<ApiResult<AuthResponseModel>> LoginAsync(LoginModel model);
	Task<ApiResult<AuthResponseModel>> RegisterAsync(RegisterModel model);
	Task LogoutAsync();
}