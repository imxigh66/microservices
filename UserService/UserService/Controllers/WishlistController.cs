using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController:ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();
            var wishlist = await _wishlistService.GetWishlistAsync(userId);
            return Ok(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _wishlistService.AddToWishlistAsync(userId, request);

            if (item == null)
                return BadRequest(new { message = "Не удалось добавить товар" });

            return CreatedAtAction(nameof(GetWishlist), item);
        }


        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveFromWishlist(int itemId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _wishlistService.RemoveFromWishlistAsync(userId, itemId);

            if (!result)
                return NotFound(new { message = "Товар не найден в wishlist" });

            return Ok(new { message = "Товар удален из wishlist" });
        }

        [HttpGet("check/{productId}")]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(new { productId, isInWishlist });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var count = await _wishlistService.ClearWishlistAsync(userId);
            return Ok(new { message = "Wishlist очищен", deletedCount = count });
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("uid");
        }
    }
}
