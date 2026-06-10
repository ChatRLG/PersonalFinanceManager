using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
	/// <summary>Returns all categories owned by a user.</summary>
	Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>Returns categories of a specific type (Income or Expense) for a user.</summary>
	Task<IEnumerable<Category>> GetByTypeAsync(
		Guid userId,
		TransactionType type,
		CancellationToken cancellationToken = default);

	/// <summary>Returns a category with its transactions eagerly loaded.</summary>
	Task<Category?> GetWithTransactionsAsync(Guid categoryId, CancellationToken cancellationToken = default);

	/// <summary>Checks if a category name already exists for a user and transaction type.</summary>
	Task<bool> NameExistsAsync(
		Guid userId,
		string name,
		TransactionType type,
		CancellationToken cancellationToken = default);
}