using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

       
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _cartService.AddToCartAsync(userId, request);

            if (item == null)
                return BadRequest(new { message = "Не удалось добавить товар в корзину" });

            return CreatedAtAction(nameof(GetCart), item);
        }

      
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _cartService.UpdateCartItemAsync(userId, itemId, request);

            if (item == null)
                return NotFound(new { message = "Товар не найден в корзине" });

            return Ok(item);
        }


        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _cartService.RemoveFromCartAsync(userId, itemId);

            if (!result)
                return NotFound(new { message = "Товар не найден в корзине" });

            return Ok(new { message = "Товар удален из корзины" });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var count = await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "Корзина очищена", deletedCount = count });
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("uid");
        }
    }
}
