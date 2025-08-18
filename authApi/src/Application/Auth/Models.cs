namespace Application.Auth;

public sealed record RegisterRequest(string Email, string Username, string Password, string? Role = null);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, string RefreshToken, string Role);
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record GoogleLoginRequest(string IdToken);

public interface IAuthService
{
	Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
	Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
	Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);
}


