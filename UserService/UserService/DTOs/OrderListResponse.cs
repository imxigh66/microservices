namespace UserService.DTOs
{
	public class OrderListResponse
	{
		public int TotalOrders { get; set; }
		public List<OrderResponse> Orders { get; set; } = new();
	}
}
