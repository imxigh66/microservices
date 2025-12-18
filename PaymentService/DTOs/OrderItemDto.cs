namespace PaymentService.DTOs
{
	public class OrderItemDto
	{
		public string ProductId { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string? Size { get; set; }
	}
}
