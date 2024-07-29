using ChatAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedModels.Chats;
using System.Security.Claims;
//using System.Xml;

namespace ChatAPI.Services
{
    [Authorize]
    public class ChatService
    {
        private readonly ChatDbContext dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _userId { get; set; }

        public ChatService(ChatDbContext chatDbContext, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            dbContext = chatDbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        // ... Реализуйте методы для работы с базой данных
        public async Task<List<ChatMessage>> GetMessagesByOrderAsync(int orderId)
        {
            _userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await dbContext.ChatMessages.Where(x => x.OrderId == orderId && x.UserId != _userId && !x.IsReaded)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsReaded, true));
            var chatList = dbContext.ChatMessages.Where(x => x.OrderId == orderId)
                .ToList();
            chatList.All(m =>
            {
                m.Mine = m.UserId == _userId;
                return true;
            });
            return chatList;
        }


        public async Task AddMessageAsync(ChatMessage chatMessage)
        {
            dbContext.ChatMessages.Add(chatMessage);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateChatMessageAsync(ChatMessage chatMessage)
        {
            try
            {
                dbContext.Entry(chatMessage).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        public async Task DeleteMessageAsync(int id)
        {
            var chatMessage = await dbContext.ChatMessages.FindAsync(id);
            if (chatMessage != null)
            {
                dbContext.ChatMessages.Remove(chatMessage);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task IsReadedAsync(int id)
        {
            var chatMessage = await dbContext.ChatMessages.FindAsync(id);
            if (chatMessage != null)
            {
                chatMessage.IsReaded = true;
                dbContext.ChatMessages.Update(chatMessage);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetCountNewMessageByOrder(int orderId)
        {
            _userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await dbContext.ChatMessages
                .Where(x => x.OrderId == orderId && !x.UserId.Equals(_userId) && !x.IsReaded).CountAsync();
        }
    }
}
