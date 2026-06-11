using System.Net;
using System.Text.Json;
using PersonalFinanceManager.Core.Exceptions;

namespace PersonalFinanceManager.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

	public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
			await HandleExceptionAsync(context, ex);
		}
	}

	private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var (statusCode, message) = exception switch
		{
			EntityNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
			InsufficientFundsException ex => (HttpStatusCode.BadRequest, ex.Message),
			BudgetExceededException ex => (HttpStatusCode.BadRequest, ex.Message),
			DomainException ex => (HttpStatusCode.BadRequest, ex.Message),
			ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
			InvalidOperationException ex => (HttpStatusCode.Conflict, ex.Message),
			_ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
		};

		context.Response.StatusCode = (int)statusCode;

		var response = new ErrorResponse
		{
			StatusCode = (int)statusCode,
			Message = message,
			Timestamp = DateTime.UtcNow
		};

		var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		await context.Response.WriteAsync(json);
	}
}

public class ErrorResponse
{
	public int StatusCode { get; set; }
	public string Message { get; set; } = string.Empty;
	public DateTime Timestamp { get; set; }
}
