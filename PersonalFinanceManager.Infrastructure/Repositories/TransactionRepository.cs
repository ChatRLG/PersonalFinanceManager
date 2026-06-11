using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
	public TransactionRepository(AppDBContext context) : base(context) { }

	public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(t => t.AccountId == accountId)
			.Include(t => t.Category)
			.OrderByDescending(t => t.TransactionDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(
		Guid accountId,
		DateTime startDate,
		DateTime endDate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(t => t.AccountId == accountId
					 && t.TransactionDate >= startDate
					 && t.TransactionDate <= endDate)
			.Include(t => t.Category)
			.OrderByDescending(t => t.TransactionDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(t => t.CategoryId == categoryId)
			.Include(t => t.Account)
			.OrderByDescending(t => t.TransactionDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Transaction>> GetByTypeAsync(
		Guid accountId,
		TransactionType type,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(t => t.AccountId == accountId && t.Type == type)
			.Include(t => t.Category)
			.OrderByDescending(t => t.TransactionDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<decimal> GetTotalByTypeAndDateRangeAsync(
		Guid accountId,
		TransactionType type,
		DateTime startDate,
		DateTime endDate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(t => t.AccountId == accountId
					 && t.Type == type
					 && t.TransactionDate >= startDate
					 && t.TransactionDate <= endDate)
			.SumAsync(t => t.Amount, cancellationToken);
	}

	public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetPaginatedAsync(
		Guid accountId,
		int pageNumber,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		var query = DbSet
			.Where(t => t.AccountId == accountId)
			.Include(t => t.Category)
			.OrderByDescending(t => t.TransactionDate);

		var totalCount = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (items, totalCount);
	}
}
