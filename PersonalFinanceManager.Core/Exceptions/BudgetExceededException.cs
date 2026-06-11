namespace PersonalFinanceManager.Core.Exceptions;
public class BudgetExceededException : DomainException
{
	public Guid BudgetId { get; }
	public decimal BudgetLimit { get; }
	public decimal AttemptedSpend { get; }

	public BudgetExceededException(Guid budgetId, decimal limit, decimal attempted)
		: base($"Budget '{budgetId}' exceeded. Limit: {limit:C}, Attempted total: {attempted:C}")
	{
		BudgetId = budgetId;
		BudgetLimit = limit;
		AttemptedSpend = attempted;
	}
}