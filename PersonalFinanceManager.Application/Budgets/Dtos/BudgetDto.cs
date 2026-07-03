using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.Budgets.Dtos;

/// <summary>Response shape for a budget. Matches the Web BudgetDto.</summary>
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

	// Computed — mirrors Web BudgetDto properties so the client receives them pre-calculated.
	public decimal Remaining => Limit - CurrentSpend;
	public decimal PercentageUsed => Limit == 0 ? 0 : Math.Round(CurrentSpend / Limit * 100, 2);
	public bool IsExceeded => CurrentSpend >= Limit;
	public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

	public static BudgetDto FromEntity(Budget b, string? categoryName = null) => new()
	{
		Id = b.Id,
		Name = b.Name,
		Limit = b.Limit,
		CurrentSpend = b.CurrentSpend,
		Currency = b.Currency.ToString(),
		Period = b.Period.ToString(),
		StartDate = b.StartDate,
		EndDate = b.EndDate,
		CategoryId = b.CategoryId,
		CategoryName = categoryName ?? b.Category?.Name
	};
}
