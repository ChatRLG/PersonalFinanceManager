using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
	public CategoryRepository(AppDBContext context) : base(context) { }

	public async Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(c => c.UserId == userId)
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Category>> GetByTypeAsync(
		Guid userId,
		TransactionType type,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(c => c.UserId == userId && c.Type == type)
			.OrderBy(c => c.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<Category?> GetWithTransactionsAsync(Guid categoryId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Include(c => c.Transactions)
			.ThenInclude(t => t.Account)
			.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
	}

	public async Task<bool> NameExistsAsync(
		Guid userId,
		string name,
		TransactionType type,
		CancellationToken cancellationToken = default)
	{
		var normalisedName = name.Trim();

		return await DbSet
			.AnyAsync(c => c.UserId == userId
			               && c.Name == normalisedName
			               && c.Type == type,
				cancellationToken);
	}
}