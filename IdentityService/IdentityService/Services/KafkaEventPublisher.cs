
using Confluent.Kafka;

namespace IdentityService.Services
{
    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<string,string> _producer;
        private readonly ILogger<KafkaEventPublisher> _logger;

        public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
        {
            _logger = logger;

            var config=new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                MessageTimeoutMs = 5000,
                CompressionType = CompressionType.Snappy
            };

            _producer= new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka Producer Error: {e.Reason}"))
                .Build();
        }
        public async Task PublishAsync<T>(string topic, T eventData) where T : class
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = System.Text.Json.JsonSerializer.Serialize(eventData),
                    Timestamp = Timestamp.Default
                };
                var result = await _producer.ProduceAsync(topic, message);

                _logger.LogInformation(
                    "Событие опубликовано в Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    result.Topic, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "❌ Ошибка публикации события в Kafka. Topic: {Topic}", topic);
                throw;
            }
        }
        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
