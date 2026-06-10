namespace PersonalFinanceManager.Core.Entities;
/// <summary>
/// Abstract base class for all domain entities.
/// Provides common identity and audit fields.
/// </summary>
public abstract class BaseEntity
{
	public Guid Id { get; protected set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	protected BaseEntity()
	{
		Id = Guid.NewGuid();
		CreatedAt = DateTime.UtcNow;
		IsDeleted = false;
	}

	/// <summary>Soft-delete this entity.</summary>
	public void MarkAsDeleted()
	{
		IsDeleted = true;
		UpdatedAt = DateTime.UtcNow;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not BaseEntity other) return false;
		if (ReferenceEquals(this, other)) return true;
		if (GetType() != other.GetType()) return false;
		return Id == other.Id;
	}

	public override int GetHashCode() => Id.GetHashCode();

	public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
		left is null ? right is null : left.Equals(right);

	public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}