namespace PersonalFinanceManager.Application.Common.Interfaces;

/// <summary>
/// Exposes the identity of the currently authenticated caller, resolved from
/// the request's JWT claims. Implemented in the API layer (reads HttpContext).
/// </summary>
public interface ICurrentUser
{
	/// <summary>The authenticated user's id, or null when unauthenticated.</summary>
	Guid? UserId { get; }

	/// <summary>The authenticated user's email, or null when unauthenticated.</summary>
	string? Email { get; }

	/// <summary>True when the request carries a valid authenticated identity.</summary>
	bool IsAuthenticated { get; }
}
