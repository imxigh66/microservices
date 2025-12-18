namespace UserService.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }  // ID товара из CatalogService
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Навигация
        public virtual UserProfile User { get; set; } = null!;
    }
}
