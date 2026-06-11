using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
{
	public BudgetRepository(AppDBContext context) : base(context) { }

	public async Task<IEnumerable<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(b => b.UserId == userId)
			.Include(b => b.Category)
			.OrderByDescending(b => b.StartDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		return await DbSet
			.Where(b => b.UserId == userId
					 && b.StartDate <= now
					 && b.EndDate >= now)
			.Include(b => b.Category)
			.OrderBy(b => b.EndDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<Budget?> GetByCategoryAndDateAsync(
		Guid userId,
		Guid categoryId,
		DateTime date,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(b => b.UserId == userId
					 && b.CategoryId == categoryId
					 && b.StartDate <= date
					 && b.EndDate >= date)
			.Include(b => b.Category)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<IEnumerable<Budget>> GetExceededBudgetsAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(b => b.UserId == userId
					 && b.CurrentSpend >= b.Limit)
			.Include(b => b.Category)
			.OrderByDescending(b => b.CurrentSpend)
			.ToListAsync(cancellationToken);
	}

	public async Task<Budget?> GetWithCategoryAsync(Guid budgetId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Include(b => b.Category)
			.FirstOrDefaultAsync(b => b.Id == budgetId, cancellationToken);
	}
}
