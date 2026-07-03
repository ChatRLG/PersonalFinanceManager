using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Infrastructure.Auth;

/// <summary>
/// Issues HS256-signed JWTs. Claims use the standard short names ("sub", "email",
/// …) without inbound/outbound remapping, so the token reads identically on the
/// API (validation) and the Blazor client (which parses "email").
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly JwtSettings _settings;

	public JwtTokenGenerator(IOptions<JwtSettings> settings)
	{
		_settings = settings.Value;
	}

	public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
	{
		var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Email, user.Email),
			new(JwtRegisteredClaimNames.GivenName, user.FirstName),
			new(JwtRegisteredClaimNames.FamilyName, user.LastName),
			new("name", user.FullName),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _settings.Issuer,
			audience: _settings.Audience,
			claims: claims,
			notBefore: DateTime.UtcNow,
			expires: expiresAtUtc,
			signingCredentials: credentials);

		var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
		return (tokenString, expiresAtUtc);
	}
}
