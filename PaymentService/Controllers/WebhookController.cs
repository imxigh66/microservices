using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace PaymentService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WebhookController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly ILogger<WebhookController> _logger;
		private readonly Services.PaymentService _paymentService;

		public WebhookController(
			IConfiguration config,
			ILogger<WebhookController> logger,
			Services.PaymentService paymentService)
		{
			_config = config;
			_logger = logger;
			_paymentService = paymentService;
		}

		[HttpPost("stripe")]
		public async Task<IActionResult> HandleStripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
			var webhookSecret = _config["Stripe:WebhookSecret"];

			try
			{
				var stripeEvent = EventUtility.ConstructEvent(
					json,
					Request.Headers["Stripe-Signature"],
					webhookSecret
				);

				_logger.LogInformation($"Получен Stripe webhook: {stripeEvent.Type}");

				// Обработка события
				if (stripeEvent.Type == "checkout.session.completed")
				{
					var session = stripeEvent.Data.Object as Session;
					if (session != null)
					{
						var orderId = session.SuccessUrl?.Split("orderId=").LastOrDefault();
						if (!string.IsNullOrEmpty(orderId))
						{
							_logger.LogInformation($"Платеж успешен для заказа: {orderId}");
							await _paymentService.HandleSuccessAsync(orderId);
						}
					}
				}
				else if (stripeEvent.Type == "checkout.session.expired")
				{
					var session = stripeEvent.Data.Object as Session;
					if (session != null)
					{
						var orderId = session.CancelUrl?.Split("orderId=").LastOrDefault();
						if (!string.IsNullOrEmpty(orderId))
						{
							_logger.LogInformation($"Платеж отменен для заказа: {orderId}");
							await _paymentService.HandleFailureAsync(orderId);
						}
					}
				}

				return Ok();
			}
			catch (StripeException ex)
			{
				_logger.LogError(ex, "Ошибка обработки Stripe webhook");
				return BadRequest();
			}
		}
	}
}