using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Auth;
using PersonalFinanceManager.Application.Auth.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;

namespace PersonalFinanceManager.API.Controllers;

/// <summary>
/// Authentication endpoints: register, login, and a protected identity probe.
/// Inherits route "api/[controller]" → /api/auth.
/// </summary>
public class AuthController : BaseApiController
{
	private readonly IAuthAppService _authService;
	private readonly ICurrentUser _currentUser;

	public AuthController(IAuthAppService authService, ICurrentUser currentUser)
	{
		_authService = authService;
		_currentUser = currentUser;
	}

	/// <summary>Registers a new user and returns a JWT.</summary>
	[HttpPost("register")]
	public async Task<ActionResult<AuthResult>> Register(
		RegisterRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(request, cancellationToken);
		return Ok(result);
	}

	/// <summary>Authenticates an existing user and returns a JWT.</summary>
	[HttpPost("login")]
	public async Task<ActionResult<AuthResult>> Login(
		LoginRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.LoginAsync(request, cancellationToken);
		return Ok(result);
	}

	/// <summary>Returns the caller's identity — used to verify the bearer token works.</summary>
	[Authorize]
	[HttpGet("me")]
	public ActionResult Me()
	{
		return Ok(new
		{
			userId = _currentUser.UserId,
			email = _currentUser.Email,
			isAuthenticated = _currentUser.IsAuthenticated
		});
	}
}
