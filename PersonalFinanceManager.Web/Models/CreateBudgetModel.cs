using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class CreateBudgetModel
{
	[Required(ErrorMessage = "Budget name is required.")]
	[StringLength(100, MinimumLength = 2)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
	public decimal Limit { get; set; }

	public string Currency { get; set; } = "USD";

	[Required(ErrorMessage = "Category is required.")]
	public string CategoryId { get; set; } = string.Empty;

	[Required(ErrorMessage = "Period is required.")]
	public string Period { get; set; } = string.Empty;

	[Required]
	public DateTime StartDate { get; set; }

	[Required]
	public DateTime EndDate { get; set; }
}