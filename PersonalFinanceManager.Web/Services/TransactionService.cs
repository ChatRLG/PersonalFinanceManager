using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class TransactionService : ITransactionService
{
	private readonly IApiClient _api;

	public TransactionService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResult<List<TransactionDto>>> GetAllAsync()
	{
		return await _api.GetAsync<List<TransactionDto>>("api/transactions");
	}

	public async Task<ApiResult<List<TransactionDto>>> GetByAccountAsync(Guid accountId)
	{
		return await _api.GetAsync<List<TransactionDto>>($"api/transactions?accountId={accountId}");
	}

	public async Task<ApiResult<TransactionDto>> CreateAsync(CreateTransactionModel model)
	{
		return await _api.PostAsync<CreateTransactionModel, TransactionDto>("api/transactions", model);
	}

	public async Task<ApiResult> DeleteAsync(Guid id)
	{
		return await _api.DeleteAsync($"api/transactions/{id}");
	}
}