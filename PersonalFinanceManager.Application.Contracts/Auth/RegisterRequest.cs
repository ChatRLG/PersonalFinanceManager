using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Auth;

/// <summary>Payload to register a new user account.</summary>
public class RegisterRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public string DefaultCurrency { get; set; } = "USD";
}
