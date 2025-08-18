using Domain.Entities;

namespace Application.Admin;

public sealed record UserDto(
	Guid Id,
	string Email,
	string Role,
	DateTime CreatedAtUtc,
	DateTime? LastLoginAtUtc
);

public sealed record CreateUserRequest(string Email, string Password, string Role);
public sealed record UpdateUserRequest(string Email, string? Role);
public sealed record DeleteUserRequest(Guid Id);

public interface IAdminService
{
	Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
	Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
	Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
	Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
	Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}
