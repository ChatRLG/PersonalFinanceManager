using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Budgets.Dtos;

public class UpdateBudgetRequest
{
	[Required, StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[Required, Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
	public decimal Limit { get; set; }

	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}
