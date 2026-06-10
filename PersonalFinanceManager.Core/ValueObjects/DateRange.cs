namespace PersonalFinanceManager.Core.ValueObjects;
/// <summary>
/// Represents a range between two dates. Immutable value object.
/// Used for budget periods, report date ranges, etc.
/// </summary>
public class DateRange : IEquatable<DateRange>
{
	public DateTime StartDate { get; }
	public DateTime EndDate { get; }

	private DateRange() { }

	public DateRange(DateTime startDate, DateTime endDate)
	{
		if (endDate < startDate)
			throw new ArgumentException("End date cannot be before start date.");

		StartDate = startDate;
		EndDate = endDate;
	}

	/// <summary>Number of days in this range (inclusive).</summary>
	public int DurationInDays => (EndDate - StartDate).Days + 1;

	/// <summary>Returns true if the given date falls within this range (inclusive).</summary>
	public bool Contains(DateTime date) => date >= StartDate && date <= EndDate;

	/// <summary>Returns true if two date ranges overlap.</summary>
	public bool Overlaps(DateRange other) =>
		StartDate <= other.EndDate && other.StartDate <= EndDate;

	/// <summary>Creates a DateRange for the current calendar month.</summary>
	public static DateRange CurrentMonth()
	{
		var now = DateTime.UtcNow;
		var start = new DateTime(now.Year, now.Month, 1);
		var end = start.AddMonths(1).AddDays(-1);
		return new DateRange(start, end);
	}

	/// <summary>Creates a DateRange for a specific month and year.</summary>
	public static DateRange ForMonth(int year, int month)
	{
		var start = new DateTime(year, month, 1);
		var end = start.AddMonths(1).AddDays(-1);
		return new DateRange(start, end);
	}

	// --- Equality Members ---
	public bool Equals(DateRange? other)
	{
		if (other is null) return false;
		return StartDate == other.StartDate && EndDate == other.EndDate;
	}

	public override bool Equals(object? obj) => Equals(obj as DateRange);
	public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);

	public static bool operator ==(DateRange? left, DateRange? right) =>
		left is null ? right is null : left.Equals(right);

	public static bool operator !=(DateRange? left, DateRange? right) => !(left == right);

	public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
