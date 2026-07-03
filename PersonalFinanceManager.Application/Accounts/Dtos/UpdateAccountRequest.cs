using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Accounts.Dtos;

public class UpdateAccountRequest
{
	[Required, StringLength(150)]
	public string Name { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Description { get; set; }
}
