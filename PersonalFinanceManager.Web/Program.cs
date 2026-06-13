using Microsoft.AspNetCore.Components.Authorization;
using PersonalFinanceManager.Web.Auth;
using PersonalFinanceManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Razor / Blazor ──────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ── Custom Local Storage ────────────────────────────────
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// ── Toast Notifications ─────────────────────────────────
builder.Services.AddScoped<ToastService>();

// ── Authentication State ────────────────────────────────
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
	provider => provider.GetRequiredService<JwtAuthStateProvider>());

// ── HTTP Client for API communication ───────────────────
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
				 ?? "http://localhost:5122";

builder.Services.AddScoped<AuthTokenHandler>();

builder.Services.AddHttpClient("PFM_API", client =>
{
	client.BaseAddress = new Uri(apiBaseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped<IApiClient>(provider =>
{
	var factory = provider.GetRequiredService<IHttpClientFactory>();
	var httpClient = factory.CreateClient("PFM_API");
	var logger = provider.GetRequiredService<ILogger<ApiClient>>();
	return new ApiClient(httpClient, logger);
});

// ── Domain Services ─────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
