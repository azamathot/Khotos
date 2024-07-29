using Microsoft.EntityFrameworkCore;
using SharedModels.Chats;
using SharedModels.Orders;

namespace Ordering.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        //public DbSet<StatusOrder> StatusOrders { get; set; }
        //public DbSet<StatusPayment> StatusPayments { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<StatusOrder>().HasData(
        //            new StatusOrder { Id = 1, Name = "Создан", Color = Color.Lime },
        //            new StatusOrder { Id = 2, Name = "На рассмотрении", Color = Color.Gold },
        //            new StatusOrder { Id = 3, Name = "Отказ в выполнении", Color = Color.Red },
        //            new StatusOrder { Id = 4, Name = "В разработке", Color = Color.Orange },
        //            new StatusOrder { Id = 5, Name = "Приостановлен", Color = Color.LightGray },
        //            new StatusOrder { Id = 6, Name = "На внедрении", Color = Color.LightSkyBlue },
        //            new StatusOrder { Id = 7, Name = "Выполнен", Color = Color.CornflowerBlue }
        //    );

        //    modelBuilder.Entity<StatusPayment>().HasData(
        //            new StatusOrder { Id = 1, Name = "Неоплачен", Color = Color.Red },
        //            new StatusOrder { Id = 2, Name = "Частично оплачен", Color = Color.Orange },
        //            new StatusOrder { Id = 3, Name = "Полностью оплачен", Color = Color.LimeGreen }
        //    );
        //}
    }
}
