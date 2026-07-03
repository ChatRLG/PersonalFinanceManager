using FluentAssertions;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.UnitTests.Domain;

public class TransactionTests
{
    private static readonly Guid _accountId = Guid.NewGuid();
    private static readonly Guid _destId    = Guid.NewGuid();
    private static readonly Guid _catId     = Guid.NewGuid();

    // ── Income/Expense constructor ────────────────────────

    [Fact]
    public void IncomeExpenseConstructor_TransferType_ThrowsArgumentException()
    {
        var act = () => new Transaction(
            100m, CurrencyCode.USD, TransactionType.Transfer,
            "desc", DateTime.UtcNow, _accountId, _catId);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*transfer constructor*");
    }

    [Fact]
    public void IncomeExpenseConstructor_ZeroAmount_ThrowsArgumentException()
    {
        var act = () => new Transaction(
            0m, CurrencyCode.USD, TransactionType.Expense,
            "desc", DateTime.UtcNow, _accountId, _catId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IncomeExpenseConstructor_BlankDescription_ThrowsArgumentException()
    {
        var act = () => new Transaction(
            100m, CurrencyCode.USD, TransactionType.Expense,
            "   ", DateTime.UtcNow, _accountId, _catId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IncomeExpenseConstructor_ValidIncome_SetsProperties()
    {
        var txn = new Transaction(
            250m, CurrencyCode.USD, TransactionType.Income,
            "Salary", DateTime.UtcNow, _accountId, _catId, "Monthly");

        txn.Amount.Should().Be(250m);
        txn.Type.Should().Be(TransactionType.Income);
        txn.AccountId.Should().Be(_accountId);
        txn.IsRecurring.Should().BeFalse();
    }

    // ── Transfer constructor ──────────────────────────────

    [Fact]
    public void TransferConstructor_SameSourceAndDestination_ThrowsArgumentException()
    {
        var act = () => new Transaction(
            100m, CurrencyCode.USD, "Transfer from savings",
            DateTime.UtcNow, _accountId, _accountId, _catId);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*different*");
    }

    [Fact]
    public void TransferConstructor_DifferentAccounts_SetsDestinationAccountId()
    {
        var txn = new Transaction(
            100m, CurrencyCode.USD, "Transfer",
            DateTime.UtcNow, _accountId, _destId, _catId);

        txn.Type.Should().Be(TransactionType.Transfer);
        txn.AccountId.Should().Be(_accountId);
        txn.DestinationAccountId.Should().Be(_destId);
    }

    // ── UpdateTransactionDate ─────────────────────────────

    [Fact]
    public void UpdateTransactionDate_ExactlyOneDayAhead_Succeeds()
    {
        // Guard is `newDate > DateTime.UtcNow.AddDays(1)` — strictly greater.
        // A date of UtcNow.Date.AddDays(1) (midnight tomorrow) is within the allowed window.
        var txn = new Transaction(
            100m, CurrencyCode.USD, TransactionType.Expense,
            "desc", DateTime.UtcNow, _accountId, _catId);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var act = () => txn.UpdateTransactionDate(tomorrow);

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateTransactionDate_TwoDaysAhead_ThrowsArgumentException()
    {
        var txn = new Transaction(
            100m, CurrencyCode.USD, TransactionType.Expense,
            "desc", DateTime.UtcNow, _accountId, _catId);

        var act = () => txn.UpdateTransactionDate(DateTime.UtcNow.AddDays(2));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateTransactionDate_PastDate_Succeeds()
    {
        var txn = new Transaction(
            100m, CurrencyCode.USD, TransactionType.Expense,
            "desc", DateTime.UtcNow, _accountId, _catId);

        var act = () => txn.UpdateTransactionDate(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        act.Should().NotThrow();
    }

    // ── IsRecurring ───────────────────────────────────────

    [Fact]
    public void MarkAsRecurring_ThenUnmark_TogglesCorrectly()
    {
        var txn = new Transaction(
            100m, CurrencyCode.USD, TransactionType.Expense,
            "desc", DateTime.UtcNow, _accountId, _catId);

        txn.MarkAsRecurring();
        txn.IsRecurring.Should().BeTrue();

        txn.UnmarkAsRecurring();
        txn.IsRecurring.Should().BeFalse();
    }
}
