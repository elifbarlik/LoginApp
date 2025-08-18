using Application.Abstractions;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public sealed class JwtSettings
{
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	public int AccessTokenExpiryMinutes { get; set; } = 15; // Shorter for security
	public int RefreshTokenExpiryDays { get; set; } = 7;
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly JwtSettings _settings;

	public JwtTokenGenerator(IConfiguration configuration)
	{
		_settings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
	}

	public string GenerateAccessToken(Domain.Entities.User user)
	{
		var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
		var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Email, user.Email),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(ClaimTypes.Role, user.Role),
			new("role", user.Role)
        };

		var token = new JwtSecurityToken(
			issuer: _settings.Issuer,
			audience: _settings.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	public ClaimsPrincipal? ValidateToken(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			// Preserve original JWT claim types (keep "sub" instead of mapping to NameIdentifier)
			tokenHandler.InboundClaimTypeMap.Clear();
			tokenHandler.OutboundClaimTypeMap.Clear();
			// Program.cs ile aynı secret key kullanımı
			var secret = _settings.SecretKey ?? "dev-secret-change-me-please-1234567890";
			var key = Encoding.UTF8.GetBytes(secret);
			
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = _settings.Issuer,
				ValidateAudience = true,
				ValidAudience = _settings.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
				// Align with middleware so Name = sub and roles come from ClaimTypes.Role
				NameClaimType = JwtRegisteredClaimNames.Sub,
				RoleClaimType = ClaimTypes.Role
			};

			var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
			return principal;
		}
		catch
		{
			return null;
		}
	}

	public ClaimsPrincipal? ValidateTokenWithoutLifetime(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			// Preserve original JWT claim types (keep "sub" instead of mapping to NameIdentifier)
			tokenHandler.InboundClaimTypeMap.Clear();
			tokenHandler.OutboundClaimTypeMap.Clear();
			// Program.cs ile aynı secret key kullanımı
			var secret = _settings.SecretKey ?? "dev-secret-change-me-please-1234567890";
			var key = Encoding.UTF8.GetBytes(secret);
			
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = _settings.Issuer,
				ValidateAudience = true,
				ValidAudience = _settings.Audience,
				ValidateLifetime = false,
				ClockSkew = TimeSpan.Zero,
				// Align with middleware so Name = sub and roles come from ClaimTypes.Role
				NameClaimType = JwtRegisteredClaimNames.Sub,
				RoleClaimType = ClaimTypes.Role
			};

			var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
			return principal;
		}
		catch
		{
			return null;
		}
	}
}


