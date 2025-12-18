namespace UserService.Services
{
    public interface ICatalogClient
    {
        Task<(bool exists, string? name, decimal? price, string? thumbUrl)>
        GetProductPreviewAsync(int productId, CancellationToken ct = default);
    }
}
