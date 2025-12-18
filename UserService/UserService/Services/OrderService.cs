using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Events;
using UserService.Kafka;
using UserService.Models;

namespace UserService.Services
{
	public class OrderService : IOrderService
	{
		private readonly UserDbContext _context;
		private readonly ILogger<OrderService> _logger;
		private readonly ICartService _cartService;
		private readonly OrderProducer _producer;

		public OrderService(
			UserDbContext context,
			ILogger<OrderService> logger,
			ICartService cartService,OrderProducer producer)
		{
			_context = context;
			_logger = logger;
			_cartService = cartService;
			_producer = producer;
		}

		public async Task<OrderResponse> CreateOrderAsync(string userId, CreateOrderRequest request)
		{
			// Получаем корзину
			var cart = await _cartService.GetCartAsync(userId);

			if (cart.Items.Count == 0)
			{
				throw new InvalidOperationException("Корзина пуста");
			}

			// Создаем заказ в транзакции
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var order = new Order
				{
					UserId = userId,
					TotalPrice = cart.TotalPrice,
					Status = OrderStatus.Pending,
					ShippingAddress = request.ShippingAddress,
					PaymentMethod = request.PaymentMethod,
					Items = cart.Items.Select(item => new OrderItem
					{
						ProductId = item.ProductId,
						ProductName = item.Name ?? "Unknown",
						Quantity = item.Quantity,
						Price = item.Price,
						Size = item.Size,
						ImageUrl = item.ImageUrl
					}).ToList()
				};

				_context.Orders.Add(order);
				await _context.SaveChangesAsync();

				var kafkaEvent = new OrderCreatedEvent
				{
					OrderId = order.OrderId,
					UserId = userId,
					TotalPrice = order.TotalPrice,
					Items = order.Items.Select(i => new OrderCreatedItem
					{
						ProductId = i.ProductId,
						Price = i.Price,
						Quantity = i.Quantity,
						Size = i.Size
					}).ToList()
				};

				await _producer.PublishOrderCreatedAsync(kafkaEvent);
				// Очищаем корзину
				await _cartService.ClearCartAsync(userId);

				await transaction.CommitAsync();

				_logger.LogInformation("Заказ {OrderId} создан для пользователя {UserId}", order.OrderId, userId);

				return MapToResponse(order);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Ошибка создания заказа для пользователя {UserId}", userId);
				throw;
			}
		}

		public async Task<OrderResponse?> GetOrderAsync(string userId, string orderId)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

			return order != null ? MapToResponse(order) : null;
		}

		public async Task<OrderListResponse> GetUserOrdersAsync(string userId)
		{
			var orders = await _context.Orders
				.Include(o => o.Items)
				.Where(o => o.UserId == userId)
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync();

			return new OrderListResponse
			{
				TotalOrders = orders.Count,
				Orders = orders.Select(MapToResponse).ToList()
			};
		}

		public async Task<bool> CancelOrderAsync(string userId, string orderId)
		{
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

			if (order == null)
				return false;

			if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
			{
				throw new InvalidOperationException("Невозможно отменить отправленный или доставленный заказ");
			}

			order.Status = OrderStatus.Cancelled;
			order.CompletedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			_logger.LogInformation("Заказ {OrderId} отменен пользователем {UserId}", orderId, userId);

			return true;
		}

		private static OrderResponse MapToResponse(Order order)
		{
			return new OrderResponse
			{
				Id = order.Id,
				OrderId = order.OrderId,
				UserId = order.UserId,
				TotalPrice = order.TotalPrice,
				Status = order.Status.ToString(),
				CreatedAt = order.CreatedAt,
				CompletedAt = order.CompletedAt,
				ShippingAddress = order.ShippingAddress,
				PaymentMethod = order.PaymentMethod,
				Items = order.Items.Select(item => new OrderItemResponse
				{
					Id = item.Id,
					ProductId = item.ProductId,
					ProductName = item.ProductName,
					Quantity = item.Quantity,
					Price = item.Price,
					TotalPrice = item.Price * item.Quantity,
					Size = item.Size,
					Color = item.Color,
					ImageUrl = item.ImageUrl
				}).ToList()
			};
		}
	}
}
