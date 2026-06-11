using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.Entities;

/// <summary>
/// Represents a single financial transaction (income, expense, or transfer).
/// 
/// For transfers: the source account is debited and the DestinationAccount
/// is credited. Both AccountId and DestinationAccountId are populated.
/// </summary>
public class Transaction : BaseEntity
{
	// ──────────────────────────── Properties ────────────────────────────

	/// <summary>The transaction amount. Always stored as a positive number.</summary>
	public decimal Amount { get; private set; }

	/// <summary>The currency of this transaction.</summary>
	public CurrencyCode Currency { get; private set; }

	public TransactionType Type { get; private set; }

	/// <summary>Short description of the transaction (e.g., "Weekly groceries").</summary>
	public string Description { get; private set; } = string.Empty;

	/// <summary>Optional longer notes.</summary>
	public string? Notes { get; private set; }

	/// <summary>
	/// The date the transaction actually occurred (may differ from CreatedAt
	/// if entered retroactively).
	/// </summary>
	public DateTime TransactionDate { get; private set; }

	/// <summary>Whether this is a recurring transaction template.</summary>
	public bool IsRecurring { get; private set; }

	// ──────────────────────── Foreign Keys / Nav ────────────────────────

	/// <summary>The primary (source) account. For expenses/income this is the only account.</summary>
	public Guid AccountId { get; private set; }
	public Account? Account { get; private set; }

	/// <summary>The category this transaction belongs to.</summary>
	public Guid CategoryId { get; private set; }
	public Category? Category { get; private set; }

	/// <summary>
	/// For transfers only — the destination account that receives the money.
	/// Null for income/expense transactions.
	/// </summary>
	public Guid? DestinationAccountId { get; private set; }
	public Account? DestinationAccount { get; private set; }

	// ──────────────────────────── Constructors ──────────────────────────

	private Transaction() { }

	/// <summary>
	/// Creates a new income or expense transaction.
	/// </summary>
	public Transaction(
		decimal amount,
		CurrencyCode currency,
		TransactionType type,
		string description,
		DateTime transactionDate,
		Guid accountId,
		Guid categoryId,
		string? notes = null)
	{
		if (amount <= 0)
			throw new ArgumentException("Transaction amount must be positive.", nameof(amount));
		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException("Description is required.", nameof(description));
		if (type == TransactionType.Transfer)
			throw new ArgumentException(
				"Use the transfer constructor for transfer transactions.", nameof(type));

		Amount = amount;
		Currency = currency;
		Type = type;
		Description = description.Trim();
		TransactionDate = transactionDate;
		AccountId = accountId;
		CategoryId = categoryId;
		Notes = notes?.Trim();
		IsRecurring = false;
	}

	/// <summary>
	/// Creates a transfer transaction between two accounts.
	/// </summary>
	public Transaction(
		decimal amount,
		CurrencyCode currency,
		string description,
		DateTime transactionDate,
		Guid sourceAccountId,
		Guid destinationAccountId,
		Guid categoryId,
		string? notes = null)
	{
		if (amount <= 0)
			throw new ArgumentException("Transfer amount must be positive.", nameof(amount));
		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException("Description is required.", nameof(description));
		if (sourceAccountId == destinationAccountId)
			throw new ArgumentException(
				"Source and destination accounts must be different.", nameof(destinationAccountId));

		Amount = amount;
		Currency = currency;
		Type = TransactionType.Transfer;
		Description = description.Trim();
		TransactionDate = transactionDate;
		AccountId = sourceAccountId;
		DestinationAccountId = destinationAccountId;
		CategoryId = categoryId;
		Notes = notes?.Trim();
		IsRecurring = false;
	}

	// ──────────────────────── Behaviour Methods ─────────────────────────

	/// <summary>Updates editable transaction details.</summary>
	public void UpdateDetails(string description, string? notes = null)
	{
		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException("Description is required.", nameof(description));

		Description = description.Trim();
		Notes = notes?.Trim();
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Updates the transaction date (e.g., correcting a mistake).</summary>
	public void UpdateTransactionDate(DateTime newDate)
	{
		if (newDate > DateTime.UtcNow.AddDays(1))
			throw new ArgumentException("Transaction date cannot be in the future.", nameof(newDate));

		TransactionDate = newDate;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Marks this transaction as part of a recurring series.</summary>
	public void MarkAsRecurring()
	{
		IsRecurring = true;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Removes the recurring flag from this transaction.</summary>
	public void UnmarkAsRecurring()
	{
		IsRecurring = false;
		UpdatedAt = DateTime.UtcNow;
	}
}
