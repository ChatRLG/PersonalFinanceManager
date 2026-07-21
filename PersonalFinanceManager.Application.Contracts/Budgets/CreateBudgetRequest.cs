using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Budgets;

public class CreateBudgetRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
    public decimal Limit { get; set; }

    public string Currency { get; set; } = "USD";

    /// <summary>Budget period string (e.g. "Monthly", "Weekly", "Yearly").</summary>
    public string Period { get; set; } = "Monthly";

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
}
