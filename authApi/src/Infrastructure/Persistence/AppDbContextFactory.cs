using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var builder = new DbContextOptionsBuilder<AppDbContext>();
		builder.UseSqlite("Data Source=auth.db");
		return new AppDbContext(builder.Options);
	}
}


