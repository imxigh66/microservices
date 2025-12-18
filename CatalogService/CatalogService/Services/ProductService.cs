using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;



namespace CatalogService.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _dbContext;
        public ProductService(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Product> CreateAsync(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return  product;
        }

        public async Task<bool> AddImagesAsync(int productId, List<string> imageUrls)
        {
            var product = await _dbContext.Products
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return false;

            foreach (var url in imageUrls)
            {
                product.AdditionalImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url
                });
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return false;
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products
               .AsNoTracking()
               .Include(p => p.Category)
               .Include(p => p.ProductSizes)
                .Include(p => p.AdditionalImages)
               .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductSizes)
                 .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateAsync(int id, Product updated)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return false;
            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Sex = updated.Sex;
            product.Season = updated.Season;
            product.CategoryId = updated.CategoryId;
            await _dbContext.SaveChangesAsync();
            return true;

        }
    }
}
