using Microsoft.EntityFrameworkCore;
using SharedModels.Products;

namespace Products.Data
{
	public class ProductsDbContext : DbContext
	{
		public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
			: base(options)
		{
			Database.Migrate();
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>().HasData(
					new Category { Id = 1, Name = "Разработка программного обеспечения" },
					new Category { Id = 2, Name = "Образовательные услуги" }
			);

			modelBuilder.Entity<Product>().HasData(
					new Product { Id = 1, Name = "Веб-приложения (включая SPA, PWA)", CategoryId = 1 },
					new Product { Id = 2, Name = "Десктопные приложения (Windows)", CategoryId = 1 },
					new Product { Id = 3, Name = "Мобильные приложения на Xamarin (Android, Windows 10, ios)", CategoryId = 1 },
					new Product { Id = 4, Name = "Интеграция с соц. сетями (Telegram, WhatsApp, Instagram)", CategoryId = 1 },
					new Product { Id = 5, Name = "Автоматизация бизнес процессов", CategoryId = 1 },
					new Product { Id = 6, Name = "Репетиторство по программированию (Python, C#, ASP.Net Core, Xamarin, JS, HTML5 и CSS3)", CategoryId = 2 },
					new Product { Id = 7, Name = "Подготовка к ЕГЭ по информатике", CategoryId = 2 }
			);

		}
	}
}
