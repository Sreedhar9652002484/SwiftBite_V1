namespace SwiftBite.OrderService.Domain.Interfaces;

// ✅ Kafka event publisher interface
// Infrastructure implements this — Domain stays clean!
public interface IEventPublisher
{
    Task PublishAsync<T>(
        string topic, T message,
        CancellationToken ct = default) where T : class;
}