using FluentAssertions;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.UnitTests.Domain;

public class UserTests
{
    private static User CreateUser() =>
        new("test@x.com", "First", "Last", "hash", CurrencyCode.USD);

    // ── AddAccount ────────────────────────────────────────

    [Fact]
    public void AddAccount_ValidName_ReturnsAccountAndAddsToCollection()
    {
        var user = CreateUser();

        var account = user.AddAccount("Savings", AccountType.Savings, CurrencyCode.USD, 0m);

        account.Should().NotBeNull();
        user.Accounts.Should().Contain(account);
    }

    [Fact]
    public void AddAccount_DuplicateNameExactCase_ThrowsInvalidOperationException()
    {
        var user = CreateUser();
        user.AddAccount("Savings", AccountType.Savings, CurrencyCode.USD, 0m);

        var act = () => user.AddAccount("Savings", AccountType.Checking, CurrencyCode.USD, 0m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void AddAccount_DuplicateNameDifferentCase_ThrowsInvalidOperationException()
    {
        // Uniqueness check uses OrdinalIgnoreCase
        var user = CreateUser();
        user.AddAccount("savings", AccountType.Savings, CurrencyCode.USD, 0m);

        var act = () => user.AddAccount("SAVINGS", AccountType.Savings, CurrencyCode.USD, 0m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddAccount_SoftDeletedWithSameName_Allowed()
    {
        // A soft-deleted account is excluded from the uniqueness check
        var user = CreateUser();
        var existing = user.AddAccount("Savings", AccountType.Savings, CurrencyCode.USD, 0m);
        existing.MarkAsDeleted();

        var act = () => user.AddAccount("Savings", AccountType.Checking, CurrencyCode.USD, 0m);

        act.Should().NotThrow();
    }

    // ── AddCategory ───────────────────────────────────────

    [Fact]
    public void AddCategory_SameNameSameType_ThrowsInvalidOperationException()
    {
        var user = CreateUser();
        user.AddCategory("Groceries", TransactionType.Expense);

        var act = () => user.AddCategory("Groceries", TransactionType.Expense);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void AddCategory_SameNameDifferentType_Allowed()
    {
        // Income "Other" and Expense "Other" can coexist
        var user = CreateUser();
        user.AddCategory("Other", TransactionType.Income);

        var act = () => user.AddCategory("Other", TransactionType.Expense);

        act.Should().NotThrow();
        user.Categories.Count(c => c.Name == "Other").Should().Be(2);
    }

    [Fact]
    public void AddCategory_SoftDeletedSameNameSameType_Allowed()
    {
        var user = CreateUser();
        var cat = user.AddCategory("Groceries", TransactionType.Expense);
        cat.MarkAsDeleted();

        var act = () => user.AddCategory("Groceries", TransactionType.Expense);

        act.Should().NotThrow();
    }

    // ── AddBudget ─────────────────────────────────────────

    private static (User user, Guid catId, DateTime start, DateTime end) BudgetSetup()
    {
        var user = CreateUser();
        var cat  = user.AddCategory("Food", TransactionType.Expense);
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end   = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        return (user, cat.Id, start, end);
    }

    [Fact]
    public void AddBudget_CategoryNotInUser_ThrowsInvalidOperationException()
    {
        var (user, _, start, end) = BudgetSetup();
        var foreignCatId = Guid.NewGuid();

        var act = () => user.AddBudget("Food", 500m, CurrencyCode.USD,
            BudgetPeriod.Monthly, start, end, foreignCatId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void AddBudget_SoftDeletedCategory_ThrowsInvalidOperationException()
    {
        var (user, catId, start, end) = BudgetSetup();
        var cat = user.Categories.First(c => c.Id == catId);
        cat.MarkAsDeleted();

        var act = () => user.AddBudget("Food", 500m, CurrencyCode.USD,
            BudgetPeriod.Monthly, start, end, catId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void AddBudget_OverlappingDates_SameCategory_ThrowsInvalidOperationException()
    {
        var (user, catId, start, end) = BudgetSetup();
        user.AddBudget("Food Jan", 500m, CurrencyCode.USD, BudgetPeriod.Monthly, start, end, catId);

        // Overlapping period
        var act = () => user.AddBudget("Food Jan2", 500m, CurrencyCode.USD, BudgetPeriod.Monthly,
            start.AddDays(10), end.AddDays(10), catId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*overlapping*");
    }

    [Fact]
    public void AddBudget_AdjacentDates_SameCategory_Allowed()
    {
        // Guard: b.StartDate < newEnd && b.EndDate > newStart
        // Adjacent (touching) budgets satisfy neither simultaneously.
        var (user, catId, start, end) = BudgetSetup();
        user.AddBudget("Jan", 500m, CurrencyCode.USD, BudgetPeriod.Monthly, start, end, catId);

        // Feb starts exactly when Jan ends
        var febStart = end;          // Jan 31 = Feb start
        var febEnd   = end.AddDays(28);

        var act = () => user.AddBudget("Feb", 500m, CurrencyCode.USD, BudgetPeriod.Monthly,
            febStart, febEnd, catId);

        act.Should().NotThrow();
    }

    [Fact]
    public void AddBudget_DeletedBudgetWithOverlap_Allowed()
    {
        var (user, catId, start, end) = BudgetSetup();
        var budget = user.AddBudget("Jan", 500m, CurrencyCode.USD, BudgetPeriod.Monthly, start, end, catId);
        budget.MarkAsDeleted();

        // Same dates — should succeed because the existing budget is soft-deleted
        var act = () => user.AddBudget("Jan2", 500m, CurrencyCode.USD, BudgetPeriod.Monthly,
            start, end, catId);

        act.Should().NotThrow();
    }

    [Fact]
    public void AddBudget_ValidRequest_AddsToBudgetCollection()
    {
        var (user, catId, start, end) = BudgetSetup();

        var budget = user.AddBudget("Food", 500m, CurrencyCode.USD, BudgetPeriod.Monthly, start, end, catId);

        user.Budgets.Should().Contain(budget);
        budget.Limit.Should().Be(500m);
        budget.CategoryId.Should().Be(catId);
    }

    // ── Email / Name validation ───────────────────────────

    [Fact]
    public void Constructor_BlankEmail_ThrowsArgumentException()
    {
        var act = () => new User("   ", "F", "L", "hash", CurrencyCode.USD);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_InvalidEmailFormat_ThrowsArgumentException()
    {
        var act = () => new User("notanemail", "F", "L", "hash", CurrencyCode.USD);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullPasswordHash_ThrowsArgumentNullException()
    {
        var act = () => new User("a@b.com", "F", "L", null!, CurrencyCode.USD);
        act.Should().Throw<ArgumentNullException>();
    }
}
