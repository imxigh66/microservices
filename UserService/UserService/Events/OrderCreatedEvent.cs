namespace UserService.Events
{
	public class OrderCreatedEvent
	{
		public string OrderId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public decimal TotalPrice { get; set; }
		public List<OrderCreatedItem> Items { get; set; } = new();
	}
}
