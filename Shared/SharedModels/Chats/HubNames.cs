namespace SharedModels.Chats
{
    public static class HubNames
    {
        public const string HubUrl = "/chathub";
        public const long MaxFileSize = 1024 * 1024 * 2;

        public const string RECEIVE = "ReceiveMessage";
        public const string JOIN_GROUP = "JoinGroup";
        public const string LEAVE_GROUP = "LeaveGroup";
        public const string SEND = "SendMessage";
        public const string UPDATE_MESSAGE = "UpdataeMessage";
        public const string DELETE_MESSAGE = "DeleteMessage";
        public const string IS_READED = "IsReaded";

    }
}
