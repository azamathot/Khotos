namespace SharedModels.Orders
{
    public class OrderView
    {
        public required Order Order { get; set; }
        public int? CountNewMessage { get; set; }
        public DateTimeOffset? LastChange { get; set; }
    }
}
