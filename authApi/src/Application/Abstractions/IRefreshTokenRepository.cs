using Domain.Entities;

namespace Application.Abstractions;

public interface IRefreshTokenRepository
{
	Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
	Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
	Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
	Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
	Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}
