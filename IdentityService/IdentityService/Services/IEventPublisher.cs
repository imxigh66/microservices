namespace IdentityService.Services
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topic,T eventData) where T : class;
    }
}
