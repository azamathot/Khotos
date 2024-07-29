namespace WebUI_Wasm.Hubs
{
    public class ChatManager
    {
        public List<ChatUser> Users { get; } = [];

        public void ConnectUser(string userName, string userId, string connectionId)
        {
            var userAlreadyExists = GetConnectedUserByName(userName);
            if (userAlreadyExists != null)
            {
                userAlreadyExists.AppendConnection(connectionId);
                return;
            }

            var user = new ChatUser(userName, userId);
            user.AppendConnection(connectionId);
            Users.Add(user);
        }

        /// <summary>
        /// Disconnect user from connection.
        /// If we found the connection is last, than we remove user from user list.
        /// </summary>
        /// <param name="connectionId"></param>
        public bool DisconnectUser(string connectionId)
        {
            var userExists = GetConnectedUserById(connectionId);
            if (userExists == null)
            {
                return false;
            }

            if (!userExists.Connections.Any(y => y.ConnectionId.Equals(connectionId)))
            {
                return false; // should never happen, but...
            }

            //if (userExists.Connections.Count() == 1)
            if (userExists.Connections.Count() >= 1)
            {
                Users.Remove(userExists);
                return true;
            }

            userExists.RemoveConnection(connectionId);
            return false;
        }

        /// <summary>
        /// Returns <see cref="ChatUser"/> by connectionId if connection found
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private ChatUser? GetConnectedUserById(string connectionId) =>
            Users
                .FirstOrDefault(x => x.Connections.Select(c => c.ConnectionId)
                .Contains(connectionId));

        /// <summary>
        /// Returns <see cref="ChatUser"/> by userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private ChatUser? GetConnectedUserByName(string userName) =>
            Users
                .FirstOrDefault(x => string.Equals(
                    x.UserName,
                    userName,
                    StringComparison.CurrentCultureIgnoreCase));
    }
}
