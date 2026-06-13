using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class CategoryService : ICategoryService
{
	private readonly IApiClient _api;

	public CategoryService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResponse<List<CategoryDto>>> GetAllAsync()
		=> await _api.GetAsync<List<CategoryDto>>("api/categories");

	public async Task<ApiResponse<List<CategoryDto>>> GetByTypeAsync(string type)
		=> await _api.GetAsync<List<CategoryDto>>($"api/categories?type={type}");

	public async Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id)
		=> await _api.GetAsync<CategoryDto>($"api/categories/{id}");

	public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryModel model)
		=> await _api.PostAsync<CategoryDto>("api/categories", model);

	public async Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, CreateCategoryModel model)
		=> await _api.PutAsync<CategoryDto>($"api/categories/{id}", model);

	public async Task<ApiResponse> DeleteAsync(Guid id)
		=> await _api.DeleteAsync($"api/categories/{id}");
}