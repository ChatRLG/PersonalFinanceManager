using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Application.Contracts.Auth;

/// <summary>Payload to authenticate an existing user.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
