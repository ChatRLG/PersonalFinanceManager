using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Transactions;

public class CreateTransactionRequest
{
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    /// <summary>Transaction type string (e.g. "Expense", "Income", "Transfer").</summary>
    public string Type { get; set; } = "Expense";

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.Today;

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>Required when Type == "Transfer".</summary>
    public Guid? DestinationAccountId { get; set; }
}
