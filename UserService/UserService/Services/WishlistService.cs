using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<WishlistService> _logger;
        private readonly ICatalogClient _catalogClient;

        public WishlistService(UserDbContext context, ILogger<WishlistService> logger, ICatalogClient catalogClient)
        {
            _context = context;
            _logger = logger;
            _catalogClient = catalogClient;
        }

        public async Task<IEnumerable<WishlistItemResponse>> GetWishlistAsync(string userId)
        {
            var items = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            var list = new List<WishlistItemResponse>();
            foreach (var i in items)
            {
                var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(i.ProductId);
                list.Add(new WishlistItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    AddedAt = i.AddedAt,
                    Name = exists ? name : "[removed]",
                    Price = exists ? price : null,
                    AdditionalImageUrls = thumb
                });
            }
            return list;
        }

        public async Task<WishlistItemResponse?> AddToWishlistAsync(string userId, AddToWishlistRequest req)
        {
         
            var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(req.ProductId);
            if (!exists) return null;

         
            var existsRow = await _context.WishlistItems.AnyAsync(x => x.UserId == userId && x.ProductId == req.ProductId);
            if (existsRow) 
            {
                var existing = await _context.WishlistItems.AsNoTracking()
                                  .FirstAsync(x => x.UserId == userId && x.ProductId == req.ProductId);
                return new WishlistItemResponse { Id = existing.Id, ProductId = existing.ProductId, AddedAt = existing.AddedAt, Name = name, Price = price, AdditionalImageUrls = thumb };
            }

            var entity = new WishlistItem { UserId = userId, ProductId = req.ProductId };
            _context.WishlistItems.Add(entity);
            await _context.SaveChangesAsync();

            return new WishlistItemResponse { Id = entity.Id, ProductId = entity.ProductId, AddedAt = entity.AddedAt, Name = name, Price = price, AdditionalImageUrls = thumb };
        }


        public async Task<bool> RemoveFromWishlistAsync(string userId, int itemId)
        {
            var it = await _context.WishlistItems.FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);
            if (it is null) return false;
            _context.WishlistItems.Remove(it);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> IsInWishlistAsync(string userId, int productId) =>
        _context.WishlistItems.AnyAsync(x => x.UserId == userId && x.ProductId == productId);

        public async Task<int> ClearWishlistAsync(string userId)
        {
            var items = _context.WishlistItems.Where(x => x.UserId == userId);
            var count = await items.CountAsync();
            _context.WishlistItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return count;
        }

     
    }
}
