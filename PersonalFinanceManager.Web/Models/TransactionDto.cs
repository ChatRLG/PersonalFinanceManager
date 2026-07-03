using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class TransactionDto
{
	public Guid Id { get; set; }
	public decimal Amount { get; set; }
	public string Currency { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string? Notes { get; set; }
	public DateTime TransactionDate { get; set; }
	public bool IsRecurring { get; set; }
	public Guid AccountId { get; set; }
	public string? AccountName { get; set; }
	public Guid CategoryId { get; set; }
	public string? CategoryName { get; set; }
	public Guid? DestinationAccountId { get; set; }
	public string? DestinationAccountName { get; set; }
}

public class CreateTransactionModel
{
	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
	public decimal Amount { get; set; }

	[Required]
	public string Currency { get; set; } = "USD";

	[Required]
	public string Type { get; set; } = "Expense";

	[Required(ErrorMessage = "Description is required")]
	[StringLength(500)]
	public string Description { get; set; } = string.Empty;

	public string? Notes { get; set; }

	[Required]
	public DateTime TransactionDate { get; set; } = DateTime.Today;

	[Required(ErrorMessage = "Account is required")]
	public Guid AccountId { get; set; }

	[Required(ErrorMessage = "Category is required")]
	public Guid CategoryId { get; set; }

	public Guid? DestinationAccountId { get; set; }
}