namespace UserService.DTOs
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Новые поля из каталога
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}
