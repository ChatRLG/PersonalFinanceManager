using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Application.Transactions;
using PersonalFinanceManager.Application.Transactions.Dtos;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.UnitTests.Application;

/// <summary>
/// Tests for TransactionAppService — the key orchestrator of Design Rule #1
/// (account balance + budget spend updated in a single SaveChangesAsync).
///
/// Gotcha: DeleteAsync calls Accounts.GetByIdAsync TWICE for the same accountId:
///   1. Inside RequireOwnedTransactionAsync (ownership guard)
///   2. In the method body (balance reversal)
/// The mock Setup must NOT restrict call count (do NOT use .Once()).
/// </summary>
public class TransactionAppServiceTests
{
    private readonly User _user;
    private readonly Guid _userId;
    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<IAccountRepository>    _mockAccounts;
    private readonly Mock<ITransactionRepository> _mockTxns;
    private readonly Mock<ICategoryRepository>   _mockCategories;
    private readonly Mock<IBudgetRepository>     _mockBudgets;
    private readonly Mock<ICurrentUser>          _mockCurrentUser;
    private readonly TransactionAppService _sut;

    public TransactionAppServiceTests()
    {
        _user   = new User("t@t.com", "T", "T", "hash", CurrencyCode.USD);
        _userId = _user.Id;

        _mockAccounts    = new Mock<IAccountRepository>();
        _mockTxns        = new Mock<ITransactionRepository>();
        _mockCategories  = new Mock<ICategoryRepository>();
        _mockBudgets     = new Mock<IBudgetRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Accounts).Returns(_mockAccounts.Object);
        _uow.Setup(u => u.Transactions).Returns(_mockTxns.Object);
        _uow.Setup(u => u.Categories).Returns(_mockCategories.Object);
        _uow.Setup(u => u.Budgets).Returns(_mockBudgets.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockCurrentUser.Setup(c => c.UserId).Returns(_userId);

        _sut = new TransactionAppService(_uow.Object, _mockCurrentUser.Object);
    }

    // ── Helpers ───────────────────────────────────────────

    private Account MakeOwnedAccount(decimal balance = 1000m, AccountType type = AccountType.Checking)
        => _user.AddAccount($"Acc-{Guid.NewGuid():N}", type, CurrencyCode.USD, balance);

    private Category MakeOwnedCategory(TransactionType type = TransactionType.Expense)
        => _user.AddCategory($"Cat-{Guid.NewGuid():N}", type);

    private void SetupAccountMock(Account account)
        => _mockAccounts.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(account);

    private void SetupCategoryMock(Category cat)
        => _mockCategories.Setup(r => r.GetByIdAsync(cat.Id, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(cat);

    private static CreateTransactionRequest ExpenseRequest(Account account, Category cat, decimal amount = 150m) =>
        new()
        {
            Amount = amount, Currency = CurrencyCode.USD,
            Type = TransactionType.Expense, Description = "Test expense",
            TransactionDate = DateTime.UtcNow,
            AccountId = account.Id, CategoryId = cat.Id
        };

    private static CreateTransactionRequest IncomeRequest(Account account, Category cat, decimal amount = 200m) =>
        new()
        {
            Amount = amount, Currency = CurrencyCode.USD,
            Type = TransactionType.Income, Description = "Test income",
            TransactionDate = DateTime.UtcNow,
            AccountId = account.Id, CategoryId = cat.Id
        };

    // ── CreateAsync — Expense ─────────────────────────────

    [Fact]
    public async Task CreateAsync_Expense_DebitsAccount_RecordsBudgetSpend_SavesOnce()
    {
        var account = MakeOwnedAccount(balance: 1000m);
        var cat     = MakeOwnedCategory(TransactionType.Expense);
        SetupAccountMock(account);
        SetupCategoryMock(cat);

        var budget = new Budget("Food", 500m, CurrencyCode.USD, BudgetPeriod.Monthly,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30), _userId, cat.Id);
        _mockBudgets.Setup(r => r.GetByCategoryAndDateAsync(_userId, cat.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(budget);

        _mockTxns.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction t, CancellationToken _) => t);
        _mockTxns.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction?)null); // service falls back to in-memory txn

