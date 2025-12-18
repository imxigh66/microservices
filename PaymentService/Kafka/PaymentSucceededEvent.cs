namespace PaymentService.Kafka
{
	public class PaymentSucceededEvent
	{
		public string OrderId { get; set; } = string.Empty;
		public string PaymentIntentId { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public DateTime PaidAt { get; set; }
	}
}
