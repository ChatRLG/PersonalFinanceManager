namespace PersonalFinanceManager.Application.Contracts.Transactions;

/// <summary>Response shape for a transaction. Shared between Web, Desktop, and future clients.</summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool IsRecurring { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountName { get; set; }
}
