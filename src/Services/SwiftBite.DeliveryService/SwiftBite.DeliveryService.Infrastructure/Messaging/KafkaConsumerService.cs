    // SwiftBite.DeliveryService.Infrastructure/Messaging/KafkaConsumerService.cs
    using Confluent.Kafka;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SwiftBite.DeliveryService.Domain.Domain.Entities;
    using SwiftBite.DeliveryService.Domain.Interfaces;
    using SwiftBite.DeliveryService.Infrastructure.Persistence;
using SwiftBite.Shared.Kernel.Events;
using System.Text.Json;

    namespace SwiftBite.DeliveryService.Infrastructure.Messaging;

    public class KafkaConsumerService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly ConsumerConfig _config;

        private readonly string[] _topics =
        [
            "swiftbite.order.ready"
            // Add more topics here later e.g. order.cancelled
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
                GroupId = "delivery-service",   // ← different group from notification-service
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false  // Manual commit like yours
            };
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation(
                "🚚 Delivery Kafka consumer started | Topics: {Topics}",
                string.Join(", ", _topics));

            using var consumer =
                new ConsumerBuilder<string, string>(_config).Build();

            consumer.Subscribe(_topics);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(
                        TimeSpan.FromMilliseconds(100));

                    if (result?.Message is null) continue;

                    _logger.LogInformation(
                        "📨 Kafka received | Topic: {Topic} | Offset: {Offset}",
                        result.Topic, result.Offset.Value);

                    await ProcessMessageAsync(
                        result.Topic, result.Message.Value, ct);

                    consumer.Commit(result);  // Manual commit after processing
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
                case "swiftbite.order.ready":
                    await HandleOrderReady(message, ct);
                    break;
            }
        }

        // 🍔 Order ready → create DeliveryJob
        private async Task HandleOrderReady(
            string message, CancellationToken ct)
        {
        // 🪵 ADD THIS FIRST
        _logger.LogInformation("🔥 HandleOrderReady called | Raw message: {Message}", message);

        var evt = JsonSerializer.Deserialize<OrderReadyEvent>(message)!;

        // 🪵 ADD THIS SECOND
        _logger.LogInformation("🔥 Deserialized OrderId: {OrderId}", evt?.OrderId);

        using var scope = _services.CreateScope();
            var jobRepo = scope.ServiceProvider
                .GetRequiredService<IDeliveryJobRepository>();
            var db = scope.ServiceProvider
              .GetRequiredService<DeliveryDbContext>();  // ← just use DbContext

            // ✅ Prevent duplicate jobs if message consumed twice
            var existing = await jobRepo.GetByOrderIdAsync(evt.OrderId, ct);
            if (existing is not null)
            {
                _logger.LogWarning(
                    "DeliveryJob already exists for Order {OrderId}, skipping.",
                    evt.OrderId);
                return;

            }

            // ✅ Create DeliveryJob
            var job = DeliveryJob.Create(
                orderId: evt.OrderId,
                customerId: evt.CustomerId,  // ✅ ADD
                orderNumber: evt.OrderNumber,
                restaurantName: evt.RestaurantName,
                customerName: evt.CustomerName,
                customerPhone: evt.CustomerPhone,
                pickupAddress: evt.RestaurantName, // use restaurant address if available
                deliveryAddress: evt.DeliveryAddress,
                deliveryCity: evt.DeliveryCity,
                deliveryFee: evt.DeliveryFee
            );

            await jobRepo.AddAsync(job, ct);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "✅ DeliveryJob {JobId} created for Order {OrderId}",
                job.Id, evt.OrderId);
        }
    }