
namespace CatalogService.Services
{
    public class ImageServiceClient : IImageServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageServiceClient> _logger;

        public ImageServiceClient(HttpClient httpClient,ILogger<ImageServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            // Добавьте проверку на null и пустую строку
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                _logger.LogWarning("Attempted to delete empty image URL");
                return true; // Считаем успешным, если нечего удалять
            }

            try
            {
                var key = ExtractKeyFromUrl(imageUrl);
                var response = await _httpClient.DeleteAsync($"api/v1/images/{key}");
                return response.IsSuccessStatusCode;
            }
            catch (UriFormatException ex)
            {
                _logger.LogError(ex, "Invalid image URL format: {Url}", imageUrl);
                return false;
            }
        }

        private string ExtractKeyFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            // Проверяем, что это валидный URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException($"Invalid URL format: {url}", nameof(url));

            return uri.Segments.Last();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            _logger.LogInformation("Uploading file {FileName}, Size: {Size}", file.FileName, file.Length);

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("api/v1/images/upload", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response: {StatusCode}, Content: {Content}", response.StatusCode, responseContent);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload image: {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadImageAsync(file);
                    uploadedUrls.Add(url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload image {FileName}", file.FileName);
                }
            }

            return uploadedUrls;
        }
    }
}
