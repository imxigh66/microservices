using UserService.DTOs;

namespace UserService.Services
{
    public interface ICartService
    {
        Task<CartSummaryResponse> GetCartAsync(string userId);
        Task<CartItemResponse?> AddToCartAsync(string userId, AddToCartRequest request);
        Task<CartItemResponse?> UpdateCartItemAsync(string userId, int itemId, UpdateCartItemRequest request);
        Task<bool> RemoveFromCartAsync(string userId, int itemId);
        Task<int> ClearCartAsync(string userId);
    }
}
