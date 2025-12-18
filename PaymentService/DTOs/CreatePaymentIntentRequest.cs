namespace PaymentService.DTOs
{
	public class CreatePaymentIntentRequest
	{

		public string OrderId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public string Currency { get; set; } = "usd";
	}
}
