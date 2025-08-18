using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _dbContext;

	public UserRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
	}

	public async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
	}

	public async Task<IEnumerable<Domain.Entities.User>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Users
			.AsNoTracking()
			.OrderBy(u => u.CreatedAtUtc)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Domain.Entities.User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Users
			.AsNoTracking()
			.Where(u => u.Role == role)
			.OrderBy(u => u.CreatedAtUtc)
			.ToListAsync(cancellationToken);
	}

	public async Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
	{
		_dbContext.Users.Add(user);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
	{
		_dbContext.Users.Update(user);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var user = await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);
		if (user != null)
		{
			_dbContext.Users.Remove(user);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}


