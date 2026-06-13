using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface ITransactionService
{
	Task<ApiResponse<List<TransactionDto>>> GetAllAsync();
	Task<ApiResponse<List<TransactionDto>>> GetByAccountAsync(Guid accountId);
	Task<ApiResponse<List<TransactionDto>>> GetRecentAsync(int count = 10);
	Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid id);
	Task<ApiResponse<TransactionDto>> CreateAsync(CreateTransactionModel model);
	Task<ApiResponse> DeleteAsync(Guid id);
}