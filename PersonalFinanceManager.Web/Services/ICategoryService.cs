using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface ICategoryService
{
	Task<ApiResult<List<CategoryDto>>> GetAllAsync();
	Task<ApiResult<CategoryDto>> CreateAsync(CreateCategoryModel model);
	Task<ApiResult> DeleteAsync(Guid id);
}


