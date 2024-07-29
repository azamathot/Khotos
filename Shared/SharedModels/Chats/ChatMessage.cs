using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Chats
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string? Message { get; set; }
        public string? MediaData { get; set; }
        public required string UserId { get; set; }
        public bool IsReaded { get; set; }
        public DateTimeOffset SendTime { get; set; }

        [NotMapped]
        public bool Mine { get; set; }
        [NotMapped]
        public string CSS => Mine ? "sent" : "received";
        //[NotMapped]
        public string? UserName { get; set; }

    }
}
