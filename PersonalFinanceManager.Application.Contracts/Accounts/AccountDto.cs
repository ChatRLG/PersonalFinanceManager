namespace PersonalFinanceManager.Application.Contracts.Accounts;

/// <summary>Response shape for an account. Shared between Web, Desktop, and future clients.</summary>
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
}
