using Confluent.Kafka;
using System.Text.Json;
using UserService.Events;

namespace UserService.Kafka
{
	public class OrderProducer : IDisposable
	{
		private readonly IProducer<Null, string> _producer;
		private readonly string _topic;
		private readonly ILogger<OrderProducer> _logger;

		public OrderProducer(IConfiguration config, ILogger<OrderProducer> logger)
		{
			_logger = logger;

			var kafkaConfig = new ProducerConfig
			{
				BootstrapServers = config["Kafka:BootstrapServers"],
				// ВАЖНО: Настройки для надежной отправки
				Acks = Acks.All, // Ждем подтверждения от всех реплик
				EnableIdempotence = true, // Защита от дубликатов
				MaxInFlight = 1, // Гарантия порядка сообщений
				MessageTimeoutMs = 30000,
				RequestTimeoutMs = 30000
			};

			_producer = new ProducerBuilder<Null, string>(kafkaConfig)
				.SetErrorHandler((_, e) => _logger.LogError($"Kafka Error: {e.Reason}"))
				.SetLogHandler((_, log) => _logger.LogInformation($"Kafka Log: {log.Message}"))
				.Build();

			_topic = config["Kafka:Topics:OrderCreated"] ?? "order-created";

			_logger.LogInformation($"OrderProducer initialized. Topic: {_topic}, Servers: {kafkaConfig.BootstrapServers}");
		}

		public async Task PublishOrderCreatedAsync(OrderCreatedEvent evt)
		{
			try
			{
				var json = JsonSerializer.Serialize(evt);

				_logger.LogInformation($"Отправка события в Kafka. Topic: {_topic}, OrderId: {evt.OrderId}");

				var deliveryResult = await _producer.ProduceAsync(_topic, new Message<Null, string>
				{
					Value = json,
					Timestamp = Timestamp.Default
				});

				// КРИТИЧНО: Flush гарантирует отправку
				_producer.Flush(TimeSpan.FromSeconds(10));

				_logger.LogInformation(
					$"✓ Событие успешно отправлено в Kafka. " +
					$"Topic: {deliveryResult.Topic}, " +
					$"Partition: {deliveryResult.Partition}, " +
					$"Offset: {deliveryResult.Offset}, " +
					$"OrderId: {evt.OrderId}");
			}
			catch (ProduceException<Null, string> ex)
			{
				_logger.LogError(ex, $"✗ Ошибка отправки в Kafka. Topic: {_topic}, Error: {ex.Error.Reason}");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"✗ Неожиданная ошибка при отправке в Kafka");
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				// Flush перед закрытием
				_producer?.Flush(TimeSpan.FromSeconds(10));
				_producer?.Dispose();
				_logger.LogInformation("OrderProducer disposed");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при закрытии OrderProducer");
			}
		}
	}
}