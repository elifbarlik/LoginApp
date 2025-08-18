using Domain.Entities;
using System.Security.Claims;

namespace Application.Abstractions;

public interface IPasswordHasher
{
	string HashPassword(string password);
	bool Verify(string password, string passwordHash);
}

public interface IJwtTokenGenerator
{
	string GenerateAccessToken(Domain.Entities.User user);
	string GenerateRefreshToken();
	ClaimsPrincipal? ValidateToken(string token);
	ClaimsPrincipal? ValidateTokenWithoutLifetime(string token);
}

public interface IRefreshTokenValidator
{
	bool ValidateRefreshToken(string refreshToken);
}


