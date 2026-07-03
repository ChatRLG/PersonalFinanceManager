using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class TransactionService : ITransactionService
{
    private readonly IApiClient _api;

    public TransactionService(IApiClient api)
    {
        _api = api;
    }

    /// <inheritdoc/>
    public async Task<ApiResult<PagedResultDto<TransactionDto>>> GetPagedAsync(
        Guid? accountId = null, int page = 1, int pageSize = 20)
    {
        var url = $"api/transactions?page={page}&pageSize={pageSize}";
        if (accountId.HasValue)
            url += $"&accountId={accountId.Value}";

        return await _api.GetAsync<PagedResultDto<TransactionDto>>(url);
    }

    /// <inheritdoc/>
    public async Task<ApiResult<List<TransactionDto>>> GetByAccountAsync(Guid accountId)
    {
        // Uses the unpaged /account/{id} endpoint exposed by TransactionsController.
        return await _api.GetAsync<List<TransactionDto>>($"api/transactions/account/{accountId}");
    }

    public async Task<ApiResult<List<TransactionDto>>> GetRecentAsync(int count = 10)
    {
        return await _api.GetAsync<List<TransactionDto>>($"api/transactions/recent?count={count}");
    }

    public async Task<ApiResult<TransactionDto>> GetByIdAsync(Guid id)
    {
        return await _api.GetAsync<TransactionDto>($"api/transactions/{id}");
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
