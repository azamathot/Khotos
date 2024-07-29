namespace SharedModels.Payment
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public decimal Price { get; set; } = decimal.Zero;
        public int Quantity { get; set; } = 1;
    }
}
