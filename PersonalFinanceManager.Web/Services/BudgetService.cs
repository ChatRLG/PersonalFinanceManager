using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class BudgetService : IBudgetService
{
	private readonly IApiClient _api;

	public BudgetService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResponse<List<BudgetDto>>> GetAllAsync()
		=> await _api.GetAsync<List<BudgetDto>>("api/budgets");

	public async Task<ApiResponse<List<BudgetDto>>> GetActiveAsync()
		=> await _api.GetAsync<List<BudgetDto>>("api/budgets?active=true");

	public async Task<ApiResponse<BudgetDto>> GetByIdAsync(Guid id)
		=> await _api.GetAsync<BudgetDto>($"api/budgets/{id}");

	public async Task<ApiResponse<BudgetDto>> CreateAsync(CreateBudgetModel model)
		=> await _api.PostAsync<BudgetDto>("api/budgets", model);

	public async Task<ApiResponse<BudgetDto>> UpdateAsync(Guid id, UpdateBudgetModel model)
		=> await _api.PutAsync<BudgetDto>($"api/budgets/{id}", model);

	public async Task<ApiResponse> DeleteAsync(Guid id)
		=> await _api.DeleteAsync($"api/budgets/{id}");
}