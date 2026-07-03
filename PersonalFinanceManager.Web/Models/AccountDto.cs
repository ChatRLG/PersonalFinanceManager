namespace PersonalFinanceManager.Web.Models;

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

public class CreateAccountModel
{
	[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Account name is required")]
	[System.ComponentModel.DataAnnotations.StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[System.ComponentModel.DataAnnotations.Required]
	public string Type { get; set; } = "Checking";

	[System.ComponentModel.DataAnnotations.Required]
	public string Currency { get; set; } = "USD";

	public decimal InitialBalance { get; set; }

	public string? Description { get; set; }
}

public class UpdateAccountModel
{
	[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Account name is required")]
	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }
}