

namespace SwiftBite.DeliveryService.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(
            string topic, T message,
            CancellationToken ct = default) where T : class;
    }
}
