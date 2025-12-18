using UserService.DTOs;

namespace UserService.Services
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemResponse>> GetWishlistAsync(string userId);
        Task<WishlistItemResponse?> AddToWishlistAsync(string userId, AddToWishlistRequest request);
        Task<bool> RemoveFromWishlistAsync(string userId, int itemId);
        Task<bool> IsInWishlistAsync(string userId, int productId);
        Task<int> ClearWishlistAsync(string userId);
    }

}
