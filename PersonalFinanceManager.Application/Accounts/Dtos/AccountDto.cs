using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.Accounts.Dtos;

/// <summary>Response shape for an account. Field names intentionally match the Web AccountDto.</summary>
public class AccountDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Currency { get; set; } = string.Empty;
	public decimal Balance { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }

	public static AccountDto FromEntity(Account a) => new()
	{
		Id = a.Id,
		Name = a.Name,
		Type = a.Type.ToString(),
		Currency = a.Currency.ToString(),
		Balance = a.Balance,
		Description = a.Description,
		IsActive = a.IsActive,
		CreatedAt = a.CreatedAt
	};
}
