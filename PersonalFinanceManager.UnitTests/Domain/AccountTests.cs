using FluentAssertions;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;

namespace PersonalFinanceManager.UnitTests.Domain;

/// <summary>
/// Tests for Account entity behaviour. Account constructor is internal —
/// every test creates an Account via User.AddAccount().
/// </summary>
public class AccountTests
{
    private static User CreateUser() =>
        new("test@x.com", "First", "Last", "hash", CurrencyCode.USD);

    // ── Credit ───────────────────────────────────────────

    [Fact]
    public void Credit_InactiveAccount_ThrowsInvalidOperationException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Savings", AccountType.Savings, CurrencyCode.USD, 500m);
        account.Deactivate();

        var act = () => account.Credit(100m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void Credit_ZeroAmount_ThrowsArgumentException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);

        var act = () => account.Credit(0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Credit_NegativeAmount_ThrowsArgumentException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);

        var act = () => account.Credit(-50m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Credit_ValidAmount_IncrementsBalance()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);

        account.Credit(200m);

        account.Balance.Should().Be(700m);
    }

    // ── Debit ────────────────────────────────────────────

    [Fact]
    public void Debit_InactiveAccount_ThrowsInvalidOperationException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);
        account.Deactivate();

        var act = () => account.Debit(100m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void Debit_ZeroAmount_ThrowsArgumentException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);

        var act = () => account.Debit(0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Debit_NegativeAmount_ThrowsArgumentException()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 500m);

        var act = () => account.Debit(-10m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(AccountType.Checking)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.Cash)]
    public void Debit_AssetAccountBelowZero_ThrowsInsufficientFundsException(AccountType type)
    {
        var user = CreateUser();
        var account = user.AddAccount("Account", type, CurrencyCode.USD, 100m);

        var act = () => account.Debit(150m);

        act.Should().Throw<InsufficientFundsException>()
            .And.AccountId.Should().Be(account.Id);
    }

    [Theory]
    [InlineData(AccountType.Checking)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.Cash)]
    public void Debit_AssetAccountExactlyToZero_Succeeds(AccountType type)
    {
        // Guard is `Balance - amount < 0`; zero result is on the allowed side.
        var user = CreateUser();
        var account = user.AddAccount("Account", type, CurrencyCode.USD, 100m);

        account.Debit(100m);

        account.Balance.Should().Be(0m);
    }

    [Theory]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.Loan)]
    [InlineData(AccountType.Investment)]
    public void Debit_NonAssetAccount_AllowsNegativeBalance(AccountType type)
    {
        var user = CreateUser();
        var account = user.AddAccount("Account", type, CurrencyCode.USD, 100m);

        account.Debit(200m); // goes below zero — no exception

        account.Balance.Should().Be(-100m);
    }

    // ── Activate / Deactivate ─────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 0m);

        account.Deactivate();

        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var user = CreateUser();
        var account = user.AddAccount("Checking", AccountType.Checking, CurrencyCode.USD, 0m);
        account.Deactivate();

        account.Activate();

        account.IsActive.Should().BeTrue();
    }
}
