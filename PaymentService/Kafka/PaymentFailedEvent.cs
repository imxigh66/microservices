namespace PaymentService.Kafka
{
	public class PaymentFailedEvent
	{
		public string OrderId { get; set; } = string.Empty;
		public string Reason { get; set; } = string.Empty;
		public DateTime FailedAt { get; set; }
	}
}
