using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services
{
    public class CartService : ICartService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<CartService> _logger;
        private readonly ICatalogClient _catalogClient;

        public CartService(UserDbContext context, ILogger<CartService> logger, ICatalogClient catalogClient)
        {
            _context = context;
            _logger = logger;
            _catalogClient = catalogClient;
        }

        public async Task<CartSummaryResponse> GetCartAsync(string userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();

            var cartItems = new List<CartItemResponse>();

            foreach (var item in items)
            {
                var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(item.ProductId);

                cartItems.Add(new CartItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Size = item.Size,
                    Price = exists ? price ?? item.Price : item.Price, // Используем актуальную цену или сохраненную
                    TotalPrice = (exists ? price ?? item.Price : item.Price) * item.Quantity,
                    AddedAt = item.AddedAt,
                    UpdatedAt = item.UpdatedAt,
                    Name = exists ? name : "[removed]",
                    ImageUrl = thumb
                });
            }

            return new CartSummaryResponse
            {
                TotalItems = items.Sum(i => i.Quantity),
                TotalPrice = cartItems.Sum(i => i.TotalPrice),
                Items = cartItems
            };
        }


        public async Task<CartItemResponse?> AddToCartAsync(string userId, AddToCartRequest request)
        {
            // Проверяем существование товара в каталоге
            var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(request.ProductId);

            if (!exists)
            {
                _logger.LogWarning("Попытка добавить несуществующий товар {ProductId} в корзину", request.ProductId);
                return null;
            }

            // Используем актуальную цену из каталога
            var actualPrice = price ?? request.Price;

            // Проверяем есть ли уже такой товар с теми же параметрами
            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.ProductId == request.ProductId &&
                    c.Size == request.Size);

            if (existing != null)
            {
                // Увеличиваем количество и обновляем цену
                existing.Quantity += request.Quantity;
                existing.Price = actualPrice;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Количество товара {ProductId} увеличено до {Quantity}", request.ProductId, existing.Quantity);

                return new CartItemResponse
                {
                    Id = existing.Id,
                    ProductId = existing.ProductId,
                    Quantity = existing.Quantity,
                    Size = existing.Size,
                    Price = existing.Price,
                    TotalPrice = existing.Price * existing.Quantity,
                    AddedAt = existing.AddedAt,
                    UpdatedAt = existing.UpdatedAt,
                    Name = name,
                    ImageUrl = thumb
                };
            }

            // Добавляем новый товар
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Size = request.Size,
                Price = actualPrice,
                AddedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Товар {ProductId} добавлен в корзину пользователя {UserId}", request.ProductId, userId);

            return new CartItemResponse
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Size = cartItem.Size,
                Price = cartItem.Price,
                TotalPrice = cartItem.Price * cartItem.Quantity,
                AddedAt = cartItem.AddedAt,
                UpdatedAt = cartItem.UpdatedAt,
                Name = name,
                ImageUrl = thumb
            };
        }

        public async Task<CartItemResponse?> UpdateCartItemAsync(string userId, int itemId, UpdateCartItemRequest request)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);

            if (item == null)
                return null;

            // Получаем актуальную информацию о товаре
            var (exists, name, price, thumb) = await _catalogClient.GetProductPreviewAsync(item.ProductId);

            item.Quantity = request.Quantity;
            item.Size = request.Size ?? item.Size;

            // Обновляем цену, если товар все еще существует
            if (exists && price.HasValue)
            {
                item.Price = price.Value;
            }

            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Товар в корзине обновлен: ItemId={ItemId}, Quantity={Quantity}", itemId, request.Quantity);

            return new CartItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Size = item.Size,
                Price = item.Price,
                TotalPrice = item.Price * item.Quantity,
                AddedAt = item.AddedAt,
                UpdatedAt = item.UpdatedAt,
                Name = exists ? name : "[removed]",
                ImageUrl = thumb
            };
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int itemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);

            if (item == null)
                return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Товар удален из корзины: ItemId={ItemId}, UserId={UserId}", itemId, userId);

            return true;
        }

        public async Task<int> ClearCartAsync(string userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Корзина очищена для пользователя {UserId}. Удалено {Count} товаров", userId, items.Count);

            return items.Count;
        }
    }
}