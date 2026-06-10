using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
	public UserRepository(AppDBContext context) : base(context) { }

	public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var normalisedEmail = email.Trim().ToLowerInvariant();

		return await DbSet
			.FirstOrDefaultAsync(u => u.Email == normalisedEmail, cancellationToken);
	}

	public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
	{
		var normalisedEmail = email.Trim().ToLowerInvariant();

		return await DbSet
			.AnyAsync(u => u.Email == normalisedEmail, cancellationToken);
	}

	public async Task<User?> GetWithAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Include(u => u.Accounts)
			.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
	}

	public async Task<User?> GetWithFullProfileAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await DbSet
			.Include(u => u.Accounts)
			.Include(u => u.Categories)
			.Include(u => u.Budgets)
			.ThenInclude(b => b.Category)
			.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
	}
}
