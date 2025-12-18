using Confluent.Kafka;
using System.Text.Json;
using UserService.Data;
using UserService.Events;
using UserService.Models;

namespace UserService.Services
{
    public class KafkaConsumerService: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<KafkaConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken); // Подождать запуска приложения
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            var topic = _configuration["Kafka:Topics:UserRegistered"];
            consumer.Subscribe(topic);

            _logger.LogInformation("Kafka Consumer запущен. Слушаем топик: {Topic}", topic);

            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        _logger.LogInformation("Получено сообщение из Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                            consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);

                        var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(consumeResult.Message.Value);

                        if (userRegisteredEvent != null)
                        {
                            await HandleUserRegisteredEvent(userRegisteredEvent);
                            consumer.Commit(consumeResult);

                            _logger.LogInformation(
                                " Профиль создан для пользователя: {Email}", userRegisteredEvent.Email);
                        }
                    }
                    catch (ConsumeException ex)
                    {

                        _logger.LogError(ex, "Ошибка при чтении из Kafka");
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке события");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(" Kafka Consumer остановлен");
                consumer.Close();
            }
        }

        private async Task HandleUserRegisteredEvent(UserRegisteredEvent userEvent)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            // Проверяем что профиль еще не создан
            var existingProfile = await dbContext.UserProfiles.FindAsync(userEvent.UserId);
            if (existingProfile != null)
            {
                _logger.LogWarning(" Профиль уже существует для UserId: {UserId}", userEvent.UserId);
                return;
            }

            // Создаем новый профиль
            var userProfile = new UserProfile
            {
                UserId = userEvent.UserId,
                Email = userEvent.Email,
                FirstName = userEvent.FirstName,
                LastName = userEvent.LastName,
                CreatedAt = userEvent.RegisteredAt
            };

            dbContext.UserProfiles.Add(userProfile);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                " UserProfile создан: UserId={UserId}, Email={Email}",
                userProfile.UserId, userProfile.Email);
        }
    }
}
