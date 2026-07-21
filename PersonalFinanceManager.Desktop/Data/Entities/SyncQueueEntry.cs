namespace PersonalFinanceManager.Desktop.Data.Entities;

/// <summary>
/// Queue of pending API operations to sync when the connection is restored.
/// Each entry represents one create, update, or delete operation.
/// </summary>
public class SyncQueueEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The domain entity type (e.g. "Transaction", "Account").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The server-side entity ID, if known (null for new creates before first sync).</summary>
    public Guid? EntityId { get; set; }

    /// <summary>The local offline entity ID for transactions created offline.</summary>
    public Guid? LocalEntityId { get; set; }

    /// <summary>The operation: "Create", "Update", or "Delete".</summary>
    public string OperationType { get; set; } = "Create";

    /// <summary>JSON-serialized request payload.</summary>
    public string Payload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SyncedAt { get; set; }
    public bool SyncFailed { get; set; }
    public string? SyncError { get; set; }
}
