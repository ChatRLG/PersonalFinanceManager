namespace PersonalFinanceManager.Core.Enums;
/// <summary>
/// Represents the direction of a financial transaction.
/// </summary>
public enum TransactionType
{
	/// <summary>Money coming in (salary, refund, gift, etc.).</summary>
	Income = 0,

	/// <summary>Money going out (purchase, payment, etc.).</summary>
	Expense = 1,

	/// <summary>Money moving between user's own accounts.</summary>
	Transfer = 2
}
