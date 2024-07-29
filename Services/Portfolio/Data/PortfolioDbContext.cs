using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data
{
	public class PortfolioDbContext : DbContext
	{
		public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
			: base(options)
		{
			Database.Migrate();
		}

		public DbSet<Photo> Photos { get; set; }
		public DbSet<Work> Works { get; set; }

	}
}
