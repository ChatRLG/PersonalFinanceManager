using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Categories;

public class CreateCategoryRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Transaction type string (e.g. "Expense", "Income").</summary>
    public string Type { get; set; } = "Expense";

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(7)]
    public string? Colour { get; set; }
}
