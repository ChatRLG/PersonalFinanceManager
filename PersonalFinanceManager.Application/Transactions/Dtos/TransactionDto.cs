using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.Transactions.Dtos;

/// <summary>Response shape for a transaction. Matches the Web TransactionDto.</summary>
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

	public static TransactionDto FromEntity(Transaction t) => new()
	{
		Id = t.Id,
		Amount = t.Amount,
		Currency = t.Currency.ToString(),
		Type = t.Type.ToString(),
		Description = t.Description,
		Notes = t.Notes,
		TransactionDate = t.TransactionDate,
		IsRecurring = t.IsRecurring,
		AccountId = t.AccountId,
		AccountName = t.Account?.Name,
		CategoryId = t.CategoryId,
		CategoryName = t.Category?.Name,
		DestinationAccountId = t.DestinationAccountId,
		DestinationAccountName = t.DestinationAccount?.Name
	};
}
