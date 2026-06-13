using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class CreateTransactionModel
{
	[Required(ErrorMessage = "Transaction type is required.")]
	public string Type { get; set; } = string.Empty;

	[Required(ErrorMessage = "Description is required.")]
	[StringLength(200, MinimumLength = 2)]
	public string Description { get; set; } = string.Empty;

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
	public decimal Amount { get; set; }

	public string Currency { get; set; } = "USD";

	[Required(ErrorMessage = "Account is required.")]
	public string AccountId { get; set; } = string.Empty;

	public string? DestinationAccountId { get; set; }

	[Required(ErrorMessage = "Category is required.")]
	public string CategoryId { get; set; } = string.Empty;

	[Required]
	public DateTime TransactionDate { get; set; } = DateTime.Today;

	public string? Notes { get; set; }
}