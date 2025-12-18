namespace CatalogService.DTO
{
    public class ProductWithImagesRequest:ProductRequest
    {
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; }
    }
}
