using FluentAssertions;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.ValueObjects;

namespace PersonalFinanceManager.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Add_SameCurrency_AccumulatesAmount()
    {
        var a = new Money(100m, CurrencyCode.USD);
        var b = new Money(50m, CurrencyCode.USD);

        var result = a.Add(b);

        result.Amount.Should().Be(150m);
        result.Currency.Should().Be(CurrencyCode.USD);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        var a = new Money(100m, CurrencyCode.USD);
        var b = new Money(100m, CurrencyCode.EUR);

        var act = () => a.Add(b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_SameCurrency_CanResultInNegative()
    {
        var a = new Money(50m, CurrencyCode.USD);
        var b = new Money(100m, CurrencyCode.USD);

        var result = a.Subtract(b);

        result.Amount.Should().Be(-50m);
    }

    [Fact]
    public void Negate_PositiveBecomesNegative()
    {
        var m = new Money(100m, CurrencyCode.USD);
        m.Negate().Amount.Should().Be(-100m);
    }

    [Fact]
    public void Negate_NegativeBecomesPositive()
    {
        var m = new Money(-100m, CurrencyCode.USD);
        m.Negate().Amount.Should().Be(100m);
    }

    [Fact]
    public void Negate_ZeroRemainsZero()
    {
        var m = new Money(0m, CurrencyCode.USD);
        m.Negate().Amount.Should().Be(0m);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = new Money(100m, CurrencyCode.USD);
        var b = new Money(100m, CurrencyCode.USD);

        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equality_SameAmountDifferentCurrency_NotEqual()
    {
        var a = new Money(100m, CurrencyCode.USD);
        var b = new Money(100m, CurrencyCode.EUR);

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void Zero_StaticFactory_ReturnsZeroAmount()
    {
        var zero = Money.Zero(CurrencyCode.USD);
        zero.Amount.Should().Be(0m);
        zero.IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsPositive_IsNegative_IsZero_WorkCorrectly()
    {
        new Money(1m, CurrencyCode.USD).IsPositive().Should().BeTrue();
        new Money(-1m, CurrencyCode.USD).IsNegative().Should().BeTrue();
        new Money(0m, CurrencyCode.USD).IsZero().Should().BeTrue();
    }
}
