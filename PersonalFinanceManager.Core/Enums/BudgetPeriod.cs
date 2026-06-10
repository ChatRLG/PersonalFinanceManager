namespace PersonalFinanceManager.Core.Enums;
/// <summary>
/// Represents the recurring period for a budget.
/// </summary>
public enum BudgetPeriod
{
	/// <summary>Budget resets daily.</summary>
	Daily = 0,

	/// <summary>Budget resets weekly.</summary>
	Weekly = 1,

	/// <summary>Budget resets monthly (most common).</summary>
	Monthly = 2,

	/// <summary>Budget resets quarterly.</summary>
	Quarterly = 3,

	/// <summary>Budget resets yearly.</summary>
	Yearly = 4,

	/// <summary>One-time budget with fixed start/end dates.</summary>
	Custom = 5
}