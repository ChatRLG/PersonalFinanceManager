using PersonalFinanceManager.Application.Contracts.Accounts;
using PersonalFinanceManager.Application.Contracts.Auth;
using PersonalFinanceManager.Application.Contracts.Budgets;
using PersonalFinanceManager.Application.Contracts.Categories;
using PersonalFinanceManager.Application.Contracts.Dashboard;
using PersonalFinanceManager.Application.Contracts.Transactions;

namespace PersonalFinanceManager.Mobile.Services;

public interface IApiClient
{
    Task<bool> PingAsync(CancellationToken ct = default);

    // Auth
    Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(RegisterRequest req, CancellationToken ct = default);

    // Accounts
    Task<List<AccountDto>> GetAccountsAsync(CancellationToken ct = default);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest req, CancellationToken ct = default);
    Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest req, CancellationToken ct = default);
    Task ActivateAccountAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAccountAsync(Guid id, CancellationToken ct = default);
    Task DeleteAccountAsync(Guid id, CancellationToken ct = default);

    // Transactions
    Task<PagedResult<TransactionDto>> GetTransactionsPagedAsync(
        Guid? accountId = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<List<TransactionDto>> GetRecentTransactionsAsync(int count = 10, CancellationToken ct = default);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest req, CancellationToken ct = default);
    Task DeleteTransactionAsync(Guid id, CancellationToken ct = default);

    // Budgets
    Task<List<BudgetDto>> GetBudgetsAsync(CancellationToken ct = default);
    Task<List<BudgetDto>> GetActiveBudgetsAsync(CancellationToken ct = default);
    Task<BudgetDto> CreateBudgetAsync(CreateBudgetRequest req, CancellationToken ct = default);
    Task<BudgetDto> UpdateBudgetAsync(Guid id, UpdateBudgetRequest req, CancellationToken ct = default);
    Task DeleteBudgetAsync(Guid id, CancellationToken ct = default);

    // Categories
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest req, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    // Dashboard
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
