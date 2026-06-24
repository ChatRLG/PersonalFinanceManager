using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class AccountService : IAccountService
{
	private readonly IApiClient _api;

	public AccountService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResult<List<AccountDto>>> GetAllAsync()
	{
		return await _api.GetAsync<List<AccountDto>>("api/accounts");
	}

	public async Task<ApiResult<AccountDto>> GetByIdAsync(Guid id)
	{
		return await _api.GetAsync<AccountDto>($"api/accounts/{id}");
	}

	public async Task<ApiResult<AccountDto>> CreateAsync(CreateAccountModel model)
	{
		return await _api.PostAsync<CreateAccountModel, AccountDto>("api/accounts", model);
	}

	public async Task<ApiResult> DeleteAsync(Guid id)
	{
		return await _api.DeleteAsync($"api/accounts/{id}");
	}

	public async Task<ApiResult> DeactivateAsync(Guid id)
	{
		return await _api.PutAsync($"api/accounts/{id}/deactivate");
	}

	public async Task<ApiResult> ActivateAsync(Guid id)
	{
		return await _api.PutAsync($"api/accounts/{id}/activate");
	}
}