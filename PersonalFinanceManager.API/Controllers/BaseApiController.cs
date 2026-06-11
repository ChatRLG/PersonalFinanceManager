using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceManager.API.Controllers;

/// <summary>
/// Base controller with common route prefix and API conventions.
/// All domain controllers should inherit from this.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}