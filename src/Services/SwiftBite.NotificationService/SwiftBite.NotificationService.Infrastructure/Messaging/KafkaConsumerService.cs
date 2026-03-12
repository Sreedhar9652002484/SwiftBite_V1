using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SwiftBite.NotificationService.Application.Events;
using SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;
using SwiftBite.NotificationService.Domain.Enums;
using System.Text.Json;

namespace SwiftBite.NotificationService.Infrastructure.Messaging;

// ✅ Background service — runs forever consuming Kafka
public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _config;

    // ✅ Topics we listen to
    private readonly string[] _topics =
    [
        "swiftbite.order.placed",
        "swiftbite.order.confirmed",
        "swiftbite.order.cancelled",
        "swiftbite.payment.success",
        "swiftbite.payment.failed"
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
            BootstrapServers =
                configuration["Kafka:BootstrapServers"]
                ?? "localhost:9092",
            GroupId = "notification-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false  // Manual commit!
        };
    }

    protected override async Task ExecuteAsync(
        CancellationToken ct)
    {
        _logger.LogInformation(
            "🎧 Kafka consumer started | " +
            "Topics: {Topics}",
            string.Join(", ", _topics));

        using var consumer =
            new ConsumerBuilder<string, string>(_config)
                .Build();

        consumer.Subscribe(_topics);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(
                    TimeSpan.FromMilliseconds(100));

                if (result?.Message is null) continue;

                _logger.LogInformation(
                    "📨 Kafka received | " +
                    "Topic: {Topic} | " +
                    "Offset: {Offset}",
                    result.Topic,
                    result.Offset.Value);

                await ProcessMessageAsync(
                    result.Topic,
                    result.Message.Value, ct);

                // ✅ Manual commit after processing!
                consumer.Commit(result);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex,
                    "❌ Kafka consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Message processing error");
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(
        string topic, string message,
        CancellationToken ct)
    {
        using var scope = _services
            .CreateScope();
        var mediator = scope.ServiceProvider
            .GetRequiredService<IMediator>();

        switch (topic)
        {
            case "swiftbite.order.placed":
                await HandleOrderPlaced(
                    mediator, message, ct);
                break;

            case "swiftbite.order.confirmed":
                await HandleOrderConfirmed(
                    mediator, message, ct);
                break;

            case "swiftbite.order.cancelled":
                await HandleOrderCancelled(
                    mediator, message, ct);
                break;

            case "swiftbite.payment.success":
                await HandlePaymentSuccess(
                    mediator, message, ct);
                break;

            case "swiftbite.payment.failed":
                await HandlePaymentFailed(
                    mediator, message, ct);
                break;
        }
    }

    // 🛒 Order placed → notify customer
    private async Task HandleOrderPlaced(
        IMediator mediator, string message,
        CancellationToken ct)
    {
        var evt = JsonSerializer
            .Deserialize<OrderPlacedNotificationEvent>(
                message)!;

        await mediator.Send(
            new SendNotificationCommand(
                evt.CustomerId,
                "🛒 Order Placed!",
                $"Your order from {evt.RestaurantName} " +
                $"has been placed! Total: ₹{evt.TotalAmount}",
                NotificationType.OrderPlaced,
                NotificationChannel.SignalR,
                evt.OrderId.ToString()), ct);
    }

    // ✅ Order confirmed → notify customer
    private async Task HandleOrderConfirmed(
        IMediator mediator, string message,
        CancellationToken ct)
    {
        var evt = JsonSerializer
            .Deserialize<OrderStatusChangedEvent>(
                message)!;

        await mediator.Send(
            new SendNotificationCommand(
                evt.CustomerId,
                "✅ Order Confirmed!",
                $"{evt.RestaurantName} accepted your order! " +
                $"Estimated time: {evt.EstimatedMinutes} mins",
                NotificationType.OrderConfirmed,
                NotificationChannel.SignalR,
                evt.OrderId.ToString()), ct);
    }

    // ❌ Order cancelled → notify customer
    private async Task HandleOrderCancelled(
        IMediator mediator, string message,
        CancellationToken ct)
    {
        var evt = JsonSerializer
            .Deserialize<OrderStatusChangedEvent>(
                message)!;

        await mediator.Send(
            new SendNotificationCommand(
                evt.CustomerId,
                "❌ Order Cancelled",
                $"Your order from {evt.RestaurantName} " +
                "has been cancelled. Refund will be processed.",
                NotificationType.OrderCancelled,
                NotificationChannel.SignalR,
                evt.OrderId.ToString()), ct);
    }

    // 💳 Payment success → notify customer
    private async Task HandlePaymentSuccess(
        IMediator mediator, string message,
        CancellationToken ct)
    {
        var evt = JsonSerializer
            .Deserialize<PaymentNotificationEvent>(
                message)!;

        await mediator.Send(
            new SendNotificationCommand(
                evt.CustomerId,
                "💳 Payment Successful!",
                $"₹{evt.Amount} paid successfully. " +
                "Your order is confirmed!",
                NotificationType.PaymentSuccess,
                NotificationChannel.SignalR,
                evt.OrderId.ToString()), ct);
    }

    // ❌ Payment failed → notify customer
    private async Task HandlePaymentFailed(
        IMediator mediator, string message,
        CancellationToken ct)
    {
        var evt = JsonSerializer
            .Deserialize<PaymentNotificationEvent>(
                message)!;

        await mediator.Send(
            new SendNotificationCommand(
                evt.CustomerId,
                "❌ Payment Failed",
                $"Payment of ₹{evt.Amount} failed. " +
                $"Reason: {evt.FailureReason}. Please retry.",
                NotificationType.PaymentFailed,
                NotificationChannel.SignalR,
                evt.OrderId.ToString()), ct);
    }
}