using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the user with all their accounts eagerly loaded.
	/// </summary>
	Task<User?> GetWithAccountsAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the user with accounts, categories, and budgets eagerly loaded.
	/// </summary>
	Task<User?> GetWithFullProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}