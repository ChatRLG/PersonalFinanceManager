using System.Net;
using PersonalFinanceManager.IntegrationTests.Infrastructure;

namespace PersonalFinanceManager.IntegrationTests.Transactions;

/// <summary>
/// Verifies Design Rule #1: account balance and budget spend are updated
/// atomically in a single SaveChangesAsync per transaction use-case.
/// </summary>
public class TransactionConsistencyTests : IClassFixture<TransactionTestFactory>
{
    private readonly TransactionTestFactory _factory;

    public TransactionConsistencyTests(TransactionTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient NewClient() => _factory.CreateClient();

    // ── Expense atomicity ─────────────────────────────────

    [Fact]
    public async Task Expense_DebitsAccount_RecordsBudget_Atomically()
    {
        var client = NewClient();
        var (token, _) = await ApiHelpers.RegisterUserAsync(client);
        ApiHelpers.SetBearerToken(client, token);

        // Setup: account + budget
        var account    = await ApiHelpers.CreateAccountAsync(client, "Checking", initialBalance: 1000m);
        var accountId  = account.GetProperty("id").GetString()!;
        var catId      = await ApiHelpers.GetFirstExpenseCategoryIdAsync(client);
        var budget     = await ApiHelpers.CreateBudgetAsync(client, "Food", catId, limit: 500m);
        var budgetId   = budget.GetProperty("id").GetString()!;

        // Act: create expense
        await ApiHelpers.CreateExpenseAsync(client, accountId, catId, 150m);

        // Assert: account debited
        var updatedAccount = await ApiHelpers.GetAccountAsync(client, accountId);
        updatedAccount.GetProperty("balance").GetDecimal().Should().Be(850m,
            "expense of $150 should debit balance from $1000 to $850");

        // Assert: budget spend recorded
        var updatedBudget = await ApiHelpers.GetBudgetAsync(client, budgetId);
        updatedBudget.GetProperty("currentSpend").GetDecimal().Should().Be(150m,
            "budget spend should reflect the $150 expense");
    }

    [Fact]
    public async Task DeleteExpense_ReversesBalance_AndBudget_Atomically()
    {
        var client = NewClient();
        var (token, _) = await ApiHelpers.RegisterUserAsync(client);
        ApiHelpers.SetBearerToken(client, token);

        var account   = await ApiHelpers.CreateAccountAsync(client, "Checking", initialBalance: 1000m);
        var accountId = account.GetProperty("id").GetString()!;
        var catId     = await ApiHelpers.GetFirstExpenseCategoryIdAsync(client);
        var budget    = await ApiHelpers.CreateBudgetAsync(client, "Food", catId, limit: 500m);
        var budgetId  = budget.GetProperty("id").GetString()!;

        // Record an expense
        var txn   = await ApiHelpers.CreateExpenseAsync(client, accountId, catId, 150m);
        var txnId = txn.GetProperty("id").GetString()!;

        // Delete it
        await ApiHelpers.DeleteTransactionAsync(client, txnId);

        // Balance should be restored
        var updatedAccount = await ApiHelpers.GetAccountAsync(client, accountId);
        updatedAccount.GetProperty("balance").GetDecimal().Should().Be(1000m,
            "deleting the expense should restore balance to $1000");

        // Budget spend should be zero
        var updatedBudget = await ApiHelpers.GetBudgetAsync(client, budgetId);
        updatedBudget.GetProperty("currentSpend").GetDecimal().Should().Be(0m,
            "deleting the expense should reverse budget spend to $0");
    }

    // ── Transfer ──────────────────────────────────────────

    [Fact]
    public async Task Transfer_DebitsSource_CreditsDestination()
    {
        var client = NewClient();
        var (token, _) = await ApiHelpers.RegisterUserAsync(client);
        ApiHelpers.SetBearerToken(client, token);

        var accA    = await ApiHelpers.CreateAccountAsync(client, "AccountA", initialBalance: 1000m);
        var accB    = await ApiHelpers.CreateAccountAsync(client, "AccountB", initialBalance: 200m);
        var accAId  = accA.GetProperty("id").GetString()!;
        var accBId  = accB.GetProperty("id").GetString()!;
        var catId   = await ApiHelpers.GetFirstExpenseCategoryIdAsync(client);

        await ApiHelpers.CreateTransferAsync(client, accAId, accBId, catId, 300m);

        var updA = await ApiHelpers.GetAccountAsync(client, accAId);
        var updB = await ApiHelpers.GetAccountAsync(client, accBId);
        updA.GetProperty("balance").GetDecimal().Should().Be(700m);
        updB.GetProperty("balance").GetDecimal().Should().Be(500m);
    }

    // ── InsufficientFunds ─────────────────────────────────

    [Fact]
    public async Task Expense_InsufficientFunds_Returns400()
    {
        var client = NewClient();
        var (token, _) = await ApiHelpers.RegisterUserAsync(client);
        ApiHelpers.SetBearerToken(client, token);

        var account   = await ApiHelpers.CreateAccountAsync(client, "SmallAccount", initialBalance: 100m);
        var accountId = account.GetProperty("id").GetString()!;
        var catId     = await ApiHelpers.GetFirstExpenseCategoryIdAsync(client);

        var body = new
        {
            amount = 500m, currency = "USD", type = "Expense",
            description = "Too big", accountId, categoryId = catId,
            transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        var response = await client.PostAsJsonAsync("api/transactions", body);

        // GlobalExceptionHandlerMiddleware maps InsufficientFundsException → 400
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
