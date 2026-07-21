namespace PersonalFinanceManager.Desktop.Data.Entities;

/// <summary>
/// A transaction created while the API was unreachable.
/// Persisted locally until synced.
/// </summary>
public class OfflineTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Type { get; set; } = "Expense";
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? DestinationAccountId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; }
    public DateTime? SyncedAt { get; set; }
    public bool SyncFailed { get; set; }
    public string? SyncError { get; set; }
}
