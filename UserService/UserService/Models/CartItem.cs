namespace UserService.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }  // ID товара из CatalogService
        public int Quantity { get; set; } = 1;
        public string? Size { get; set; } // Размер обуви
        public decimal Price { get; set; } // Цена на момент добавления
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигация
        public virtual UserProfile User { get; set; } = null!;
    }
}
