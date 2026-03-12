namespace SwiftBite.NotificationService.Domain.Interfaces;

// ✅ Kafka consumer interface
public interface IEventConsumer
{
    Task StartConsumingAsync(CancellationToken ct);
}