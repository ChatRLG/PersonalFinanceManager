using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.Categories.Dtos;

public class CreateCategoryRequest
{
	[Required, StringLength(100)]
	public string Name { get; set; } = string.Empty;

	public TransactionType Type { get; set; } = TransactionType.Expense;

	[StringLength(50)]
	public string? Icon { get; set; }

	[StringLength(7)]
	public string? Colour { get; set; }
}
