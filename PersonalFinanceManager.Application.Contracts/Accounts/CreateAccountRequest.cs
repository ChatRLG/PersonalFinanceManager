using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Accounts;

public class CreateAccountRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Account type string (e.g. "Checking", "Savings"). Kept as string so Contracts has no dependency on Core enums.</summary>
    public string Type { get; set; } = "Checking";

    public string Currency { get; set; } = "USD";

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; } = 0m;

    [StringLength(500)]
    public string? Description { get; set; }
}
