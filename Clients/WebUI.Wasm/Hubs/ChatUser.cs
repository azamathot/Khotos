namespace WebUI_Wasm.Hubs
{
    public class ChatUser
    {
        private readonly List<ChatConnection> _connections;

        public ChatUser(string userName, string userId)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            UserId = userId;
            _connections = [];
        }

        /// <summary>
        /// User identity name
        /// </summary>
        public string UserName { get; }
        public string UserId { get; }
        /// <summary>
        /// UTC time connected
        /// </summary>
        public DateTime? ConnectedAt
        {
            get
            {
                if (Connections.Any())
                {
                    return Connections
                        .OrderByDescending(x => x.ConnectedAt)
                        .Select(x => x.ConnectedAt)
                        .First();
                }

                return null;
            }
        }

        /// <summary>
        /// All user connections
        /// </summary>
        public IEnumerable<ChatConnection> Connections => _connections;

        /// <summary>
        /// Append connection for user
        /// </summary>
        /// <param name="connectionId"></param>
        public void AppendConnection(string connectionId)
        {
            ArgumentNullException.ThrowIfNull(connectionId);
            //var fakes = _connections.Where(x => (DateTime.UtcNow - x.ConnectedAt).TotalMinutes < 2);
            //foreach (var item in fakes)
            //{
            //    _connections.Remove(item);
            //}
            var connection = new ChatConnection
            {
                ConnectedAt = DateTime.UtcNow,
                ConnectionId = connectionId
            };

            _connections.Add(connection);
        }

        /// <summary>
        /// Remove connection from user
        /// </summary>
        public void RemoveConnection(string connectionId)
        {
            ArgumentNullException.ThrowIfNull(connectionId);

            var connection = _connections.SingleOrDefault(x => x.ConnectionId.Equals(connectionId));
            if (connection == null)
            {
                return;
            }
            _connections.Remove(connection);
        }
    }
}
