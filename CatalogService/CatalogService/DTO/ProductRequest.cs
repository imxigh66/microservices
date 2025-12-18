using CatalogService.Models;

namespace CatalogService.DTO
{
    public class ProductRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public string Sex { get; set; } = null!;
        public string Season { get; set; } = null!;

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<ProductSizeRequest> Sizes { get; set; } = new();
    }
}
