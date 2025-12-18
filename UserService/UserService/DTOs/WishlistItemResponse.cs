namespace UserService.DTOs
{
    public class WishlistItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; }


        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }

        public string Sex { get; set; } = null!;
        public string Season { get; set; } = null!;

        //public int CategoryId { get; set; }
        //public string? CategoryName { get; set; }  // удобно для карточки

        //public List<ProductSizeDto> Sizes { get; set; } = new();
        //public string? MainImageUrl { get; set; }
        public string? AdditionalImageUrls { get; set; }
    }
}
