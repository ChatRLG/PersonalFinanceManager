using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.API.Auth;
using PersonalFinanceManager.API.Middleware;
using PersonalFinanceManager.Application;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Infrastructure;
using PersonalFinanceManager.Infrastructure.Auth;
using PersonalFinanceManager.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ──────────────── Services ────────────────

// Infrastructure layer (DbContext, Repositories, UnitOfWork, auth services)
builder.Services.AddInfrastructure(builder.Configuration);

// Application layer (use-case services)
builder.Services.AddApplication();

// Current-user accessor (reads JWT claims off HttpContext)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

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

	// Enable "Authorize" in the Swagger UI with a bearer token.
	var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Paste the JWT returned by /api/auth/login (no 'Bearer ' prefix needed).",
		Reference = new Microsoft.OpenApi.Models.OpenApiReference
		{
			Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
			Id = "Bearer"
		}
	};

	options.AddSecurityDefinition("Bearer", jwtScheme);
	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		[jwtScheme] = Array.Empty<string>()
	});
});

// ──────────────── Authentication ────────────────
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
	?? throw new InvalidOperationException("JWT settings ('Jwt' section) are not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtSettings.Issuer,
			ValidateAudience = true,
			ValidAudience = jwtSettings.Audience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromMinutes(1)
		};
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

// Apply pending EF Core migrations (creates the DB on first run).
// This preserves existing data. To wipe and recreate during development,
// call DatabaseInitializer.ResetDatabaseAsync(app.Services) instead.
await DatabaseInitializer.MigrateDatabaseAsync(app.Services);


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
