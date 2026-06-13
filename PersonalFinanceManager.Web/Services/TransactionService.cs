using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class TransactionService : ITransactionService
{
	private readonly IApiClient _api;

	public TransactionService(IApiClient api)
	{
		_api = api;
	}

	public async Task<ApiResponse<List<TransactionDto>>> GetAllAsync()
		=> await _api.GetAsync<List<TransactionDto>>("api/transactions");

	public async Task<ApiResponse<List<TransactionDto>>> GetByAccountAsync(Guid accountId)
		=> await _api.GetAsync<List<TransactionDto>>($"api/transactions?accountId={accountId}");

	public async Task<ApiResponse<List<TransactionDto>>> GetRecentAsync(int count = 10)
		=> await _api.GetAsync<List<TransactionDto>>($"api/transactions?count={count}");

	public async Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid id)
		=> await _api.GetAsync<TransactionDto>($"api/transactions/{id}");

	public async Task<ApiResponse<TransactionDto>> CreateAsync(CreateTransactionModel model)
		=> await _api.PostAsync<TransactionDto>("api/transactions", model);

	public async Task<ApiResponse> DeleteAsync(Guid id)
		=> await _api.DeleteAsync($"api/transactions/{id}");
}