        await _sut.CreateAsync(ExpenseRequest(account, cat, 150m));

        account.Balance.Should().Be(850m, "expense of 150 debited from 1000");
        budget.CurrentSpend.Should().Be(150m, "budget spend recorded");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBudgets.Verify(r => r.UpdateAsync(budget, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Expense_NoBudget_SkipsBudgetUpdate()
    {
        var account = MakeOwnedAccount(balance: 1000m);
        var cat     = MakeOwnedCategory(TransactionType.Expense);
        SetupAccountMock(account);
        SetupCategoryMock(cat);

        // No budget found for this category/date
        _mockBudgets.Setup(r => r.GetByCategoryAndDateAsync(_userId, cat.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Budget?)null);
        _mockTxns.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction t, CancellationToken _) => t);
        _mockTxns.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction?)null);

        await _sut.CreateAsync(ExpenseRequest(account, cat, 150m));

        _mockBudgets.Verify(r => r.UpdateAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
        account.Balance.Should().Be(850m);
    }

    // ── CreateAsync — Income ──────────────────────────────

    [Fact]
    public async Task CreateAsync_Income_CreditsAccount_NoBudgetRecorded()
    {
        var account = MakeOwnedAccount(balance: 500m);
        var cat     = MakeOwnedCategory(TransactionType.Income);
        SetupAccountMock(account);
        SetupCategoryMock(cat);

        _mockTxns.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction t, CancellationToken _) => t);
        _mockTxns.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction?)null);

        await _sut.CreateAsync(IncomeRequest(account, cat, 200m));

        account.Balance.Should().Be(700m, "income of 200 credited to 500");
        _mockBudgets.Verify(r => r.GetByCategoryAndDateAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never,
            "budgets are never consulted for income");
    }

    // ── CreateAsync — Transfer ────────────────────────────

    [Fact]
    public async Task CreateAsync_Transfer_DebitsSource_CreditsDestination()
    {
        var source = MakeOwnedAccount(balance: 1000m);
        var dest   = MakeOwnedAccount(balance: 200m);
        var cat    = MakeOwnedCategory(TransactionType.Expense); // transfers need a category
        SetupAccountMock(source);
        SetupAccountMock(dest);
        SetupCategoryMock(cat);

        _mockTxns.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction t, CancellationToken _) => t);
        _mockTxns.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Transaction?)null);

        var request = new CreateTransactionRequest
        {
            Amount = 300m, Currency = CurrencyCode.USD,
            Type = TransactionType.Transfer, Description = "Move funds",
            TransactionDate = DateTime.UtcNow,
            AccountId = source.Id, CategoryId = cat.Id,
            DestinationAccountId = dest.Id
        };

        await _sut.CreateAsync(request);

        source.Balance.Should().Be(700m);
        dest.Balance.Should().Be(500m);
        // Budget spend is NOT recorded for transfers
        _mockBudgets.Verify(r => r.GetByCategoryAndDateAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── CreateAsync — InsufficientFunds ───────────────────

    [Fact]
    public async Task CreateAsync_InsufficientFunds_PropagatesInsufficientFundsException()
    {
        var account = MakeOwnedAccount(balance: 50m, type: AccountType.Checking);
        var cat     = MakeOwnedCategory(TransactionType.Expense);
        SetupAccountMock(account);
        SetupCategoryMock(cat);

        var act = () => _sut.CreateAsync(ExpenseRequest(account, cat, 500m)); // 500 > 50

        await act.Should().ThrowAsync<InsufficientFundsException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── CreateAsync — Ownership ───────────────────────────

    [Fact]
    public async Task CreateAsync_WrongUserAccount_ThrowsEntityNotFoundException()
    {
        var otherUser    = new User("other@x.com", "O", "U", "hash", CurrencyCode.USD);
        var otherAccount = otherUser.AddAccount("OtherAcc", AccountType.Checking, CurrencyCode.USD, 0m);

        _mockAccounts.Setup(r => r.GetByIdAsync(otherAccount.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(otherAccount);

        var act = () => _sut.CreateAsync(new CreateTransactionRequest
        {
            Amount = 10m, Currency = CurrencyCode.USD,
            Type = TransactionType.Expense, Description = "Hack",
            TransactionDate = DateTime.UtcNow,
            AccountId = otherAccount.Id, CategoryId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── DeleteAsync — Expense ─────────────────────────────

    [Fact]
    public async Task DeleteAsync_Expense_ReversesBalance_ReversesBudget_SavesOnce()
    {
        var account = MakeOwnedAccount(balance: 850m); // after a 150 expense was recorded
        var cat     = MakeOwnedCategory(TransactionType.Expense);

        var txn = new Transaction(150m, CurrencyCode.USD, TransactionType.Expense,
            "Test", DateTime.UtcNow, account.Id, cat.Id);

        // GetByIdAsync for transaction + for account ownership + for balance reversal
        _mockTxns.Setup(r => r.GetByIdAsync(txn.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(txn);
        // Both ownership check AND balance-reversal in the method body call Accounts.GetByIdAsync
        _mockAccounts.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(account);

        var budget = new Budget("Food", 500m, CurrencyCode.USD, BudgetPeriod.Monthly,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30), _userId, cat.Id);
        budget.RecordSpending(150m, allowExceed: true); // pre-loaded with the spend
        _mockBudgets.Setup(r => r.GetByCategoryAndDateAsync(_userId, cat.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(budget);

        await _sut.DeleteAsync(txn.Id);

        account.Balance.Should().Be(1000m, "expense reversed: 850 + 150 = 1000");
        budget.CurrentSpend.Should().Be(0m, "budget spend reversed: 150 - 150 = 0");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBudgets.Verify(r => r.UpdateAsync(budget, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Income_ReversesBalance_NoBudgetCall()
    {
        var account = MakeOwnedAccount(balance: 700m); // after a 200 income was recorded
        var cat     = MakeOwnedCategory(TransactionType.Income);

        var txn = new Transaction(200m, CurrencyCode.USD, TransactionType.Income,
            "Salary", DateTime.UtcNow, account.Id, cat.Id);

        _mockTxns.Setup(r => r.GetByIdAsync(txn.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(txn);
        _mockAccounts.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(account);

        await _sut.DeleteAsync(txn.Id);

        account.Balance.Should().Be(500m, "income reversed: 700 - 200 = 500");
        _mockBudgets.Verify(r => r.GetByCategoryAndDateAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Transfer_RestoresSource_RestoresDestination_NoBudget()
    {
        var source = MakeOwnedAccount(balance: 700m); // after 300 was transferred out
        var dest   = MakeOwnedAccount(balance: 500m); // after 300 was transferred in
        var cat    = MakeOwnedCategory(TransactionType.Expense);

        // Transfer transaction: source debited 300, dest credited 300
        var txn = new Transaction(300m, CurrencyCode.USD, "Move funds",
            DateTime.UtcNow, source.Id, dest.Id, cat.Id);

        _mockTxns.Setup(r => r.GetByIdAsync(txn.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(txn);
        // Ownership check uses source account; balance reversal also fetches both
        _mockAccounts.Setup(r => r.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(source);
        _mockAccounts.Setup(r => r.GetByIdAsync(dest.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(dest);

        await _sut.DeleteAsync(txn.Id);

        source.Balance.Should().Be(1000m, "source restored: 700 + 300 = 1000");
        dest.Balance.Should().Be(200m, "destination reversed: 500 - 300 = 200");
        _mockBudgets.Verify(r => r.GetByCategoryAndDateAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
