using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Kafka;
using PaymentService.Models;

namespace PaymentService.Services
{
	public class PaymentService
	{
		private readonly PaymentDbContext _context;
		private readonly StripeService _stripeService;
		private readonly PaymentProducer _producer;
		private readonly ILogger<PaymentService> _logger;

		public PaymentService(
			PaymentDbContext context,
			StripeService stripeService,
			PaymentProducer producer,
			ILogger<PaymentService> logger)
		{
			_context = context;
			_stripeService = stripeService;
			_producer = producer;
			_logger = logger;
		}

		public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
		{
			_logger.LogInformation($"Создание PaymentIntent для заказа {request.OrderId}");

			try
			{
				// Создаем запись о платеже
				var payment = new Payment
				{
					OrderId = request.OrderId,
					UserId = request.UserId,
					Amount = request.Amount,
					Status = PaymentStatus.Pending // ИСПРАВЛЕНО
				};

				_context.Payments.Add(payment);
				await _context.SaveChangesAsync();

				_logger.LogInformation($"Платеж создан в БД: {payment.Id}");

				// Вызываем Stripe
				var sessionId = await _stripeService.CreateCheckoutSessionAsync(
					Guid.Parse(request.OrderId),
					request.Amount);

				// Обновляем платеж
				payment.StripeSessionId = sessionId;
				payment.Status = PaymentStatus.Processing; // ИСПРАВЛЕНО
				await _context.SaveChangesAsync();

				_logger.LogInformation($"Stripe Session создан: {sessionId}");

				return new PaymentIntentResponse
				{
					Success = true,
					PaymentIntentId = sessionId,
					ClientSecret = sessionId, // Для Stripe Checkout это Session ID
					Status = payment.Status.ToString()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка создания платежа для заказа {request.OrderId}");

				return new PaymentIntentResponse
				{
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		public async Task HandleSuccessAsync(string orderId)
		{
			var payment = await _context.Payments
				.FirstOrDefaultAsync(p => p.OrderId == orderId);

			if (payment == null)
			{
				_logger.LogWarning($"Платеж не найден для заказа {orderId}");
				return;
			}

			_logger.LogInformation($"Обработка успешного платежа. OrderId: {payment.OrderId}");

			payment.Status = PaymentStatus.Success;
			payment.PaidAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			// Отправляем событие в Kafka
			var successEvent = new PaymentSucceededEvent
			{
				OrderId = payment.OrderId,
				PaymentIntentId = payment.StripeSessionId ?? "",
				Amount = payment.Amount,
				PaidAt = payment.PaidAt.Value
			};

			await _producer.PublishPaymentSucceededAsync(successEvent);
			_logger.LogInformation($"✓ Событие payment-succeeded отправлено для {payment.OrderId}");
		}

		public async Task HandleFailureAsync(string orderId)
		{
			var payment = await _context.Payments
				.FirstOrDefaultAsync(p => p.OrderId == orderId);

			if (payment == null)
			{
				_logger.LogWarning($"Платеж не найден для заказа {orderId}");
				return;
			}

			_logger.LogInformation($"Обработка неудачного платежа. OrderId: {payment.OrderId}");

			payment.Status = PaymentStatus.Failed;
			await _context.SaveChangesAsync();

			// Отправляем событие в Kafka
			var failedEvent = new PaymentFailedEvent
			{
				OrderId = payment.OrderId,
				Reason = "Payment cancelled or failed",
				FailedAt = DateTime.UtcNow
			};

			await _producer.PublishPaymentFailedAsync(failedEvent);
			_logger.LogInformation($"✓ Событие payment-failed отправлено для {payment.OrderId}");
		}

		public async Task<Payment?> GetPaymentByOrderIdAsync(string orderId)
		{
			return await _context.Payments
				.FirstOrDefaultAsync(p => p.OrderId == orderId);
		}
	}
}