using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class CategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string? Icon { get; set; }
	public string? Colour { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class CreateCategoryModel
{
	[Required(ErrorMessage = "Category name is required")]
	[StringLength(100)]
	public string Name { get; set; } = string.Empty;

	[Required]
	public string Type { get; set; } = "Expense";

	public string? Icon { get; set; }
	public string? Colour { get; set; }
}