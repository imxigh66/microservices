using UserService.DTOs;

namespace UserService.Services
{
	public interface IOrderService
	{
		Task<OrderResponse> CreateOrderAsync(string userId, CreateOrderRequest request);
		Task<OrderResponse?> GetOrderAsync(string userId, string orderId);
		Task<OrderListResponse> GetUserOrdersAsync(string userId);
		Task<bool> CancelOrderAsync(string userId, string orderId);
	}
}
