using ChatAPI.Services;
using Microsoft.AspNetCore.SignalR;
using SharedModels.Chats;

namespace ChatAPI.Hubs
{
    //[Authorize]
    public class ChatHub(ChatService chatService) : Hub
    {
        private readonly ChatService _chatService = chatService;
        private static readonly List<ChatUser> usersLookup = new List<ChatUser>();

        #region overrides
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected ID: {Context.ConnectionId}, username: {Context.User?.Identity?.Name ?? "[Unknown]"}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var user = usersLookup.FirstOrDefault(x => x.ConnectionID == Context.ConnectionId);
                if (user != null)
                {
                    await Clients.Group(user.GroupName).SendAsync(HubNames.LEAVE_GROUP, user.UserName);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.GroupName);
                    usersLookup.Remove(user);
                    Console.WriteLine($"Disconnected username: {user.UserName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR on Disconnected: " + ex.ToString());
            }
            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        public async Task SendMessage(ChatMessage messageData)
        {
            try
            {
                await _chatService.AddMessageAsync(messageData);
                var connectionId = Context.ConnectionId;
                await Clients.GroupExcept(messageData.OrderId.ToString()!, connectionId)
                    .SendAsync(HubNames.RECEIVE, messageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR on Send: " + ex.ToString());
            }
        }

        public async Task JoinGroup(string groupName, string username, string userid, string Companion)
        {
            var userName = Context.User?.Identity?.Name ?? username;
            var userId = Context.UserIdentifier ?? userid;
            var connectionId = Context.ConnectionId;

            usersLookup.Add(new ChatUser(userName, userId, connectionId, groupName));
            await Groups.AddToGroupAsync(connectionId, groupName);
            await Clients.GroupExcept(groupName, connectionId).SendAsync(HubNames.JOIN_GROUP, userName);

            Console.WriteLine($"group: {groupName}");
            if (usersLookup.Any(x => x.UserName == Companion))
                await Clients.Group(groupName).SendAsync(HubNames.JOIN_GROUP, Companion); ;
        }

        public async Task UpdataeMessage(ChatMessage chatMessage)
        {
            await _chatService.UpdateChatMessageAsync(chatMessage);
            await Clients
                .GroupExcept(chatMessage.OrderId.ToString(), Context.ConnectionId)
                .SendAsync(HubNames.UPDATE_MESSAGE, chatMessage);
        }
        public async Task DeleteMessage(int messageId, string groupName)
        {
            await _chatService.DeleteMessageAsync(messageId);
            await Clients
                .GroupExcept(groupName, Context.ConnectionId)
                .SendAsync(HubNames.DELETE_MESSAGE, messageId);
        }

        public async Task IsReaded(int messageId, string groupName)
        {
            await _chatService.IsReadedAsync(messageId);
            await Clients
                .GroupExcept(groupName, Context.ConnectionId)
                .SendAsync(HubNames.IS_READED, messageId);
        }
    }
}