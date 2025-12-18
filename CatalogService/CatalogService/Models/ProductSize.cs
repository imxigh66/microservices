

using CatalogService.Enums;

namespace CatalogService.Models
{
    public class ProductSize
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public Size Size { get; set; }
        public int Quantity { get; set; }
    }
}
