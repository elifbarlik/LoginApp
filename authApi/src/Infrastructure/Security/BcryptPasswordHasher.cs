using Application.Abstractions;

namespace Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
	public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
	public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}


