namespace CatalogService.Services
{
    public interface IImageServiceClient
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
