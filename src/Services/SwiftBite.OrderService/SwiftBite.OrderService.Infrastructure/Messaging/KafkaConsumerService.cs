using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SwiftBite.OrderService.Application.Orders.Commands.UpdateOrderStatus;
using SwiftBite.OrderService.Domain.Enums;
using SwiftBite.Shared.Kernel.Events;
using System.Text.Json;

namespace SwiftBite.OrderService.Infrastructure.Messaging;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _config;

    private readonly string[] _topics =
    [
        "swiftbite.delivery.accepted",
        "swiftbite.delivery.pickedup",
        "swiftbite.delivery.delivered"
    ];

    public KafkaConsumerService(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _services = services;
        _logger = logger;
        _config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "order-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "📦 Order Kafka consumer started | Topics: {Topics}",
            string.Join(", ", _topics));

        using var consumer =
            new ConsumerBuilder<string, string>(_config).Build();

        consumer.Subscribe(_topics);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(100));
                if (result?.Message is null) continue;

                _logger.LogInformation(
                    "📨 Received | Topic: {Topic} | Offset: {Offset}",
                    result.Topic, result.Offset.Value);

                await ProcessMessageAsync(result.Topic, result.Message.Value, ct);
                consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "❌ Kafka consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Message processing error");
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(
        string topic, string message, CancellationToken ct)
    {
        switch (topic)
        {
            case "swiftbite.delivery.accepted":
                await HandleDeliveryAccepted(message, ct);
                break;
            case "swiftbite.delivery.pickedup":
                await HandleDeliveryPickedUp(message, ct);
                break;
            case "swiftbite.delivery.delivered":
                await HandleDeliveryDelivered(message, ct);
                break;
        }
    }

    // 🛵 Partner accepted → Order = PickedUp
    private async Task HandleDeliveryAccepted(
        string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<DeliveryJobAcceptedEvent>(message)!;
        _logger.LogInformation(
            "🛵 Delivery accepted for Order {OrderId}", evt.OrderId);

        await UpdateOrderStatus(evt.OrderId, OrderStatus.PickedUp, ct);
    }

    // 📦 Partner picked up → Order = OutForDelivery
    private async Task HandleDeliveryPickedUp(
        string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<DeliveryJobPickedUpEvent>(message)!;
        _logger.LogInformation(
            "📦 Order picked up {OrderId}", evt.OrderId);

        await UpdateOrderStatus(evt.OrderId, OrderStatus.OutForDelivery, ct);
    }

    // ✅ Partner delivered → Order = Delivered
    private async Task HandleDeliveryDelivered(
        string message, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<DeliveryJobDeliveredEvent>(message)!;
        _logger.LogInformation(
            "✅ Order delivered {OrderId}", evt.OrderId);

        await UpdateOrderStatus(evt.OrderId, OrderStatus.Delivered, ct);
    }

    private async Task UpdateOrderStatus(
        Guid orderId, OrderStatus newStatus, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new UpdateOrderStatusCommand(
                orderId,
                "system",    // ← system triggered, no user
                newStatus,
                null),       // ← no rowVersion for system updates
            ct);
    }
}