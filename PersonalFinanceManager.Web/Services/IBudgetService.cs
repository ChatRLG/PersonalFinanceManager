using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IBudgetService
{
	Task<ApiResult<List<BudgetDto>>> GetAllAsync();
	Task<ApiResult<List<BudgetDto>>> GetActiveAsync();
	Task<ApiResult<BudgetDto>> GetByIdAsync(Guid id);
	Task<ApiResult<BudgetDto>> CreateAsync(CreateBudgetModel model);
	Task<ApiResult<BudgetDto>> UpdateAsync(Guid id, UpdateBudgetModel model);
	Task<ApiResult> DeleteAsync(Guid id);
}