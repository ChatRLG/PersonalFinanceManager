
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.ValueObjects;

namespace PersonalFinanceManager.Core.Entities;

/// <summary>
/// Represents a registered user. This is the root aggregate —
/// all financial data is owned by a User.
/// </summary>
public class User : BaseEntity
{
	// ──────────────────────────── Properties ────────────────────────────

	public string Email { get; private set; } = string.Empty;
	public string FirstName { get; private set; } = string.Empty;
	public string LastName { get; private set; } = string.Empty;

	/// <summary>
	/// Stores the BCrypt/Argon2 hash — NEVER store plain text passwords.
	/// </summary>
	public string PasswordHash { get; private set; } = string.Empty;

	/// <summary>The user's preferred default currency for new accounts.</summary>
	public CurrencyCode DefaultCurrency { get; private set; }

	/// <summary>Optional mailing address (value object, stored as owned entity in EF).</summary>
	public Address? Address { get; private set; }

	// ──────────────────────── Navigation Properties ─────────────────────

	private readonly List<Account> _accounts = new();
	public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

	private readonly List<Category> _categories = new();
	public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

	private readonly List<Budget> _budgets = new();
	public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();

	// ──────────────────────────── Constructors ──────────────────────────

	/// <summary>EF Core requires a parameterless constructor. Keep it private.</summary>
	private User() { }

	/// <summary>
	/// Creates a new User. All required fields are validated here.
	/// </summary>
	public User(string email, string firstName, string lastName,
				string passwordHash, CurrencyCode defaultCurrency)
	{
		SetEmail(email);
		SetName(firstName, lastName);
		PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
		DefaultCurrency = defaultCurrency;
	}

	// ──────────────────────── Computed Properties ────────────────────────

	public string FullName => $"{FirstName} {LastName}";

	// ──────────────────────── Behaviour Methods ─────────────────────────

	/// <summary>Updates first and last name with validation.</summary>
	public void SetName(string firstName, string lastName)
	{
		if (string.IsNullOrWhiteSpace(firstName))
			throw new ArgumentException("First name is required.", nameof(firstName));
		if (string.IsNullOrWhiteSpace(lastName))
			throw new ArgumentException("Last name is required.", nameof(lastName));

		FirstName = firstName.Trim();
		LastName = lastName.Trim();
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Sets and validates the email address.</summary>
	public void SetEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email cannot be empty.", nameof(email));
		if (!email.Contains('@') || !email.Contains('.'))
			throw new ArgumentException("Email format is invalid.", nameof(email));

		Email = email.Trim().ToLowerInvariant();
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Sets the user's mailing address.</summary>
	public void SetAddress(Address address)
	{
		Address = address ?? throw new ArgumentNullException(nameof(address));
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Replaces the password hash (caller is responsible for hashing).</summary>
	public void ChangePassword(string newPasswordHash)
	{
		if (string.IsNullOrWhiteSpace(newPasswordHash))
			throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

		PasswordHash = newPasswordHash;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Changes the default currency for future accounts.</summary>
	public void SetDefaultCurrency(CurrencyCode currency)
	{
		DefaultCurrency = currency;
		UpdatedAt = DateTime.UtcNow;
	}

	// ──────────────── Factory Methods for Child Entities ────────────────

	/// <summary>
	/// Creates a new Account owned by this user and adds it to the collection.
	/// </summary>
	public Account AddAccount(string name, AccountType type,
							  CurrencyCode currency, decimal initialBalance = 0m)
	{
		// Business rule: no two active accounts with the same name
		if (_accounts.Any(a => !a.IsDeleted
			&& a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException(
				$"An active account named '{name}' already exists.");
		}

		var account = new Account(name, type, currency, initialBalance, Id);
		_accounts.Add(account);
		return account;
	}

	/// <summary>
	/// Creates a new Category owned by this user.
	/// </summary>
	public Category AddCategory(string name, TransactionType type, string? icon = null)
	{
		// Business rule: no duplicate category name+type combinations
		if (_categories.Any(c => !c.IsDeleted
			&& c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			&& c.Type == type))
		{
			throw new InvalidOperationException(
				$"Category '{name}' for '{type}' already exists.");
		}

		var category = new Category(name, type, Id, icon);
		_categories.Add(category);
		return category;
	}

	/// <summary>
	/// Creates a new Budget owned by this user.
	/// </summary>
	public Budget AddBudget(
		string name,
		decimal limit,
		CurrencyCode currency,
		BudgetPeriod period,
		DateTime startDate,
		DateTime endDate,
		Guid categoryId)
	{
		// Verify the category belongs to this user
		if (!_categories.Any(c => c.Id == categoryId && !c.IsDeleted))
		{
			throw new InvalidOperationException(
				$"Category '{categoryId}' not found or does not belong to this user.");
		}

		// Business rule: only one active budget per category per overlapping period
		if (_budgets.Any(b => !b.IsDeleted
		                      && b.CategoryId == categoryId
		                      && b.StartDate < endDate
		                      && b.EndDate > startDate))
		{
			throw new InvalidOperationException(
				$"An overlapping budget already exists for this category.");
		}

		var budget = new Budget(name, limit, currency, period, startDate, endDate, Id, categoryId);
		_budgets.Add(budget);
		return budget;
	}

}
