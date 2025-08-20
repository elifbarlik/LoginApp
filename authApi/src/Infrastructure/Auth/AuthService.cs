using Application.Abstractions;
using Application.Auth;
using Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auth;

public class AuthService : IAuthService
{
	private readonly IUserRepository _users;
	private readonly IRefreshTokenRepository _refreshTokens;
	private readonly IPasswordHasher _hasher;
	private readonly IJwtTokenGenerator _jwt;
	private readonly IRefreshTokenValidator _refreshTokenValidator;
	private readonly string[] _googleClientIds;

	public AuthService(
		IUserRepository users, 
		IRefreshTokenRepository refreshTokens,
		IPasswordHasher hasher, 
		IJwtTokenGenerator jwt,
		IRefreshTokenValidator refreshTokenValidator,
		IConfiguration configuration)
	{
		_users = users;
		_refreshTokens = refreshTokens;
		_hasher = hasher;
		_jwt = jwt;
		_refreshTokenValidator = refreshTokenValidator;
		_googleClientIds = configuration.GetSection("Google:ClientIds").Get<string[]>()
			?? (configuration["Google:ClientId"] is string single && !string.IsNullOrWhiteSpace(single)
				? new[] { single }
				: Array.Empty<string>());
	}

	public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
	{
		var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
		if (existing is not null)
		{
			// Use a consistent message that ErrorController maps to 409 Conflict
			throw new InvalidOperationException("Email already exists");
		}

		var user = new Domain.Entities.User
		{
			Email = request.Email,
			Username = request.Username,
			PasswordHash = _hasher.HashPassword(request.Password),
			Role = request.Role ?? "User" // Default to User role if not specified
		};

		await _users.AddAsync(user, cancellationToken);

		var accessToken = _jwt.GenerateAccessToken(user);
		var refreshToken = _jwt.GenerateRefreshToken();
		
		// Store refresh token
		var refreshTokenEntity = new RefreshToken
		{
			UserId = user.Id,
			Token = refreshToken,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(7) // 7 days expiry
		};
		
		await _refreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

		return new AuthResponse(accessToken, refreshToken, user.Role);
	}

	public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
		if (user is null)
		{
			throw new UnauthorizedAccessException("Invalid credentials");
		}

		if (!_hasher.Verify(request.Password, user.PasswordHash))
		{
			throw new UnauthorizedAccessException("Invalid credentials");
		}

		// Update last login
		user.LastLoginAtUtc = DateTime.UtcNow;
		await _users.UpdateAsync(user, cancellationToken);

		var accessToken = _jwt.GenerateAccessToken(user);
		var refreshToken = _jwt.GenerateRefreshToken();
		
		// Store refresh token
		var refreshTokenEntity = new RefreshToken
		{
			UserId = user.Id,
			Token = refreshToken,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
		};
		
		await _refreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

		return new AuthResponse(accessToken, refreshToken, user.Role);
	}

	public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
	{
		// Validate the expired access token to get user info (without lifetime validation)
		var principal = _jwt.ValidateTokenWithoutLifetime(request.AccessToken);
		if (principal == null)
		{
			throw new UnauthorizedAccessException("Invalid access token");
		}

		// Get user ID from token (prefer "sub", fallback to NameIdentifier)
		var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
			?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
			?? principal.Identity?.Name;
		if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
		{
			throw new UnauthorizedAccessException("Invalid token claims");
		}

		// Validate refresh token
		var refreshTokenEntity = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
		if (refreshTokenEntity == null || refreshTokenEntity.UserId != userId)
		{
			throw new UnauthorizedAccessException("Invalid refresh token");
		}

		if (refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiresAtUtc < DateTime.UtcNow)
		{
			throw new UnauthorizedAccessException("Refresh token expired or revoked");
		}

		// Get user
		var user = await _users.GetByIdAsync(userId, cancellationToken);
		if (user == null)
		{
			throw new UnauthorizedAccessException("User not found");
		}

		// Generate new tokens
		var newAccessToken = _jwt.GenerateAccessToken(user);
		var newRefreshToken = _jwt.GenerateRefreshToken();

		// Revoke old refresh token
		refreshTokenEntity.IsRevoked = true;
		await _refreshTokens.UpdateAsync(refreshTokenEntity, cancellationToken);

		// Store new refresh token
		var newRefreshTokenEntity = new RefreshToken
		{
			UserId = user.Id,
			Token = newRefreshToken,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
		};
		
		await _refreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);

		return new AuthResponse(newAccessToken, newRefreshToken, user.Role);
	}

	public async Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
	{
		if (_googleClientIds.Length == 0)
		{
			throw new InvalidOperationException("Google ClientId is not configured");
		}

		var settings = new GoogleJsonWebSignature.ValidationSettings
		{
			Audience = _googleClientIds
		};

		GoogleJsonWebSignature.Payload payload;
		try
		{
			payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
		}
		catch (Exception)
		{
			throw new UnauthorizedAccessException("Invalid Google token");
		}

		if (payload.EmailVerified != true)
		{
			throw new UnauthorizedAccessException("Unverified Google email");
		}

		var user = await _users.GetByEmailAsync(payload.Email, cancellationToken);
		if (user is null)
		{
			user = new Domain.Entities.User
			{
				Email = payload.Email,
				PasswordHash = _hasher.HashPassword(Guid.NewGuid().ToString("N")),
				Role = "User"
			};
			await _users.AddAsync(user, cancellationToken);
		}

		var accessToken = _jwt.GenerateAccessToken(user);
		var refreshToken = _jwt.GenerateRefreshToken();

		var refreshTokenEntity = new RefreshToken
		{
			UserId = user.Id,
			Token = refreshToken,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
		};
		await _refreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

		return new AuthResponse(accessToken, refreshToken, user.Role);
	}

	public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
	{
		if (!_refreshTokenValidator.ValidateRefreshToken(request.RefreshToken))
		{
			throw new ArgumentException("Invalid refresh token format");
		}

		await _refreshTokens.RevokeTokenAsync(request.RefreshToken, cancellationToken);
	}
}


