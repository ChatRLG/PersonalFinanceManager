using FluentAssertions;
using PersonalFinanceManager.Core.ValueObjects;

namespace PersonalFinanceManager.UnitTests.Domain.ValueObjects;

public class DateRangeTests
{
    private static readonly DateTime Jan1  = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan15 = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan31 = new(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

    // ── Constructor ───────────────────────────────────────

    [Fact]
    public void Constructor_EndBeforeStart_ThrowsArgumentException()
    {
        var act = () => new DateRange(Jan31, Jan1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EqualDates_Valid_OneDayRange()
    {
        // Guard is endDate < startDate (strict) — equal dates are valid (1-day range)
        var range = new DateRange(Jan1, Jan1);
        range.DurationInDays.Should().Be(1);
    }

    [Fact]
    public void DurationInDays_InclusiveBothEnds()
    {
        var range = new DateRange(Jan1, Jan31);
        range.DurationInDays.Should().Be(31);
    }

    // ── Contains ─────────────────────────────────────────

    [Fact]
    public void Contains_StartDate_ReturnsTrue()
    {
        var range = new DateRange(Jan1, Jan31);
        range.Contains(Jan1).Should().BeTrue();
    }

    [Fact]
    public void Contains_EndDate_ReturnsTrue()
    {
        var range = new DateRange(Jan1, Jan31);
        range.Contains(Jan31).Should().BeTrue();
    }

    [Fact]
    public void Contains_DayBeforeStart_ReturnsFalse()
    {
        var range = new DateRange(Jan1, Jan31);
        range.Contains(Jan1.AddDays(-1)).Should().BeFalse();
    }

    [Fact]
    public void Contains_DayAfterEnd_ReturnsFalse()
    {
        var range = new DateRange(Jan1, Jan31);
        range.Contains(Jan31.AddDays(1)).Should().BeFalse();
    }

    [Fact]
    public void Contains_MiddleDate_ReturnsTrue()
    {
        var range = new DateRange(Jan1, Jan31);
        range.Contains(Jan15).Should().BeTrue();
    }

    // ── Overlaps ──────────────────────────────────────────

    [Fact]
    public void Overlaps_FullOverlap_ReturnsTrue()
    {
        var a = new DateRange(Jan1, Jan31);
        var b = new DateRange(Jan1, Jan31);
        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_PartialOverlap_ReturnsTrue()
    {
        var a = new DateRange(Jan1, Jan15);
        var b = new DateRange(Jan10, Jan31);
        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_TouchingAtBoundary_IsConsideredOverlap()
    {
        // Overlaps uses inclusive boundaries: StartDate <= other.EndDate && other.StartDate <= EndDate
        // Two ranges sharing a single boundary day DO overlap
        var a = new DateRange(Jan1, Jan15);
        var b = new DateRange(Jan15, Jan31);
        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_NonOverlapping_ReturnsFalse()
    {
        var a = new DateRange(Jan1, Jan14);
        var b = new DateRange(Jan15, Jan31);
        // Jan14 < Jan15 && Jan15 > Jan1 → but Jan14's EndDate is not >= Jan15's StartDate in an inclusive sense?
        // Check the actual implementation: StartDate <= other.EndDate && other.StartDate <= EndDate
        // a.StartDate(Jan1) <= b.EndDate(Jan31) = TRUE
        // b.StartDate(Jan15) <= a.EndDate(Jan14) = FALSE  →  no overlap
        a.Overlaps(b).Should().BeFalse();
    }

    // Helper
    private static readonly DateTime Jan10 = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan14 = new(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc);

    // ── Static factories ──────────────────────────────────

    [Fact]
    public void CurrentMonth_ContainsToday()
    {
        var range = DateRange.CurrentMonth();
        range.Contains(DateTime.UtcNow.Date).Should().BeTrue();
    }

    [Fact]
    public void ForMonth_February_LeapYear_HasCorrectEnd()
    {
        var range = DateRange.ForMonth(2024, 2); // 2024 is a leap year
        range.EndDate.Day.Should().Be(29);
        range.DurationInDays.Should().Be(29);
    }
}
