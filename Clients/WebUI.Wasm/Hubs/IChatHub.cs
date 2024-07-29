using SharedModels.Chats;

namespace WebUI_Wasm.Hubs
{
    public interface IChatHub
    {
        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendMessageAsync(ChatMessage messageData);

        /// <summary>
        /// Update user list
        /// </summary>
        /// <param name="users"></param>
        Task UpdateUsersAsync(Dictionary<string, string> users);

    }
}
