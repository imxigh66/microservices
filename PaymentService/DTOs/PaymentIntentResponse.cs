namespace PaymentService.DTOs
{
	public class PaymentIntentResponse
	{
		public bool Success { get; set; }
		public string? PaymentIntentId { get; set; }
		public string? ClientSecret { get; set; }
		public string? Status { get; set; }
		public string? ErrorMessage { get; set; }
	}
}
