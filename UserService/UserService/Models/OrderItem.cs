namespace UserService.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }

        public Order Order { get; set; } = null!;
    }
}
