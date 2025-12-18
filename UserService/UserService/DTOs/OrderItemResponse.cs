namespace UserService.DTOs
{
	public class OrderItemResponse
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal TotalPrice { get; set; }
		public string? Size { get; set; }
		public string? Color { get; set; }
		public string? ImageUrl { get; set; }
	}
}
