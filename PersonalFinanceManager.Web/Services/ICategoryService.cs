using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface ICategoryService
{
	Task<ApiResponse<List<CategoryDto>>> GetAllAsync();
	Task<ApiResponse<List<CategoryDto>>> GetByTypeAsync(string type);
	Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id);
	Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryModel model);
	Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, CreateCategoryModel model);
	Task<ApiResponse> DeleteAsync(Guid id);
}

