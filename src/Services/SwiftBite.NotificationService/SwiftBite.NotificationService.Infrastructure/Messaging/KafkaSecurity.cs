using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.NotificationService.Infrastructure.Messaging;

public static class KafkaSecurity
{
    // SASL_SSL is required by hosted brokers like Upstash; local Kafka stays PLAINTEXT when unset
    public static void Apply(ClientConfig config, IConfiguration configuration)
    {
        var username = configuration["Kafka:SaslUsername"];
        if (string.IsNullOrEmpty(username))
            return;

        config.SecurityProtocol = SecurityProtocol.SaslSsl;
        config.SaslMechanism = SaslMechanism.ScramSha256;
        config.SaslUsername = username;
        config.SaslPassword = configuration["Kafka:SaslPassword"];
    }
}
