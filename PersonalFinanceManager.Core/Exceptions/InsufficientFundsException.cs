namespace PersonalFinanceManager.Core.Exceptions;

public class InsufficientFundsException : DomainException
{
	public Guid AccountId { get; }
	public decimal RequestedAmount { get; }
	public decimal AvailableBalance { get; }

	public InsufficientFundsException(Guid accountId, decimal requested, decimal available)
		: base($"Insufficient funds in account '{accountId}'. " +
		       $"Requested: {requested:C}, Available: {available:C}")
	{
		AccountId = accountId;
		RequestedAmount = requested;
		AvailableBalance = available;
	}
}