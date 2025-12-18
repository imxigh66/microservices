using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ProductDbContext _context;
        public CategoryService(ProductDbContext context)
        {
            _context = context;
        }
        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
           var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.Include(c => c.Products).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateAsync(int id, Category updated)
        {
            var category=await _context.Categories.FindAsync(id);
            if (category == null)
                return false;
            category.Name = updated.Name;
            category.Description = updated.Description;
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
