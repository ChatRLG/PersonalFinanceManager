using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IAccountService
{
	Task<ApiResponse<List<AccountDto>>> GetAllAsync();
	Task<ApiResponse<AccountDto>> GetByIdAsync(Guid id);
	Task<ApiResponse<AccountDto>> CreateAsync(CreateAccountModel model);
	Task<ApiResponse<AccountDto>> UpdateAsync(Guid id, UpdateAccountModel model);
	Task<ApiResponse> DeactivateAsync(Guid id);
	Task<ApiResponse> ActivateAsync(Guid id);
	Task<ApiResponse> DeleteAsync(Guid id);
}