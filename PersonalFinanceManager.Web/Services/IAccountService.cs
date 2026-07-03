using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IAccountService
{
	Task<ApiResult<List<AccountDto>>> GetAllAsync();
	Task<ApiResult<AccountDto>> GetByIdAsync(Guid id);
	Task<ApiResult<AccountDto>> CreateAsync(CreateAccountModel model);
	Task<ApiResult<AccountDto>> UpdateAsync(Guid id, UpdateAccountModel model);
	Task<ApiResult> DeactivateAsync(Guid id);
	Task<ApiResult> ActivateAsync(Guid id);
	Task<ApiResult> DeleteAsync(Guid id);
}