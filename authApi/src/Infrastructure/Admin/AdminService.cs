using Application.Abstractions;
using Application.Admin;
using Domain.Entities;

namespace Infrastructure.Admin;

public class AdminService : IAdminService
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordHasher _passwordHasher;

	public AdminService(IUserRepository userRepository, IPasswordHasher passwordHasher)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
	}

	public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
	{
		var users = await _userRepository.GetAllAsync(cancellationToken);
		return users.Select(MapToDto);
	}

	public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
	{
		var users = await _userRepository.GetByRoleAsync(role, cancellationToken);
		return users.Select(MapToDto);
	}

	public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
	{
		var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
		if (existingUser != null)
		{
			throw new InvalidOperationException($"User with email {request.Email} already exists");
		}

		var user = new Domain.Entities.User
		{
			Email = request.Email,
			PasswordHash = _passwordHasher.HashPassword(request.Password),
			Role = request.Role
		};

		await _userRepository.AddAsync(user, cancellationToken);
		return MapToDto(user);
	}

	public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(id, cancellationToken);
		if (user == null)
		{
			throw new InvalidOperationException($"User with ID {id} not found");
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

		if (!string.IsNullOrEmpty(request.Role))
		{
			user.Role = request.Role;
		}

		await _userRepository.UpdateAsync(user, cancellationToken);
		return MapToDto(user);
	}

	public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(id, cancellationToken);
		if (user == null)
		{
			throw new InvalidOperationException($"User with ID {id} not found");
		}

		await _userRepository.DeleteAsync(id, cancellationToken);
	}

	private static UserDto MapToDto(Domain.Entities.User user)
	{
		return new UserDto(
			user.Id,
			user.Email,
			user.Role,
			user.CreatedAtUtc,
			user.LastLoginAtUtc
		);
	}
}
