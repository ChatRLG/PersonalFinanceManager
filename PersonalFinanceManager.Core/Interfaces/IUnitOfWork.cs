namespace PersonalFinanceManager.Core.Interfaces;

/// <summary>
/// Coordinates multiple repository operations under a single database transaction.
/// Call SaveChangesAsync() once after performing all operations.
/// </summary>
public interface IUnitOfWork : IDisposable
{
	IUserRepository Users { get; }
	IAccountRepository Accounts { get; }
	ITransactionRepository Transactions { get; }
	ICategoryRepository Categories { get; }
	IBudgetRepository Budgets { get; }

	/// <summary>
	/// Persists all pending changes to the database in a single transaction.
	/// Returns the number of rows affected.
	/// </summary>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>Starts an explicit database transaction.</summary>
	Task BeginTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>Commits the current transaction.</summary>
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>Rolls back the current transaction.</summary>
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}