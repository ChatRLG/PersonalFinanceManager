using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
	/// <summary>Returns all budgets for a user.</summary>
	Task<IEnumerable<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>Returns all currently active budgets for a user (within their date range).</summary>
	Task<IEnumerable<Budget>> GetActiveBudgetsAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>Returns the budget for a specific category within a date range.</summary>
	Task<Budget?> GetByCategoryAndDateAsync(
		Guid userId,
		Guid categoryId,
		DateTime date,
		CancellationToken cancellationToken = default);

	/// <summary>Returns all budgets that have been exceeded.</summary>
	Task<IEnumerable<Budget>> GetExceededBudgetsAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>Returns a budget with its category eagerly loaded.</summary>
	Task<Budget?> GetWithCategoryAsync(Guid budgetId, CancellationToken cancellationToken = default);
}