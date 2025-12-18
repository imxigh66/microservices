namespace CatalogService.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Sex { get; set; }
        public string Season { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        public string? MainImageUrl { get; set; }
        public List<ProductImage> AdditionalImages { get; set; } = new();
    }
}
