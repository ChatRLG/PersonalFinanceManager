using Microsoft.EntityFrameworkCore.Storage;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;          
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PersonalFinanceManager.Infrastructure;

/// <summary>
/// Coordinates multiple repository operations under a single database transaction.
/// Lazily initialises repositories on first access.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly AppDBContext _context;
	private IDbContextTransaction? _currentTransaction;
	private bool _disposed;

	private IUserRepository? _users;
	private IAccountRepository? _accounts;
	private ITransactionRepository? _transactions;
	private ICategoryRepository? _categories;
	private IBudgetRepository? _budgets;

	public UnitOfWork(AppDBContext context)
	{
		_context = context;
	}

	// ──────────────────── Repository Accessors  ───────────────────

	public IUserRepository Users => _users ??= new UserRepository(_context);

	public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);

	public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);

	public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

	public IBudgetRepository Budgets => _budgets ??= new BudgetRepository(_context);

	// ──────────────────── Persistence ───────────────────────────────────

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}

	// ──────────────────── Explicit Transaction Support ──────────────────

	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_currentTransaction is not null)
		{
			throw new InvalidOperationException(
				"A transaction is already in progress. Commit or rollback the current transaction before starting a new one.");
		}

		_currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_currentTransaction is null)
		{
			throw new InvalidOperationException(
				"No transaction is in progress. Call BeginTransactionAsync first.");
		}

		try
		{
			await _context.SaveChangesAsync(cancellationToken);
			await _currentTransaction.CommitAsync(cancellationToken);
		}
		catch
		{
			await RollbackTransactionAsync(cancellationToken);
			throw;
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_currentTransaction is null)
		{
			throw new InvalidOperationException(
				"No transaction is in progress. Call BeginTransactionAsync first.");
		}

		try
		{
			await _currentTransaction.RollbackAsync(cancellationToken);
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	// ──────────────────── Cleanup ───────────────────────────────────────

	private async Task DisposeTransactionAsync()
	{
		if (_currentTransaction is not null)
		{
			await _currentTransaction.DisposeAsync();
			_currentTransaction = null;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_currentTransaction?.Dispose();
			_context.Dispose();
		}

		_disposed = true;
	}
}
