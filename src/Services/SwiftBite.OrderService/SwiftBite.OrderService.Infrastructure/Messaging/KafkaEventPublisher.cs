using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SwiftBite.OrderService.Domain.Interfaces;
using System.Text.Json;

namespace SwiftBite.OrderService.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration[
                "Kafka:BootstrapServers"]
                ?? "localhost:9092",

            // ✅ Production grade settings
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }

    public async Task PublishAsync<T>(
        string topic, T message,
        CancellationToken ct = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);

            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            };

            var result = await _producer.ProduceAsync(
                topic, kafkaMessage, ct);

            _logger.LogInformation(
                "🔥 Kafka event published | Topic: {Topic} | " +
                "Partition: {Partition} | Offset: {Offset}",
                topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "❌ Kafka publish failed | Topic: {Topic}",
                topic);
            throw;
        }
    }
}