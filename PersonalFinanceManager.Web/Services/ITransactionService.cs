using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface ITransactionService
{
    /// <summary>Returns a page of transactions, optionally filtered by account.</summary>
    Task<ApiResult<PagedResultDto<TransactionDto>>> GetPagedAsync(Guid? accountId = null, int page = 1, int pageSize = 20);

    /// <summary>Returns all transactions for a single account (unpaged — uses /account/{id} endpoint).</summary>
    Task<ApiResult<List<TransactionDto>>> GetByAccountAsync(Guid accountId);

    Task<ApiResult<List<TransactionDto>>> GetRecentAsync(int count = 10);
    Task<ApiResult<TransactionDto>> GetByIdAsync(Guid id);
    Task<ApiResult<TransactionDto>> CreateAsync(CreateTransactionModel model);
    Task<ApiResult> DeleteAsync(Guid id);
}
