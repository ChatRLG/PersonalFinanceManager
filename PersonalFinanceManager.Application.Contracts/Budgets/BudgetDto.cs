namespace PersonalFinanceManager.Application.Contracts.Budgets;

/// <summary>Response shape for a budget. Shared between Web, Desktop, and future clients.</summary>
public class BudgetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal CurrentSpend { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Computed properties — pre-calculated by the server and sent to all clients.
    public decimal Remaining => Limit - CurrentSpend;
    public decimal PercentageUsed => Limit == 0 ? 0 : Math.Round(CurrentSpend / Limit * 100, 2);
    public bool IsExceeded => CurrentSpend >= Limit;
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}
