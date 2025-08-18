using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	
	public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Domain.Entities.User>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.HasIndex(x => x.Email).IsUnique();
			entity.Property(x => x.Email).IsRequired();
			entity.Property(x => x.PasswordHash).IsRequired();
			entity.Property(x => x.Role).IsRequired().HasDefaultValue("User");
			entity.Property(x => x.Username).HasMaxLength(100);
			entity.Property(x => x.Phone).HasMaxLength(50);
			entity.Property(x => x.Address).HasMaxLength(500);
			entity.Property(x => x.CreatedAtUtc).IsRequired();
		});

		modelBuilder.Entity<RefreshToken>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Token).IsRequired();
			entity.Property(x => x.UserId).IsRequired();
			entity.Property(x => x.ExpiresAtUtc).IsRequired();
			entity.Property(x => x.CreatedAtUtc).IsRequired();
			entity.Property(x => x.IsRevoked).IsRequired();
			
			entity.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}


