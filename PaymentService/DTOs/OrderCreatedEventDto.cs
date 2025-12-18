namespace PaymentService.DTOs
{
	public class OrderCreatedEventDto
	{
		public string OrderId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public decimal TotalPrice { get; set; }
		public List<OrderItemDto> Items { get; set; } = new();
	}
}
