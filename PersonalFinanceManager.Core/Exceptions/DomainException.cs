namespace PersonalFinanceManager.Core.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// </summary>
public class DomainException : Exception
{
	public DomainException(string message) : base(message) { }
	public DomainException(string message, Exception innerException)
		: base(message, innerException) { }
}

