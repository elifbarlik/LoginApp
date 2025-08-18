using Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
	private readonly IAdminService _adminService;

	public AdminController(IAdminService adminService)
	{
		_adminService = adminService;
	}

	[HttpGet("users")]
	public async Task<IActionResult> GetAllUsers(CancellationToken ct)
	{
		var users = await _adminService.GetAllUsersAsync(ct);
		return Ok(users);
	}

	[HttpGet("users/{role}")]
	public async Task<IActionResult> GetUsersByRole(string role, CancellationToken ct)
	{
		var users = await _adminService.GetUsersByRoleAsync(role, ct);
		return Ok(users);
	}

	[HttpPost("users")]
	public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
	{
		var user = await _adminService.CreateUserAsync(request, ct);
		return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
	}

	[HttpGet("users/{id:guid}")]
	public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
	{
		var users = await _adminService.GetAllUsersAsync(ct);
		var user = users.FirstOrDefault(u => u.Id == id);
		
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		
		return Ok(user);
	}

	[HttpPut("users/{id:guid}")]
	public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
	{
		try
		{
			var user = await _adminService.UpdateUserAsync(id, request, ct);
			return Ok(user);
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	[HttpDelete("users/{id:guid}")]
	public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
	{
		try
		{
			await _adminService.DeleteUserAsync(id, ct);
			return NoContent();
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	[HttpGet("stats")]
	public async Task<IActionResult> GetStats(CancellationToken ct)
	{
		var allUsers = await _adminService.GetAllUsersAsync(ct);
		var adminUsers = allUsers.Where(u => u.Role == "Admin").ToList();
		var regularUsers = allUsers.Where(u => u.Role == "User").ToList();

		var stats = new
		{
			TotalUsers = allUsers.Count(),
			AdminUsers = adminUsers.Count,
			RegularUsers = regularUsers.Count,
			RecentUsers = allUsers.OrderByDescending(u => u.CreatedAtUtc).Take(5)
		};

		return Ok(stats);
	}
}
