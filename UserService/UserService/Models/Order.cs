namespace UserService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
