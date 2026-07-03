using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class CategoryService : ICategoryService
{
	private readonly IApiClient _api;

	public CategoryService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResult<List<CategoryDto>>> GetAllAsync()
	{
		return await _api.GetAsync<List<CategoryDto>>("api/categories");
	}

	public async Task<ApiResult<CategoryDto>> CreateAsync(CreateCategoryModel model)
	{
		return await _api.PostAsync<CreateCategoryModel, CategoryDto>("api/categories", model);
	}

	public async Task<ApiResult> DeleteAsync(Guid id)
	{
		return await _api.DeleteAsync($"api/categories/{id}");
	}
}