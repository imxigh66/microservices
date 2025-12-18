namespace PaymentService.Models
{
	public class OrderCreatedEvent
	{
		public Guid OrderId { get; set; }
		public string UserId { get; set; } = "";
		public decimal TotalPrice { get; set; }
	}
}
