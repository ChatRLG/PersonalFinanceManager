using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IBudgetService
{
	Task<ApiResponse<List<BudgetDto>>> GetAllAsync();
	Task<ApiResponse<List<BudgetDto>>> GetActiveAsync();
	Task<ApiResponse<BudgetDto>> GetByIdAsync(Guid id);
	Task<ApiResponse<BudgetDto>> CreateAsync(CreateBudgetModel model);
	Task<ApiResponse<BudgetDto>> UpdateAsync(Guid id, UpdateBudgetModel model);
	Task<ApiResponse> DeleteAsync(Guid id);
}