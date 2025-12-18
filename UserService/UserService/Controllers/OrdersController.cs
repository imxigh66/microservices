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
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _orderService;
		private readonly ILogger<OrdersController> _logger;

		public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
		{
			_orderService = orderService;
			_logger = logger;
		}

		// POST api/orders - Создать заказ из корзины
		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var order = await _orderService.CreateOrderAsync(userId, request);
				return CreatedAtAction(nameof(GetOrder), new { orderId = order.OrderId }, order);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка создания заказа");
				return StatusCode(500, new { message = "Ошибка создания заказа" });
			}
		}

		// GET api/orders - Получить все заказы пользователя
		[HttpGet]
		public async Task<IActionResult> GetUserOrders()
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();

			var orders = await _orderService.GetUserOrdersAsync(userId);
			return Ok(orders);
		}

		// GET api/orders/{orderId} - Получить конкретный заказ
		[HttpGet("{orderId}")]
		public async Task<IActionResult> GetOrder(string orderId)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();

			var order = await _orderService.GetOrderAsync(userId, orderId);

			if (order == null)
				return NotFound(new { message = "Заказ не найден" });

			return Ok(order);
		}

		// DELETE api/orders/{orderId} - Отменить заказ
		[HttpDelete("{orderId}")]
		public async Task<IActionResult> CancelOrder(string orderId)
		{
			var userId = GetUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var result = await _orderService.CancelOrderAsync(userId, orderId);

				if (!result)
					return NotFound(new { message = "Заказ не найден" });

				return Ok(new { message = "Заказ отменен" });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		private string? GetUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier)
				   ?? User.FindFirstValue("uid");
		}
	}
}
