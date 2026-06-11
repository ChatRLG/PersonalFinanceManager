namespace PersonalFinanceManager.Core.ValueObjects;
/// <summary>
/// Represents a postal address. Immutable value object.
/// Used for user profile information.
/// </summary>
public class Address : IEquatable<Address>
{
	public string Street { get; }
	public string City { get; }
	public string State { get; }
	public string PostalCode { get; }
	public string Country { get; }

	private Address()
	{
		Street = string.Empty;
		City = string.Empty;
		State = string.Empty;
		PostalCode = string.Empty;
		Country = string.Empty;
	}

	public Address(string street, string city, string state, string postalCode, string country)
	{
		Street = street ?? throw new ArgumentNullException(nameof(street));
		City = city ?? throw new ArgumentNullException(nameof(city));
		State = state ?? throw new ArgumentNullException(nameof(state));
		PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
		Country = country ?? throw new ArgumentNullException(nameof(country));
	}

	public bool Equals(Address? other)
	{
		if (other is null) return false;
		return Street == other.Street
		       && City == other.City
		       && State == other.State
		       && PostalCode == other.PostalCode
		       && Country == other.Country;
	}

	public override bool Equals(object? obj) => Equals(obj as Address);

	public override int GetHashCode() =>
		HashCode.Combine(Street, City, State, PostalCode, Country);

	public static bool operator ==(Address? left, Address? right) =>
		left is null ? right is null : left.Equals(right);

	public static bool operator !=(Address? left, Address? right) => !(left == right);

	public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}