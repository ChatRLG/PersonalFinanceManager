using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
	/// <summary>Returns all transactions for a specific account.</summary>
	Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

	/// <summary>Returns all transactions within a date range for a specific account.</summary>
	Task<IEnumerable<Transaction>> GetByDateRangeAsync(
		Guid accountId,
		DateTime startDate,
		DateTime endDate,
		CancellationToken cancellationToken = default);

	/// <summary>Returns all transactions for a specific category.</summary>
	Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

	/// <summary>Returns all transactions of a specific type for an account.</summary>
	Task<IEnumerable<Transaction>> GetByTypeAsync(
		Guid accountId,
		TransactionType type,
		CancellationToken cancellationToken = default);

	/// <summary>Returns the sum of all transactions of a given type within a date range.</summary>
	Task<decimal> GetTotalByTypeAndDateRangeAsync(
		Guid accountId,
		TransactionType type,
		DateTime startDate,
		DateTime endDate,
		CancellationToken cancellationToken = default);

	/// <summary>Returns paginated transactions for an account (newest first).</summary>
	Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPaginatedAsync(
		Guid accountId,
		int pageNumber,
		int pageSize,
		CancellationToken cancellationToken = default);
}