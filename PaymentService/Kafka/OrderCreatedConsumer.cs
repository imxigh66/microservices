using Confluent.Kafka;
using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Services;
using System.Text.Json;

namespace PaymentService.Kafka
{
	public class OrderCreatedConsumer : BackgroundService
	{
		private readonly ILogger<OrderCreatedConsumer> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IConfiguration _configuration;
		private IConsumer<Ignore, string>? _consumer;

		public OrderCreatedConsumer(
			ILogger<OrderCreatedConsumer> logger,
			IServiceProvider serviceProvider,
			IConfiguration configuration)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_configuration = configuration;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("OrderCreatedConsumer запускается...");

			var config = new ConsumerConfig
			{
				BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "dev-kafka:9092",
				GroupId = "payment-service-group",
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = false,
				// ВАЖНО: Таймауты для надежности
				SessionTimeoutMs = 30000,
				HeartbeatIntervalMs = 10000,
				MaxPollIntervalMs = 300000
			};

			_consumer = new ConsumerBuilder<Ignore, string>(config)
				.SetErrorHandler((_, e) => _logger.LogError($"Kafka Error: {e.Reason}"))
				.SetLogHandler((_, log) => _logger.LogInformation($"Kafka Log: {log.Message}"))
				.Build();

			var topic = _configuration["Kafka:Topics:OrderCreated"] ?? "order-created";

			try
			{
				_consumer.Subscribe(topic);
				_logger.LogInformation($"✓ Подписка на топик: {topic}");
				_logger.LogInformation($"✓ Kafka Servers: {config.BootstrapServers}");
				_logger.LogInformation($"✓ Consumer Group: {config.GroupId}");

				while (!stoppingToken.IsCancellationRequested)
				{
					try
					{
						// Используем Poll вместо Consume для лучшего контроля
						var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));

						if (consumeResult != null && consumeResult.Message != null)
						{
							_logger.LogInformation($"📩 Получено сообщение из Kafka");
							_logger.LogInformation($"Topic: {consumeResult.Topic}");
							_logger.LogInformation($"Partition: {consumeResult.Partition}");
							_logger.LogInformation($"Offset: {consumeResult.Offset}");
							_logger.LogInformation($"Message: {consumeResult.Message.Value}");

							try
							{
								var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(
									consumeResult.Message.Value,
									new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

								if (orderEvent != null)
								{
									_logger.LogInformation($"Обработка заказа: {orderEvent.OrderId}");
									await ProcessOrderAsync(orderEvent);

									// Commit только после успешной обработки
									_consumer.Commit(consumeResult);
									_logger.LogInformation($"✓ Сообщение обработано и закоммичено");
								}
								else
								{
									_logger.LogWarning("Не удалось десериализовать событие");
								}
							}
							catch (JsonException jsonEx)
							{
								_logger.LogError(jsonEx, $"Ошибка JSON десериализации: {consumeResult.Message.Value}");
								// Commit чтобы не зависать на плохом сообщении
								_consumer.Commit(consumeResult);
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Ошибка обработки сообщения");
								// НЕ commit - повторим обработку
							}
						}

						await Task.Delay(100, stoppingToken);
					}
					catch (ConsumeException ex)
					{
						_logger.LogError(ex, $"Kafka ConsumeException: {ex.Error.Reason}");
						await Task.Delay(1000, stoppingToken);
					}
					catch (OperationCanceledException)
					{
						_logger.LogInformation("Consumer останавливается...");
						break;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Неожиданная ошибка в consumer loop");
						await Task.Delay(5000, stoppingToken);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Критическая ошибка в OrderCreatedConsumer");
				throw; // Позволит restart: on-failure сработать
			}
			finally
			{
				try
				{
					_consumer?.Close();
					_consumer?.Dispose();
					_logger.LogInformation("Consumer закрыт");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Ошибка при закрытии consumer");
				}
			}
		}

		private async Task ProcessOrderAsync(OrderCreatedEvent orderEvent)
		{
			using var scope = _serviceProvider.CreateScope();
			var paymentService = scope.ServiceProvider.GetRequiredService<Services.PaymentService>();

			try
			{
				_logger.LogInformation($"Создание платежа для заказа {orderEvent.OrderId}");

				var paymentRequest = new CreatePaymentIntentRequest
				{
					OrderId = orderEvent.OrderId.ToString(),
					Amount = orderEvent.TotalPrice,
					Currency = _configuration["Stripe:Currency"] ?? "usd",
					UserId = orderEvent.UserId
				};

				var result = await paymentService.CreatePaymentIntentAsync(paymentRequest);

				if (result.Success)
				{
					_logger.LogInformation($"✓ Платеж создан успешно. PaymentIntentId: {result.PaymentIntentId}");
				}
				else
				{
					_logger.LogError($"✗ Ошибка создания платежа: {result.ErrorMessage}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка обработки заказа {orderEvent.OrderId}");
				throw;
			}
		}

		public override void Dispose()
		{
			_consumer?.Close();
			_consumer?.Dispose();
			base.Dispose();
		}
	}
}