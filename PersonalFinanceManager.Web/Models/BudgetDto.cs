using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

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
	public decimal Remaining => Limit - CurrentSpend;
	public decimal PercentageUsed => Limit == 0 ? 0 : Math.Round(CurrentSpend / Limit * 100, 2);
	public bool IsExceeded => CurrentSpend >= Limit;
	public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}

public class CreateBudgetModel
{
	[Required(ErrorMessage = "Budget name is required")]
	[StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero")]
	public decimal Limit { get; set; }

	[Required]
	public string Currency { get; set; } = "USD";

	[Required]
	public string Period { get; set; } = "Monthly";

	[Required]
	public DateTime StartDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

	[Required]
	public DateTime EndDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

	[Required(ErrorMessage = "Category is required")]
	public Guid CategoryId { get; set; }
}

public class UpdateBudgetModel
{
	[Required(ErrorMessage = "Budget name is required")]
	[StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than zero")]
	public decimal Limit { get; set; }

	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}
