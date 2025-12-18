using AutoMapper;
using CatalogService.DTO;
using CatalogService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController:ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IImageServiceClient _imageServiceClient;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(IProductService productService, IMapper mapper, IImageServiceClient imageServiceClient,ILogger<ProductsController> logger)
        {
            _productService = productService;
            _mapper = mapper;
            _imageServiceClient = imageServiceClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            var dto = _mapper.Map<ProductDto>(product);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
        {
            var product = _mapper.Map<Models.Product>(request);
            //if (request.MainImage != null)
            //{
            //    product.MainImageUrl = await _imageServiceClient.UploadImageAsync(request.MainImage);
            //}

            //// Загружаем дополнительные изображения
            //if (request.AdditionalImages?.Any() == true)
            //{
            //    var additionalUrls = await _imageServiceClient.UploadImagesAsync(request.AdditionalImages);
            //    product.AdditionalImages = additionalUrls.Select(url => new Models.ProductImage
            //    {
            //        ImageUrl = url
            //    }).ToList();
            //}
            var result = await _productService.CreateAsync(product);
            var dto = _mapper.Map<ProductDto>(result);
            return Ok(dto);
        }

        [HttpPost("{id}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddProductImages(int id, [FromForm] List<IFormFile> images)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var uploadedUrls = await _imageServiceClient.UploadImagesAsync(images);

            // Используем специальный метод для сохранения
            await _productService.AddImagesAsync(id, uploadedUrls);

            return Ok(new { urls = uploadedUrls });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequest request)
        {
            var product = _mapper.Map<Models.Product>(request);
            var updated = await _productService.UpdateAsync(id, product);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            
            if (!string.IsNullOrEmpty(product.MainImageUrl))
            {
                try
                {
                    await _imageServiceClient.DeleteImageAsync(product.MainImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete main image for product {ProductId}", id);
                }
            }

          
            if (product.AdditionalImages != null)
            {
                foreach (var image in product.AdditionalImages)
                {
                    if (!string.IsNullOrEmpty(image?.ImageUrl))
                    {
                        try
                        {
                            await _imageServiceClient.DeleteImageAsync(image.ImageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete additional image for product {ProductId}", id);
                        }
                    }
                }
            }

            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        //[HttpPost("{id}/images")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> AddProductImages(int id, [FromForm] List<IFormFile> images)
        //{
        //    var product = await _productService.GetByIdAsync(id);
        //    if (product == null)
        //        return NotFound();

        //    var uploadedUrls = await _imageServiceClient.UploadImagesAsync(images);

        //    product.AdditionalImages.AddRange(
        //        uploadedUrls.Select(url => new Models.ProductImage
        //        {
        //            ProductId = id,
        //            ImageUrl = url
        //        })
        //    );

        //    await _productService.UpdateAsync(id, product);

        //    return Ok(new { urls = uploadedUrls });
        //}

        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(int id, int imageId)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var image = product.AdditionalImages.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                return NotFound();

            await _imageServiceClient.DeleteImageAsync(image.ImageUrl);
            product.AdditionalImages.Remove(image);

            await _productService.UpdateAsync(id, product);

            return NoContent();
        }
    }

}
