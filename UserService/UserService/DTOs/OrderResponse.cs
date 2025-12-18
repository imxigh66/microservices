namespace UserService.DTOs
{
	public class OrderResponse
	{
		public int Id { get; set; }
		public string OrderId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public decimal TotalPrice { get; set; }
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public string? ShippingAddress { get; set; }
		public string? PaymentMethod { get; set; }
		public List<OrderItemResponse> Items { get; set; } = new();
	}
}
