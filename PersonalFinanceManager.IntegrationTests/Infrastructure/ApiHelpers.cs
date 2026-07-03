using System.Net.Http.Headers;
using System.Text.Json;

namespace PersonalFinanceManager.IntegrationTests.Infrastructure;

/// <summary>
/// Reusable HTTP helpers for integration tests.
/// All methods assume the API returns camelCase JSON.
/// </summary>
public static class ApiHelpers
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── Auth ──────────────────────────────────────────────

    public static async Task<(string token, string email)> RegisterUserAsync(
        HttpClient client,
        string? email = null,
        string password = "TestPass1!")
    {
        email ??= $"{Guid.NewGuid():N}@test.com";

        var body = new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password,
            defaultCurrency = "USD"
        };

        var response = await client.PostAsJsonAsync("api/auth/register", body);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = content.GetProperty("token").GetString()!;
        return (token, email);
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Accounts ──────────────────────────────────────────

    public static async Task<JsonElement> CreateAccountAsync(
        HttpClient client,
        string name,
        string type = "Checking",
        decimal initialBalance = 1000m)
    {
        var body = new { name, type, currency = "USD", initialBalance };
        var response = await client.PostAsJsonAsync("api/accounts", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<JsonElement> GetAccountAsync(HttpClient client, string accountId)
    {
        var response = await client.GetAsync($"api/accounts/{accountId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ── Categories ────────────────────────────────────────

    /// <summary>Returns the first expense category Id from the seeded categories.</summary>
    public static async Task<string> GetFirstExpenseCategoryIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("api/categories?type=Expense");
        response.EnsureSuccessStatusCode();
        var cats = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        return cats![0].GetProperty("id").GetString()!;
    }

    // ── Budgets ───────────────────────────────────────────

    public static async Task<JsonElement> CreateBudgetAsync(
        HttpClient client,
        string name,
        string categoryId,
        decimal limit = 500m)
    {
        var body = new
        {
            name, limit, currency = "USD", period = "Monthly", categoryId,
            startDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            endDate   = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        var response = await client.PostAsJsonAsync("api/budgets", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<JsonElement> GetBudgetAsync(HttpClient client, string budgetId)
    {
        var response = await client.GetAsync($"api/budgets/{budgetId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ── Transactions ──────────────────────────────────────

    public static async Task<JsonElement> CreateExpenseAsync(
        HttpClient client,
        string accountId,
        string categoryId,
        decimal amount,
        string description = "Test expense")
    {
        var body = new
        {
            amount, currency = "USD", type = "Expense",
            description, accountId, categoryId,
            transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        var response = await client.PostAsJsonAsync("api/transactions", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<JsonElement> CreateIncomeAsync(
        HttpClient client,
        string accountId,
        string categoryId,
        decimal amount,
        string description = "Test income")
    {
        var body = new
        {
            amount, currency = "USD", type = "Income",
            description, accountId, categoryId,
            transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        var response = await client.PostAsJsonAsync("api/transactions", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<JsonElement> CreateTransferAsync(
        HttpClient client,
        string sourceAccountId,
        string destinationAccountId,
        string categoryId,
        decimal amount)
    {
        var body = new
        {
            amount, currency = "USD", type = "Transfer",
            description = "Transfer", sourceAccountId, accountId = sourceAccountId,
            destinationAccountId, categoryId,
            transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        var response = await client.PostAsJsonAsync("api/transactions", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task DeleteTransactionAsync(HttpClient client, string transactionId)
    {
        var response = await client.DeleteAsync($"api/transactions/{transactionId}");
        response.EnsureSuccessStatusCode();
    }
}
