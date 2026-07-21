using FluentAssertions;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;

namespace PersonalFinanceManager.UnitTests.Domain;

public class BudgetTests
{
    private static readonly Guid _userId = Guid.NewGuid();
    private static readonly Guid _catId  = Guid.NewGuid();
    private static readonly DateTime _start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _end   = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

    private static Budget MakeBudget(
        string name = "Food",
        decimal limit = 500m,
        DateTime? start = null,
        DateTime? end = null) =>
        new(name, limit, CurrencyCode.USD, BudgetPeriod.Monthly,
            start ?? _start, end ?? _end, _userId, _catId);

    // ── Constructor guards ────────────────────────────────

    [Fact]
    public void Constructor_BlankName_ThrowsArgumentException()
    {
        var act = () => MakeBudget(name: "   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ZeroLimit_ThrowsArgumentException()
    {
        var act = () => MakeBudget(limit: 0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NegativeLimit_ThrowsArgumentException()
    {
        var act = () => MakeBudget(limit: -1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EndDateEqualsStartDate_ThrowsArgumentException()
    {
        // endDate <= startDate is rejected — equal dates are NOT valid
        var act = () => MakeBudget(start: _start, end: _start);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var act = () => MakeBudget(start: _end, end: _start);
        act.Should().Throw<ArgumentException>();
    }

    // ── RecordSpending ────────────────────────────────────

    [Fact]
    public void RecordSpending_ZeroAmount_ThrowsArgumentException()
    {
        var budget = MakeBudget();
        var act = () => budget.RecordSpending(0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordSpending_NegativeAmount_ThrowsArgumentException()
    {
        var budget = MakeBudget();
        var act = () => budget.RecordSpending(-10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordSpending_ExactlyToLimit_Succeeds()
    {
        // Guard is CurrentSpend + amount > Limit (strictly greater)
        // Spending that brings CurrentSpend == Limit must succeed.
        var budget = MakeBudget(limit: 500m);

        budget.RecordSpending(500m);

        budget.CurrentSpend.Should().Be(500m);
        budget.IsExceeded.Should().BeTrue(); // IsExceeded is >= Limit
    }

    [Fact]
    public void RecordSpending_ExceedsLimit_AllowExceedFalse_ThrowsBudgetExceededException()
    {
        var budget = MakeBudget(limit: 500m);
        budget.RecordSpending(300m, allowExceed: false); // first spend fine

        var act = () => budget.RecordSpending(250m, allowExceed: false); // 300+250=550 > 500

        var ex = act.Should().Throw<BudgetExceededException>().Which;
        ex.BudgetId.Should().Be(budget.Id);
        ex.BudgetLimit.Should().Be(500m);
        // AttemptedSpend is the projected TOTAL (CurrentSpend + amount), not just the delta
        ex.AttemptedSpend.Should().Be(550m);
    }

    [Fact]
    public void RecordSpending_ExceedsLimit_AllowExceedTrue_Succeeds()
    {
        var budget = MakeBudget(limit: 500m);

        budget.RecordSpending(600m, allowExceed: true);

        budget.CurrentSpend.Should().Be(600m);
        budget.IsExceeded.Should().BeTrue();
    }

    [Fact]
    public void RecordSpending_Accumulates()
    {
        var budget = MakeBudget(limit: 500m);
        budget.RecordSpending(100m);
        budget.RecordSpending(150m);

        budget.CurrentSpend.Should().Be(250m);
        budget.Remaining.Should().Be(250m);
    }

    // ── ReverseSpending ───────────────────────────────────

    [Fact]
    public void ReverseSpending_ZeroAmount_ThrowsArgumentException()
    {
        var budget = MakeBudget();
        budget.RecordSpending(100m, allowExceed: true);
        var act = () => budget.ReverseSpending(0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReverseSpending_NegativeAmount_ThrowsArgumentException()
    {
        var budget = MakeBudget();
        budget.RecordSpending(100m, allowExceed: true);
        var act = () => budget.ReverseSpending(-10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReverseSpending_LessThanCurrentSpend_Decrements()
    {
        var budget = MakeBudget();
        budget.RecordSpending(300m, allowExceed: true);

        budget.ReverseSpending(100m);

        budget.CurrentSpend.Should().Be(200m);
    }

    [Fact]
    public void ReverseSpending_ExactlyCurrentSpend_ReturnsZero()
    {
        var budget = MakeBudget();
        budget.RecordSpending(300m, allowExceed: true);

        budget.ReverseSpending(300m);

        budget.CurrentSpend.Should().Be(0m);
    }

    [Fact]
    public void ReverseSpending_MoreThanCurrentSpend_FloorsAtZero()
    {
        // Math.Max(0, CurrentSpend - amount) prevents going negative
        var budget = MakeBudget();
        budget.RecordSpending(100m, allowExceed: true);

        budget.ReverseSpending(999m);

        budget.CurrentSpend.Should().Be(0m);
    }

    // ── UpdateDetails ─────────────────────────────────────

    [Fact]
    public void UpdateDetails_BothDatesProvided_EndEqualsStart_ThrowsArgumentException()
    {
        var budget = MakeBudget();

        var act = () => budget.UpdateDetails("Food", _start, _start);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_OnlyStartDateProvided_DatesUnchanged()
    {
        // UpdateDetails only changes dates when BOTH are non-null
        var budget = MakeBudget(start: _start, end: _end);

        budget.UpdateDetails("NewName", startDate: new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), endDate: null);

        budget.StartDate.Should().Be(_start);
        budget.EndDate.Should().Be(_end);
        budget.Name.Should().Be("NewName");
    }

    [Fact]
    public void UpdateDetails_OnlyEndDateProvided_DatesUnchanged()
    {
        var budget = MakeBudget(start: _start, end: _end);

        budget.UpdateDetails("NewName", startDate: null, endDate: new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc));

        budget.StartDate.Should().Be(_start);
        budget.EndDate.Should().Be(_end);
    }

    [Fact]
    public void UpdateDetails_BothDatesValid_UpdatesBothDates()
    {
        var budget = MakeBudget();
        var newStart = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var newEnd   = new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc);

        budget.UpdateDetails("New", newStart, newEnd);

        budget.StartDate.Should().Be(newStart);
        budget.EndDate.Should().Be(newEnd);
    }

    // ── Computed properties ───────────────────────────────

    [Fact]
    public void PercentageUsed_Computed_Correctly()
    {
        var budget = MakeBudget(limit: 500m);
        budget.RecordSpending(250m, allowExceed: true);

        budget.PercentageUsed.Should().Be(50m);
    }

    [Fact]
    public void IsExceeded_TrueWhenCurrentSpendEqualsLimit()
    {
        var budget = MakeBudget(limit: 100m);
        budget.RecordSpending(100m); // exactly to limit

        budget.IsExceeded.Should().BeTrue();
    }
}
