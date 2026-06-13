using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class CreateAccountModel
{
	[Required(ErrorMessage = "Account name is required.")]
	[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters.")]
	public string Name { get; set; } = string.Empty;

	[Required(ErrorMessage = "Account type is required.")]
	public string Type { get; set; } = string.Empty;

	public string Currency { get; set; } = "USD";

	[Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative.")]
	public decimal InitialBalance { get; set; }

	public string? Description { get; set; }
}