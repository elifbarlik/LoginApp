using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly AppDbContext _dbContext;

	public RefreshTokenRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		return await _dbContext.RefreshTokens
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked, cancellationToken);
	}

	public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
	{
		_dbContext.RefreshTokens.Add(refreshToken);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
	{
		_dbContext.RefreshTokens.Update(refreshToken);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var tokens = await _dbContext.RefreshTokens
			.Where(x => x.UserId == userId && !x.IsRevoked)
			.ToListAsync(cancellationToken);

		foreach (var token in tokens)
		{
			token.IsRevoked = true;
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		var refreshToken = await _dbContext.RefreshTokens
			.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

		if (refreshToken != null)
		{
			refreshToken.IsRevoked = true;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
