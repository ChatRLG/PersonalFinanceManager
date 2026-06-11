using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
	public AccountRepository(AppDBContext context) : base(context) { }

	public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(a => a.UserId == userId)
			.OrderBy(a => a.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<Account?> GetWithTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Include(a => a.Transactions)
			.ThenInclude(t => t.Category)
			.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
	}

	public async Task<IEnumerable<Account>> GetByTypeAsync(Guid userId, AccountType type, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(a => a.UserId == userId && a.Type == type)
			.OrderBy(a => a.Name)
			.ToListAsync(cancellationToken);
	}

	public async Task<decimal> GetTotalBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Where(a => a.UserId == userId && a.IsActive)
			.SumAsync(a => a.Balance, cancellationToken);
	}
}