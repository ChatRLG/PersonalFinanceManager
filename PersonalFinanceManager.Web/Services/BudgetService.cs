using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class BudgetService : IBudgetService
{
	private readonly IApiClient _api;

	public BudgetService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResult<List<BudgetDto>>> GetAllAsync()
	{
		return await _api.GetAsync<List<BudgetDto>>("api/budgets");
	}

	public async Task<ApiResult<BudgetDto>> CreateAsync(CreateBudgetModel model)
	{
		return await _api.PostAsync<CreateBudgetModel, BudgetDto>("api/budgets", model);
	}

	public async Task<ApiResult> DeleteAsync(Guid id)
	{
		return await _api.DeleteAsync($"api/budgets/{id}");
	}

	public async Task<ApiResult<List<BudgetDto>>> GetActiveAsync()
	{
		return await _api.GetAsync<List<BudgetDto>>("api/budgets/active");
	}

	public async Task<ApiResult<BudgetDto>> GetByIdAsync(Guid id)
	{
		return await _api.GetAsync<BudgetDto>($"api/budgets/{id}");
	}

	public async Task<ApiResult<BudgetDto>> UpdateAsync(Guid id, UpdateBudgetModel model)
	{
		return await _api.PutAsync<UpdateBudgetModel, BudgetDto>($"api/budgets/{id}", model);
	}
}