using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PersonalFinanceManager.Application.Common.Interfaces;

namespace PersonalFinanceManager.API.Auth;

/// <summary>
/// Resolves the current user from the validated JWT on the active HttpContext.
/// JwtBearer is configured with MapInboundClaims = false, so claims keep their
/// original short names ("sub", "email").
/// </summary>
public class CurrentUser : ICurrentUser
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUser(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

	public Guid? UserId
	{
		get
		{
			var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
						?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

			return Guid.TryParse(value, out var id) ? id : null;
		}
	}

	public string? Email =>
		Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
		?? Principal?.FindFirstValue(ClaimTypes.Email);

	public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
