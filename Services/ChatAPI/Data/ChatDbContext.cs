using Microsoft.EntityFrameworkCore;
using SharedModels.Chats;

namespace ChatAPI.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
            //Database.Migrate();
        }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
