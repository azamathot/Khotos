using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedModels.Chats;

namespace WebUI_Wasm.Hubs
{
    [Authorize]
    public class ChatHub(ChatManager chatManager) : Hub<IChatHub>
    {
        private readonly ChatManager _chatManager = chatManager;
        private const string _defaultGroupName = "General";
        public const string HubUrl = "/chathub";

        #region overrides
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            var userId = Context.UserIdentifier ?? "Anonymous";

            var connectionId = Context.ConnectionId;
            _chatManager.ConnectUser(userName, userId, connectionId);
            var groupname = Context.GetHttpContext()?.Request.Headers["groupname"].ToString() ?? _defaultGroupName;
            await Groups.AddToGroupAsync(connectionId, groupname);
            await UpdateUsersAsync();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var isUserRemoved = _chatManager.DisconnectUser(Context.ConnectionId);
            if (!isUserRemoved)
            {
                await base.OnDisconnectedAsync(exception);
            }

            var groupname = Context.GetHttpContext()?.Request.Headers["groupname"].ToString() ?? _defaultGroupName;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupname);
            await UpdateUsersAsync();
            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        public async Task UpdateUsersAsync()
        {
            var users = _chatManager.Users.Select(x => new KeyValuePair<string, string>(x.UserId, x.UserName)).ToDictionary();
            await Clients.All.UpdateUsersAsync(users);
        }

        //public async Task AddToGroup(string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //}

        //public async Task RemoveFromGroup(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        //}

        public async Task SendMessageAsync(ChatMessage messageData)
        {
            if (messageData.OrderId == null || messageData.OrderId <= 0)
                await Clients.All.SendMessageAsync(messageData);
            else
                await Clients.Group(messageData.OrderId.ToString()).SendMessageAsync(messageData);
        }

    }
}