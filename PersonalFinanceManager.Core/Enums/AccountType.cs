namespace PersonalFinanceManager.Core.Enums;
/// <summary>
/// Represents the type of financial account.
/// </summary>
public enum AccountType
{
	/// <summary>A standard checking/current account.</summary>
	Checking = 0,

	/// <summary>A savings account.</summary>
	Savings = 1,

	/// <summary>A credit card account (balance represents debt).</summary>
	CreditCard = 2,

	/// <summary>A cash wallet or physical cash tracking.</summary>
	Cash = 3,

	/// <summary>An investment or brokerage account.</summary>
	Investment = 4,

	/// <summary>A loan account (e.g., mortgage, personal loan).</summary>
	Loan = 5
}