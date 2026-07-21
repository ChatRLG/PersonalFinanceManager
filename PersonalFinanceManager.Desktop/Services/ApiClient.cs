using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinanceManager.Application.Contracts.Accounts;
using PersonalFinanceManager.Application.Contracts.Auth;
using PersonalFinanceManager.Application.Contracts.Budgets;
using PersonalFinanceManager.Application.Contracts.Categories;
using PersonalFinanceManager.Application.Contracts.Dashboard;
using PersonalFinanceManager.Application.Contracts.Transactions;

namespace PersonalFinanceManager.Desktop.Services;

/// <summary>
/// Thin wrapper over HttpClient that calls the PFM API and returns typed results.
/// Bearer token is attached by <see cref="AuthTokenHandler"/>.
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http) => _http = http;

    // ── Auth ─────────────────────────────────────────────────────────────────

    public Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct = default)
        => PostAsync<LoginRequest, AuthResult>("api/auth/login", req, ct);

    public Task<AuthResult> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
        => PostAsync<RegisterRequest, AuthResult>("api/auth/register", req, ct);

    // ── Accounts ─────────────────────────────────────────────────────────────

    public Task<List<AccountDto>> GetAccountsAsync(CancellationToken ct = default)
        => GetAsync<List<AccountDto>>("api/accounts", ct);

    public Task<AccountDto> CreateAccountAsync(CreateAccountRequest req, CancellationToken ct = default)
        => PostAsync<CreateAccountRequest, AccountDto>("api/accounts", req, ct);

    public Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest req, CancellationToken ct = default)
        => PutAsync<UpdateAccountRequest, AccountDto>($"api/accounts/{id}", req, ct);

    public Task ActivateAccountAsync(Guid id, CancellationToken ct = default)
        => _http.PutAsync($"api/accounts/{id}/activate", null, ct).ContinueWith(EnsureSuccess);

    public Task DeactivateAccountAsync(Guid id, CancellationToken ct = default)
        => _http.PutAsync($"api/accounts/{id}/deactivate", null, ct).ContinueWith(EnsureSuccess);

    public Task DeleteAccountAsync(Guid id, CancellationToken ct = default)
        => _http.DeleteAsync($"api/accounts/{id}", ct).ContinueWith(EnsureSuccess);

    // ── Transactions ──────────────────────────────────────────────────────────

    public Task<PagedResult<TransactionDto>> GetTransactionsPagedAsync(
        Guid? accountId = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var url = $"api/transactions?page={page}&pageSize={pageSize}";
        if (accountId.HasValue) url += $"&accountId={accountId.Value}";
        return GetAsync<PagedResult<TransactionDto>>(url, ct);
    }

    public Task<List<TransactionDto>> GetRecentTransactionsAsync(int count = 10, CancellationToken ct = default)
        => GetAsync<List<TransactionDto>>($"api/transactions/recent?count={count}", ct);

    public Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest req, CancellationToken ct = default)
        => PostAsync<CreateTransactionRequest, TransactionDto>("api/transactions", req, ct);

    public Task DeleteTransactionAsync(Guid id, CancellationToken ct = default)
        => _http.DeleteAsync($"api/transactions/{id}", ct).ContinueWith(EnsureSuccess);

    // ── Budgets ───────────────────────────────────────────────────────────────

    public Task<List<BudgetDto>> GetBudgetsAsync(CancellationToken ct = default)
        => GetAsync<List<BudgetDto>>("api/budgets", ct);

    public Task<List<BudgetDto>> GetActiveBudgetsAsync(CancellationToken ct = default)
        => GetAsync<List<BudgetDto>>("api/budgets/active", ct);

    public Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest req, CancellationToken ct = default)
        => PostAsync<CreateBudgetRequest, BudgetDto>("api/budgets", req, ct);

    public Task<BudgetDto> UpdateBudgetAsync(Guid id, UpdateBudgetRequest req, CancellationToken ct = default)
        => PutAsync<UpdateBudgetRequest, BudgetDto>($"api/budgets/{id}", req, ct);

    public Task DeleteBudgetAsync(Guid id, CancellationToken ct = default)
        => _http.DeleteAsync($"api/budgets/{id}", ct).ContinueWith(EnsureSuccess);

    // ── Categories ────────────────────────────────────────────────────────────

    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
        => GetAsync<List<CategoryDto>>("api/categories", ct);

    public Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest req, CancellationToken ct = default)
        => PostAsync<CreateCategoryRequest, CategoryDto>("api/categories", req, ct);

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
        => _http.DeleteAsync($"api/categories/{id}", ct).ContinueWith(EnsureSuccess);

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
        => GetAsync<DashboardDto>("api/dashboard", ct);

    // ── Connectivity ──────────────────────────────────────────────────────────

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync("health", ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>(_opts, ct)
               ?? throw new InvalidOperationException($"Empty response from GET {url}");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(url, body, _opts, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TResponse>(_opts, ct)
               ?? throw new InvalidOperationException($"Empty response from POST {url}");
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct)
    {
        var resp = await _http.PutAsJsonAsync(url, body, _opts, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TResponse>(_opts, ct)
               ?? throw new InvalidOperationException($"Empty response from PUT {url}");
    }

    private static void EnsureSuccess(Task<HttpResponseMessage> t)
        => t.Result.EnsureSuccessStatusCode();
}
