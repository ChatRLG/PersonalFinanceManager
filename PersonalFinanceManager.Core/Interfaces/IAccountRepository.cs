using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
	/// <summary>Returns all active (non-deleted) accounts for a given user.</summary>
	Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>Returns an account with its transactions eagerly loaded.</summary>
	Task<Account?> GetWithTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default);

	/// <summary>Returns all accounts of a specific type for a user.</summary>
	Task<IEnumerable<Account>> GetByTypeAsync(Guid userId, AccountType type, CancellationToken cancellationToken = default);

	/// <summary>Returns the total balance across all active accounts for a user.</summary>
	Task<decimal> GetTotalBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
}