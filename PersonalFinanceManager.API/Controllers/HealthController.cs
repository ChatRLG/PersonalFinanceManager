using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
	private readonly AppDBContext _context;
	private readonly ILogger<HealthController> _logger;

	public HealthController(AppDBContext context, ILogger<HealthController> logger)
	{
		_context = context;
		_logger = logger;
	}

	/// <summary>
	/// Basic liveness check — confirms the API is running.
	/// </summary>
	[HttpGet]
	public IActionResult Get()
	{
		return Ok(new
		{
			Status = "Healthy",
			Timestamp = DateTime.UtcNow,
			Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
		});
	}

	/// <summary>
	/// Database connectivity check.
	/// </summary>
	[HttpGet("db")]
	public async Task<IActionResult> CheckDatabase()
	{
		try
		{
			var canConnect = await _context.Database.CanConnectAsync();

			if (canConnect)
			{
				_logger.LogInformation("Database connectivity check passed.");
				return Ok(new
				{
					Status = "Healthy",
					Database = "Connected",
					Timestamp = DateTime.UtcNow
				});
			}

			_logger.LogWarning("Database connectivity check failed — cannot connect.");
			return StatusCode(503, new
			{
				Status = "Unhealthy",
				Database = "Cannot connect",
				Timestamp = DateTime.UtcNow
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Database connectivity check threw an exception.");
			return StatusCode(503, new
			{
				Status = "Unhealthy",
				Database = "Error",
				Error = ex.Message,
				Timestamp = DateTime.UtcNow
			});
		}
	}

	/// <summary>
	/// Returns the SQL CREATE script for the current EF model.
	/// Available only in Development.
	/// </summary>
	[HttpGet("db/script")]
	public IActionResult GetDatabaseScript(
		[FromServices] IWebHostEnvironment env)
	{
		if (!env.IsDevelopment())
			return NotFound();

		var script = DatabaseInitializer.ExportCreateScript(HttpContext.RequestServices);

		return Content(script, "text/plain");
	}
}
