
using System.Net;
using System.Text.Json;

namespace UserService.Services
{
    public class CatalogClient : ICatalogClient
    {
        private readonly HttpClient _http;
        public CatalogClient(HttpClient http) => _http = http;

        public async Task<(bool exists, string? name, decimal? price, string? thumbUrl)>
    GetProductPreviewAsync(int id, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"/api/Products/{id}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return (false, null, null, null);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<ProductPreview>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            string? thumb = json!.MainImageUrl;

            if (thumb is null && json.AdditionalImageUrls is not null)
            {
                foreach (var raw in json.AdditionalImageUrls)
                {
                    // 1) Если пришёл уже готовый URL
                    if (Uri.IsWellFormedUriString(raw, UriKind.Absolute))
                    { thumb = raw; break; }

                    // 2) Если пришла строка-JSON {"url":"https://..."}
                    try
                    {
                        if (raw.Trim().StartsWith("{"))
                        {
                            using var doc = JsonDocument.Parse(raw);
                            if (doc.RootElement.TryGetProperty("url", out var u))
                                thumb = u.GetString();
                        }
                    }
                    catch { /* игнорируем */ }
                }
            }

            return (true, json.Name, json.Price, thumb);
        }

        private sealed class ProductPreview
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public string? MainImageUrl { get; set; }
            public List<string>? AdditionalImageUrls { get; set; }
        }
    }
}
