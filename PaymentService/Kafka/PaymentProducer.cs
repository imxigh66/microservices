using Confluent.Kafka;
using PaymentService.Models;
using System.Text.Json;

namespace PaymentService.Kafka
{
	public class PaymentProducer : IDisposable
	{
		private readonly IProducer<string, string> _producer;
		private readonly ILogger<PaymentProducer> _logger;
		private readonly string _successTopic;
		private readonly string _failedTopic;

		public PaymentProducer(IConfiguration configuration, ILogger<PaymentProducer> logger)
		{
			_logger = logger;

			var config = new ProducerConfig
			{
				BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "dev-kafka:9092",
				Acks = Acks.All,
				EnableIdempotence = true,
				MaxInFlight = 1,
				LingerMs = 0
			};

			_producer = new ProducerBuilder<string, string>(config)
				.SetErrorHandler((_, e) => _logger.LogError($"Kafka Producer Error: {e.Reason}"))
				.SetLogHandler((_, log) => _logger.LogInformation($"Kafka Producer Log: {log.Message}"))
				.Build();

			_successTopic = configuration["Kafka:Topics:PaymentSucceeded"] ?? "payment-succeeded";
			_failedTopic = configuration["Kafka:Topics:PaymentFailed"] ?? "payment-failed";

			_logger.LogInformation($"PaymentProducer initialized");
			_logger.LogInformation($"Success Topic: {_successTopic}");
			_logger.LogInformation($"Failed Topic: {_failedTopic}");
		}

		public async Task PublishPaymentSucceededAsync(PaymentSucceededEvent evt)
		{
			try
			{
				var json = JsonSerializer.Serialize(evt);

				_logger.LogInformation($"Отправка payment-succeeded. OrderId: {evt.OrderId}");

				var result = await _producer.ProduceAsync(_successTopic, new Message<string, string>
				{
					Key = evt.OrderId,
					Value = json
				});

				// КРИТИЧНО: Flush!
				_producer.Flush(TimeSpan.FromSeconds(10));

				_logger.LogInformation(
					$"✓ payment-succeeded отправлено. " +
					$"Partition: {result.Partition}, Offset: {result.Offset}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки payment-succeeded");
				throw;
			}
		}

		public async Task PublishPaymentFailedAsync(PaymentFailedEvent evt)
		{
			try
			{
				var json = JsonSerializer.Serialize(evt);

				_logger.LogInformation($"Отправка payment-failed. OrderId: {evt.OrderId}");

				var result = await _producer.ProduceAsync(_failedTopic, new Message<string, string>
				{
					Key = evt.OrderId,
					Value = json
				});

				_producer.Flush(TimeSpan.FromSeconds(10));

				_logger.LogInformation($"✓ payment-failed отправлено");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка отправки payment-failed");
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				_producer?.Flush(TimeSpan.FromSeconds(10));
				_producer?.Dispose();
				_logger.LogInformation("PaymentProducer disposed");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при закрытии PaymentProducer");
			}
		}
	}
}
