using Stripe;
using Stripe.Checkout;

namespace PaymentService.Services
{
	public class StripeService
	{
		private readonly IConfiguration _config;
		private readonly ILogger<StripeService> _logger;

		public StripeService(IConfiguration config, ILogger<StripeService> logger)
		{
			_config = config;
			_logger = logger;
			StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
		}

		public async Task<string> CreateCheckoutSessionAsync(Guid orderId, decimal amount)
		{
			var successUrl = _config["Stripe:SuccessUrl"];
			var cancelUrl = _config["Stripe:CancelUrl"];

			var options = new SessionCreateOptions
			{
				Mode = "payment",
				SuccessUrl = $"{successUrl}?orderId={orderId}",
				CancelUrl = $"{cancelUrl}?orderId={orderId}",
				LineItems = new List<SessionLineItemOptions>
				{
					new SessionLineItemOptions
					{
						Quantity = 1,
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = _config["Stripe:Currency"] ?? "usd",
							UnitAmount = (long)(amount * 100), // cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = $"Order #{orderId}"
							}
						}
					}
				}
			};

			var service = new SessionService();
			var session = await service.CreateAsync(options);

			_logger.LogInformation($"Stripe Checkout Session создан: {session.Id}");

			return session.Url;
		}

		// Альтернативный метод - PaymentIntent вместо Checkout
		public async Task<PaymentIntent> CreatePaymentIntentAsync(
			decimal amount,
			string currency,
			Dictionary<string, string> metadata)
		{
			var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(amount * 100),
				Currency = currency,
				Metadata = metadata,
				AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
				{
					Enabled = true,
				}
			};

			var service = new PaymentIntentService();
			var paymentIntent = await service.CreateAsync(options);

			_logger.LogInformation($"Stripe PaymentIntent создан: {paymentIntent.Id}");

			return paymentIntent;
		}
	}
}