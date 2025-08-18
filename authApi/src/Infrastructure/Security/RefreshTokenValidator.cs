using Application.Abstractions;

namespace Infrastructure.Security;

public class RefreshTokenValidator : IRefreshTokenValidator
{
	public bool ValidateRefreshToken(string refreshToken)
	{
		if (string.IsNullOrEmpty(refreshToken))
			return false;

		try
		{
			// Basic validation - check if it's a valid base64 string
			var tokenBytes = Convert.FromBase64String(refreshToken);
			return tokenBytes.Length == 64; // Should be 64 bytes
		}
		catch
		{
			return false;
		}
	}
}
