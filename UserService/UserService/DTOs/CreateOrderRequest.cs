namespace UserService.DTOs
{
	public class CreateOrderRequest
	{
		public string? ShippingAddress { get; set; }
		public string? PaymentMethod { get; set; }
	}
}
