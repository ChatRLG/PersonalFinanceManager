using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.Budgets.Dtos;

public class CreateBudgetRequest
{
	[Required, StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[Required, Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero.")]
	public decimal Limit { get; set; }

	public CurrencyCode Currency { get; set; } = CurrencyCode.USD;

	public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;

	[Required]
	public DateTime StartDate { get; set; }

	[Required]
	public DateTime EndDate { get; set; }

	[Required]
	public Guid CategoryId { get; set; }
}
