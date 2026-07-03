using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.Transactions.Dtos;

public class CreateTransactionRequest
{
	[Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
	public decimal Amount { get; set; }

	public CurrencyCode Currency { get; set; } = CurrencyCode.USD;

	public TransactionType Type { get; set; } = TransactionType.Expense;

	[Required, StringLength(500)]
	public string Description { get; set; } = string.Empty;

	[StringLength(1000)]
	public string? Notes { get; set; }

	[Required]
	public DateTime TransactionDate { get; set; } = DateTime.Today;

	[Required]
	public Guid AccountId { get; set; }

	[Required]
	public Guid CategoryId { get; set; }

	/// <summary>Required when Type == Transfer.</summary>
	public Guid? DestinationAccountId { get; set; }
}
