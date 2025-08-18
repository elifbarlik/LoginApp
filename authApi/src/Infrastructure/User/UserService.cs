using Application.Abstractions;
using Application.User;
using Domain.Entities;

namespace Infrastructure.User;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
		if (user == null)
		{
			throw new InvalidOperationException($"User with ID {userId} not found");
		}

		return MapToDto(user);
	}

	public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
		if (user == null)
		{
			throw new InvalidOperationException($"User with ID {userId} not found");
		}

		// Check if email is being changed and if it's already taken
		if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
		{
			var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
			if (existingUser != null)
			{
				throw new InvalidOperationException($"Email {request.Email} is already taken");
			}
			user.Email = request.Email;
		}

		if (!string.IsNullOrEmpty(request.Username))
		{
			user.Username = request.Username;
		}

		if (!string.IsNullOrEmpty(request.Phone))
		{
			user.Phone = request.Phone;
		}

		if (!string.IsNullOrEmpty(request.Address))
		{
			user.Address = request.Address;
		}

		// Role değişikliği burada yapılmaz; admin endpointleri üzerinden yapılmalıdır.

		await _userRepository.UpdateAsync(user, cancellationToken);
		return MapToDto(user);
	}

	private static UserProfileDto MapToDto(Domain.Entities.User user)
	{
		return new UserProfileDto(
			user.Id,
			user.Email,
			user.Username,
			user.Role,
			user.Phone,
			user.Address,
			user.CreatedAtUtc,
			user.LastLoginAtUtc
		);
	}
}
