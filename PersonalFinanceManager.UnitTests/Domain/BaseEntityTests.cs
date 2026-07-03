using FluentAssertions;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.UnitTests.Domain;

public class BaseEntityTests
{
    // Private concrete subclass — lets us test BaseEntity without a real entity
    private sealed class TestEntity : BaseEntity { }
    private sealed class OtherEntity : BaseEntity { }

    [Fact]
    public void Constructor_AssignsNonEmptyGuid()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_TwoInstances_HaveDifferentIds()
    {
        var a = new TestEntity();
        var b = new TestEntity();
        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Constructor_CreatedAtIsApproximatelyNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new TestEntity();
        var after  = DateTime.UtcNow.AddSeconds(1);

        entity.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Constructor_IsDeletedFalse()
    {
        var entity = new TestEntity();
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Constructor_UpdatedAtIsNull()
    {
        var entity = new TestEntity();
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsDeleted_SetsIsDeletedTrue()
    {
        var entity = new TestEntity();
        entity.MarkAsDeleted();
        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void MarkAsDeleted_StampsUpdatedAt()
    {
        var entity = new TestEntity();
        var before = DateTime.UtcNow.AddSeconds(-1);

        entity.MarkAsDeleted();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void Equals_SameIdSameConcreteType_ReturnsTrue()
    {
        var a = new TestEntity();
        // Manually construct a second with the same Id via the copy pattern the code uses
        var b = new TestEntity();

        // Force same Id using reflection (protected setter)
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(b, a.Id);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIdDifferentConcreteType_ReturnsFalse()
    {
        var a = new TestEntity();
        var b = new OtherEntity();

        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(b, a.Id);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new TestEntity();
        var b = new TestEntity();
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_SameInstance_ReturnsTrue()
    {
        var a = new TestEntity();
#pragma warning disable CS1718
        (a == a).Should().BeTrue();
#pragma warning restore CS1718
    }
}
