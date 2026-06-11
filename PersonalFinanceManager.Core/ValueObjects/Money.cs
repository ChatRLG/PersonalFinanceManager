using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Core.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency. Immutable value object.
/// Two Money instances are equal if their Amount and Currency match.
/// </summary>
public class Money : IEquatable<Money>
{
	public decimal Amount { get; }
	public CurrencyCode Currency { get; }

	// EF Core needs a parameterless constructor (private is fine)
	private Money() { }

	public Money(decimal amount, CurrencyCode currency)
	{
		Amount = amount;
		Currency = currency;
	}

	/// <summary>
	/// Creates a Money with zero amount in the given currency.
	/// </summary>
	public static Money Zero(CurrencyCode currency) => new(0m, currency);

	public Money Add(Money other)
	{
		EnsureSameCurrency(other);
		return new Money(Amount + other.Amount, Currency);
	}

	public Money Subtract(Money other)
	{
		EnsureSameCurrency(other);
		return new Money(Amount - other.Amount, Currency);
	}

	public Money Negate() => new(-Amount, Currency);

	public bool IsNegative() => Amount < 0;
	public bool IsZero() => Amount == 0;
	public bool IsPositive() => Amount > 0;

	private void EnsureSameCurrency(Money other)
	{
		if (Currency != other.Currency)
			throw new InvalidOperationException($"Cannot perform operation on different currencies: {Currency} and {other.Currency}.");
	}

	// --- Equality Members ---
	public bool Equals(Money? other)
	{
		if (other is null) return false;
		return Amount == other.Amount && Currency == other.Currency;
	}

	public override bool Equals(object? obj) => Equals(obj as Money);

	public override int GetHashCode() => HashCode.Combine(Amount, Currency);

	public static bool operator ==(Money? left, Money? right) =>
		left is null ? right is null : left.Equals(right);

	public static bool operator !=(Money? left, Money? right) => !(left == right);

	public override string ToString() => $"{Amount:N2} {Currency}";
}