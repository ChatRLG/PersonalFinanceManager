namespace PersonalFinanceManager.Core.Exceptions;

public class EntityNotFoundException : DomainException
{
	public string EntityName { get; }
	public object EntityId { get; }

	public EntityNotFoundException(string entityName, object entityId)
		: base($"Entity '{entityName}' with id '{entityId}' was not found.")
	{
		EntityName = entityName;
		EntityId = entityId;
	}
}
