namespace ChatAPI.Hubs
{
    public class ChatUser
    {
        public ChatUser(string userName, string userID, string connectionID, string groupName)
        {
            UserName = userName;
            UserID = userID;
            ConnectionID = connectionID;
            GroupName = groupName;
        }

        public string UserName { get; set; }
        public string UserID { get; set; }
        public string ConnectionID { get; set; }
        public string GroupName { get; set; }
    }
}
