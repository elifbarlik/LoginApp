using Domain.Entities;

namespace Application.User;

public sealed record UserProfileDto(
	Guid Id,
	string Email,
	string? Username,
	string Role,
	string? Phone,
	string? Address,
	DateTime CreatedAtUtc,
	DateTime? LastLoginAtUtc
);

public sealed record UpdateProfileRequest(string? Email, string? Username, string? Phone, string? Address);

public interface IUserService
{
	Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
