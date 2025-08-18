using Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("user")]
[Authorize]
public class UserController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly ILogger<UserController> _logger;

	public UserController(IUserService userService, ILogger<UserController> logger)
	{
		_userService = userService;
		_logger = logger;
	}

	[HttpGet("profile")]
	public async Task<IActionResult> GetProfile(CancellationToken ct)
	{
		// Token doğrulaması - AdminController'daki gibi
		if (!User.Identity?.IsAuthenticated ?? true)
		{
			return Unauthorized(new { message = "Invalid or expired token" });
		}

		// Debug için claim'leri logla
		_logger.LogInformation("GetProfile called. User: {User}, Claims: {Claims}", 
			User.Identity?.Name, 
			string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

		var userId = GetCurrentUserId();
		if (userId == null)
		{
			_logger.LogWarning("GetCurrentUserId returned null");
			return Unauthorized(new { message = "Invalid token claims" });
		}

		try
		{
			var profile = await _userService.GetProfileAsync(userId.Value, ct);
			return Ok(profile);
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	[HttpPut("profile")]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
	{
		// Token doğrulaması - AdminController'daki gibi
		if (!User.Identity?.IsAuthenticated ?? true)
		{
			return Unauthorized(new { message = "Invalid or expired token" });
		}

		var userId = GetCurrentUserId();
		if (userId == null)
		{
			return Unauthorized(new { message = "Invalid token claims" });
		}

		try
		{
			var profile = await _userService.UpdateProfileAsync(userId.Value, request, ct);
			return Ok(profile);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpGet("info")]
	public IActionResult GetUserInfo()
	{
		// Token doğrulaması - AdminController'daki gibi
		if (!User.Identity?.IsAuthenticated ?? true)
		{
			return Unauthorized(new { message = "Invalid or expired token" });
		}

		var userId = User.FindFirst("sub")?.Value;
		var email = User.FindFirst("email")?.Value;
		var role = User.FindFirst("role")?.Value;
		
		if (string.IsNullOrEmpty(userId))
		{
			return Unauthorized(new { message = "Invalid token claims" });
		}

		var userInfo = new
		{
			UserId = userId,
			Email = email,
			Role = role,
			TokenIssuedAt = DateTime.UtcNow
		};
		
		return Ok(userInfo);
	}

	private Guid? GetCurrentUserId()
	{
		// Önce "sub" claim'ini kontrol et
		var userIdClaim = User.FindFirst("sub")?.Value;
		
		// Eğer "sub" yoksa, "nameidentifier" claim'ini kontrol et
		if (string.IsNullOrEmpty(userIdClaim))
		{
			userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}
		
		// Eğer hala yoksa, User.Identity.Name'i kontrol et
		if (string.IsNullOrEmpty(userIdClaim))
		{
			userIdClaim = User.Identity?.Name;
		}

		if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
		{
			return null;
		}
		return userId;
	}
}
