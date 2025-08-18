using Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
	{
		var result = await _authService.RegisterAsync(request, ct);
		return Ok(result);
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
	{
		var result = await _authService.LoginAsync(request, ct);
		return Ok(result);
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
	{
		var result = await _authService.RefreshTokenAsync(request, ct);
		return Ok(result);
	}

	[HttpPost("google")]
	public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request, CancellationToken ct)
	{
		var result = await _authService.LoginWithGoogleAsync(request, ct);
		return Ok(result);
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
	{
		await _authService.LogoutAsync(request, ct);
		return Ok(new { message = "Logged out successfully" });
	}
}


