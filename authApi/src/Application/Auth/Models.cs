using System.ComponentModel.DataAnnotations;

namespace Application.Auth;

public sealed record RegisterRequest
{
	[Required]
	[EmailAddress]
	[StringLength(254)]
	[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must include a domain (e.g., example.com)")]
	public string Email { get; init; } = string.Empty;

	[Required]
	[MinLength(3)]
	[MaxLength(32)]
	[RegularExpression("^(?!.*@)[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, ., _ or - and cannot be an email")]
	public string Username { get; init; } = string.Empty;

	[Required]
	[MinLength(8)]
	[MaxLength(128)]
	[RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z])\\S{8,128}$", ErrorMessage = "Password must include upper, lower, number, symbol and contain no spaces")]
	public string Password { get; init; } = string.Empty;

	[RegularExpression("^(Admin|User)$", ErrorMessage = "Invalid role")]
	public string? Role { get; init; }
}

public sealed record LoginRequest
{
	[Required]
	[EmailAddress]
	[StringLength(254)]
	[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must include a domain (e.g., example.com)")]
	public string Email { get; init; } = string.Empty;

	[Required]
	[MaxLength(128)]
	public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(string AccessToken, string RefreshToken, string Role);

public sealed record RefreshTokenRequest
{
	[Required]
	[StringLength(2048)]
	public string AccessToken { get; init; } = string.Empty;

	[Required]
	[StringLength(2048)]
	public string RefreshToken { get; init; } = string.Empty;
}

public sealed record LogoutRequest
{
	[Required]
	[StringLength(2048)]
	public string RefreshToken { get; init; } = string.Empty;
}

public sealed record GoogleLoginRequest
{
	[Required]
	[StringLength(4096)]
	public string IdToken { get; init; } = string.Empty;
}

public interface IAuthService
{
	Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
	Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
}


