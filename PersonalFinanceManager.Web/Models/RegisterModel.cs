using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class RegisterModel
{
	[Required(ErrorMessage = "First name is required")]
	[StringLength(100)]
	public string FirstName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Last name is required")]
	[StringLength(100)]
	public string LastName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email format")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password is required")]
	[MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
	public string Password { get; set; } = string.Empty;

	[Required(ErrorMessage = "Confirm password is required")]
	[Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
	public string ConfirmPassword { get; set; } = string.Empty;

	public string DefaultCurrency { get; set; } = "USD";
}