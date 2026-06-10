using System.Transactions;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

/// <summary>
/// Represents a transaction category (e.g., "Groceries", "Salary", "Rent").
/// Each category is tied to a specific TransactionType (Income or Expense).
/// </summary>
public class Category : BaseEntity
{
	// ──────────────────────────── Properties ────────────────────────────

	public string Name { get; private set; } = string.Empty;

	/// <summary>Whether this category applies to Income or Expense transactions.</summary>
	public TransactionType Type { get; private set; }

	/// <summary>Optional icon identifier (e.g., emoji or icon name for the UI).</summary>
	public string? Icon { get; private set; }

	/// <summary>Optional hex colour code for UI display (e.g., "#FF5733").</summary>
	public string? Colour { get; private set; }

	// ──────────────────────── Foreign Key / Nav ─────────────────────────

	public Guid UserId { get; private set; }
	public User? User { get; private set; }

	private readonly List<Transaction> _transactions = new();
	public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

	private readonly List<Budget> _budgets = new();
	public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();

	// ──────────────────────────── Constructors ──────────────────────────

	private Category() { }

	/// <summary>
	/// Creates a new Category. Called by User.AddCategory().
	/// Internal so only the Core assembly can create categories directly.
	/// </summary>
	internal Category(string name, TransactionType type, Guid userId, string? icon = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Category name is required.", nameof(name));

		Name = name.Trim();
		Type = type;
		UserId = userId;
		Icon = icon;
	}

	// ──────────────────────── Behaviour Methods ─────────────────────────

	/// <summary>Updates category display properties.</summary>
	public void UpdateDetails(string name, string? icon = null, string? colour = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Category name is required.", nameof(name));

		Name = name.Trim();
		Icon = icon?.Trim();
		Colour = colour?.Trim();
		UpdatedAt = DateTime.UtcNow;
	}
}
