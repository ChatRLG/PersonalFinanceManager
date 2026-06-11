using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;

namespace PersonalFinanceManager.Core.Entities;

/// <summary>
/// Represents a financial account (checking, savings, credit card, etc.).
/// Balance is only modified through Credit/Debit methods.
/// </summary>
public class Account : BaseEntity
{
	// ──────────────────────────── Properties ────────────────────────────

	public string Name { get; private set; } = string.Empty;
	public AccountType Type { get; private set; }
	public CurrencyCode Currency { get; private set; }

	/// <summary>Current balance. Modified only via Credit() / Debit().</summary>
	public decimal Balance { get; private set; }

	public string? Description { get; private set; }

	/// <summary>Inactive accounts cannot accept new transactions.</summary>
	public bool IsActive { get; private set; }

	// ──────────────────────── Foreign Key / Nav ─────────────────────────

	public Guid UserId { get; private set; }
	public User? User { get; private set; }

	private readonly List<Transaction> _transactions = new();
	public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

	// ──────────────────────────── Constructors ──────────────────────────

	private Account() { }

	/// <summary>
	/// Creates a new Account. Called by User.AddAccount().
	/// Made internal so only the Core assembly can create accounts directly.
	/// </summary>
	internal Account(string name, AccountType type, CurrencyCode currency,
		decimal initialBalance, Guid userId)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Account name is required.", nameof(name));

		Name = name.Trim();
		Type = type;
		Currency = currency;
		Balance = initialBalance;
		UserId = userId;
		IsActive = true;
	}

	// ──────────────────────── Behaviour Methods ─────────────────────────

	/// <summary>Updates the display name and optional description.</summary>
	public void UpdateDetails(string name, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Account name is required.", nameof(name));

		Name = name.Trim();
		Description = description?.Trim();
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Deactivates the account so it cannot accept new transactions.</summary>
	public void Deactivate()
	{
		IsActive = false;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Reactivates a previously deactivated account.</summary>
	public void Activate()
	{
		IsActive = true;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Adds money to this account.</summary>
	public void Credit(decimal amount)
	{
		EnsureActive();

		if (amount <= 0)
			throw new ArgumentException("Credit amount must be positive.", nameof(amount));

		Balance += amount;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Removes money from this account. For Checking/Savings/Cash accounts,
	/// the balance cannot go below zero. Credit card and Loan accounts can
	/// carry a negative balance (representing debt).
	/// </summary>
	public void Debit(decimal amount)
	{
		EnsureActive();

		if (amount <= 0)
			throw new ArgumentException("Debit amount must be positive.", nameof(amount));

		bool isAssetAccount = Type is AccountType.Checking
			or AccountType.Savings
			or AccountType.Cash;

		if (isAssetAccount && Balance - amount < 0)
		{
			throw new InsufficientFundsException(Id, amount, Balance);
		}

		Balance -= amount;
		UpdatedAt = DateTime.UtcNow;
	}

	// ──────────────────────── Private Helpers ───────────────────────────

	/// <summary>Throws if the account is inactive.</summary>
	private void EnsureActive()
	{
		if (!IsActive)
			throw new InvalidOperationException(
				$"Account '{Name}' (Id: {Id}) is inactive and cannot accept transactions.");
	}
}