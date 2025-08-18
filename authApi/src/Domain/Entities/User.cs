namespace Domain.Entities;

public class User
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Email { get; set; } = string.Empty;
	public string? Username { get; set; }
	public string PasswordHash { get; set; } = string.Empty;
	public string Role { get; set; } = "User"; // Default role
	public string? Phone { get; set; }
	public string? Address { get; set; }
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	public DateTime? LastLoginAtUtc { get; set; }
}

public class RefreshToken
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid UserId { get; set; }
	public string Token { get; set; } = string.Empty;
	public DateTime ExpiresAtUtc { get; set; }
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	public bool IsRevoked { get; set; } = false;
	
	public User User { get; set; } = null!;
}
