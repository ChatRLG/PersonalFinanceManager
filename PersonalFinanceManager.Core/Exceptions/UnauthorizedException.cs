namespace PersonalFinanceManager.Core.Exceptions;

/// <summary>
/// Thrown when authentication fails (e.g. bad credentials) or a caller attempts
/// to act without a valid identity. Mapped to HTTP 401 by the API middleware.
/// </summary>
public class UnauthorizedException : DomainException
{
	public UnauthorizedException(string message) : base(message) { }
}
