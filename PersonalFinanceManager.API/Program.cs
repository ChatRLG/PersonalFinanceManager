using System.Text.Json.Serialization;
using PersonalFinanceManager.API.Middleware;
using PersonalFinanceManager.Infrastructure;
using PersonalFinanceManager.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ──────────────── Services ────────────────

// Infrastructure layer (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers + JSON settings
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
	{
		Title = "Personal Finance Manager API",
		Version = "v1",
		Description = "API for managing personal finances — accounts, transactions, categories, and budgets."
	});
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
	?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
	options.AddPolicy("DefaultPolicy", policy =>
	{
		policy.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();


// ──────────────── Database Initialization ────────────────

//await DatabaseInitializer.InitializeDatabaseAsync(app.Services, app.Environment.IsDevelopment());

// TEMPORARY: Reset DB after schema changes. Revert after first successful run.
await DatabaseInitializer.ResetDatabaseAsync(app.Services);


// ──────────────── Middleware Pipeline ────────────────────

// Global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Finance Manager v1");
		options.RoutePrefix = "swagger";
	});
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
