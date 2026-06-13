using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IAuthService
{
	Task<ApiResponse<AuthResponseModel>> LoginAsync(LoginModel model);
	Task<ApiResponse<AuthResponseModel>> RegisterAsync(RegisterModel model);
	Task LogoutAsync();
}