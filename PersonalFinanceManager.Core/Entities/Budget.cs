using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;

namespace PersonalFinanceManager.Core.Entities;

/// <summary>
/// Represents a spending budget for a specific category over a defined period.
/// Tracks how much has been spent against the budgeted limit.
/// </summary>
public class Budget : BaseEntity
{
	// ──────────────────────────── Properties ────────────────────────────

	/// <summary>A friendly name for this budget (e.g., "Monthly Groceries").</summary>
	public string Name { get; private set; } = string.Empty;

	/// <summary>The maximum amount allowed for this budget period.</summary>
	public decimal Limit { get; private set; }

	/// <summary>
	/// Running total of how much has been spent against this budget.
	/// Updated each time a matching transaction is recorded.
	/// </summary>
	public decimal CurrentSpend { get; private set; }

	/// <summary>The currency for this budget.</summary>
	public CurrencyCode Currency { get; private set; }

	/// <summary>The recurring period for this budget.</summary>
	public BudgetPeriod Period { get; private set; }

	/// <summary>When this budget period starts.</summary>
	public DateTime StartDate { get; private set; }

	/// <summary>When this budget period ends.</summary>
	public DateTime EndDate { get; private set; }

	// ──────────────────────── Foreign Keys / Nav ────────────────────────

	public Guid UserId { get; private set; }
	public User? User { get; private set; }

	/// <summary>The spending category this budget tracks.</summary>
	public Guid CategoryId { get; private set; }
	public Category? Category { get; private set; }

	// ──────────────────────────── Constructors ──────────────────────────

	private Budget() { }

	/// <summary>
	/// Creates a new Budget. Called via User or directly from a service.
	/// </summary>
	public Budget(
		string name,
		decimal limit,
		CurrencyCode currency,
		BudgetPeriod period,
		DateTime startDate,
		DateTime endDate,
		Guid userId,
		Guid categoryId)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Budget name is required.", nameof(name));
		if (limit <= 0)
			throw new ArgumentException("Budget limit must be positive.", nameof(limit));
		if (endDate <= startDate)
			throw new ArgumentException("End date must be after start date.", nameof(endDate));

		Name = name.Trim();
		Limit = limit;
		Currency = currency;
		Period = period;
		StartDate = startDate;
		EndDate = endDate;
		UserId = userId;
		CategoryId = categoryId;
		CurrentSpend = 0;
	}

	// ──────────────────────── Computed Properties ───────────────────────

	/// <summary>How much budget remains.</summary>
	public decimal Remaining => Limit - CurrentSpend;

	/// <summary>Percentage of budget consumed (0–100+).</summary>
	public decimal PercentageUsed => Limit == 0 ? 0 : Math.Round(CurrentSpend / Limit * 100, 2);

	/// <summary>True when spending has reached or exceeded the limit.</summary>
	public bool IsExceeded => CurrentSpend >= Limit;

	/// <summary>True if the current date falls within the budget period.</summary>
	public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

	// ──────────────────────── Behaviour Methods ─────────────────────────

	/// <summary>Updates the budget limit.</summary>
	public void UpdateLimit(decimal newLimit)
	{
		if (newLimit <= 0)
			throw new ArgumentException("Budget limit must be positive.", nameof(newLimit));

		Limit = newLimit;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Updates the budget name and optionally the period dates.</summary>
	public void UpdateDetails(string name, DateTime? startDate = null, DateTime? endDate = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Budget name is required.", nameof(name));

		Name = name.Trim();

		if (startDate.HasValue && endDate.HasValue)
		{
			if (endDate.Value <= startDate.Value)
				throw new ArgumentException("End date must be after start date.");

			StartDate = startDate.Value;
			EndDate = endDate.Value;
		}

		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Records spending against this budget.
	/// Throws <see cref="BudgetExceededException"/> if the budget would be exceeded.
	/// </summary>
	/// <param name="amount">The expense amount to record.</param>
	/// <param name="allowExceed">
	/// If false (default), throws when spending exceeds the limit.
	/// If true, records the spend but still marks the budget as exceeded.
	/// </param>
	public void RecordSpending(decimal amount, bool allowExceed = false)
	{
		if (amount <= 0)
			throw new ArgumentException("Spending amount must be positive.", nameof(amount));

		if (!allowExceed && CurrentSpend + amount > Limit)
		{
			throw new BudgetExceededException(Id, Limit, CurrentSpend + amount);
		}

		CurrentSpend += amount;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Reverses previously recorded spending (e.g., when a transaction is deleted).
	/// </summary>
	public void ReverseSpending(decimal amount)
	{
		if (amount <= 0)
			throw new ArgumentException("Amount must be positive.", nameof(amount));

		CurrentSpend = Math.Max(0, CurrentSpend - amount);
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Resets the spend counter to zero. Typically called at the start
	/// of a new budget period.
	/// </summary>
	public void ResetSpend()
	{
		CurrentSpend = 0;
		UpdatedAt = DateTime.UtcNow;
	}
}
