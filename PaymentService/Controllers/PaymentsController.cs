using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;

namespace PaymentService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentsController : ControllerBase
	{
		private readonly Services.PaymentService _paymentService;
		private readonly ILogger<PaymentsController> _logger;

		public PaymentsController(
			Services.PaymentService paymentService,
			ILogger<PaymentsController> logger)
		{
			_paymentService = paymentService;
			_logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
		{
			if (string.IsNullOrEmpty(request.OrderId))
			{
				return BadRequest("OrderId is required");
			}

			_logger.LogInformation($"Получен запрос на создание платежа для заказа {request.OrderId}");

			var result = await _paymentService.CreatePaymentIntentAsync(request);

			if (result.Success)
			{
				return Ok(result);
			}

			return BadRequest(result);
		}

		[HttpGet("{orderId}")]
		public async Task<IActionResult> GetPayment(string orderId)
		{
			var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

			if (payment == null)
			{
				return NotFound(new { message = $"Платеж для заказа {orderId} не найден" });
			}

			return Ok(payment);
		}

		[HttpPost("success")]
		public async Task<IActionResult> HandleSuccess([FromQuery] string orderId)
		{
			if (string.IsNullOrEmpty(orderId))
			{
				return BadRequest("OrderId is required");
			}

			await _paymentService.HandleSuccessAsync(orderId);
			return Ok(new { message = "Payment processed successfully" });
		}

		[HttpPost("cancel")]
		public async Task<IActionResult> HandleCancel([FromQuery] string orderId)
		{
			if (string.IsNullOrEmpty(orderId))
			{
				return BadRequest("OrderId is required");
			}

			await _paymentService.HandleFailureAsync(orderId);
			return Ok(new { message = "Payment cancelled" });
		}
	}
}