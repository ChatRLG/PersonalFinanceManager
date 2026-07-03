using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.Accounts.Dtos;

public class CreateAccountRequest
{
	[Required, StringLength(150)]
	public string Name { get; set; } = string.Empty;

	public AccountType Type { get; set; } = AccountType.Checking;

	public CurrencyCode Currency { get; set; } = CurrencyCode.USD;

	[Range(0, double.MaxValue)]
	public decimal InitialBalance { get; set; } = 0m;

	[StringLength(500)]
	public string? Description { get; set; }
}
