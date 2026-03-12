using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.NotificationService.Application.Common.Interfaces;
using SwiftBite.NotificationService.Domain.Interfaces;
using SwiftBite.NotificationService.Infrastructure.Caching;
using SwiftBite.NotificationService.Infrastructure.Messaging;
using SwiftBite.NotificationService.Infrastructure.Persistence;
using SwiftBite.NotificationService.Infrastructure.Persistence.Repositories;
using SwiftBite.NotificationService.Infrastructure.SignalR;

namespace SwiftBite.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ EF Core
        services.AddDbContext<NotificationDbContext>(
            options => options.UseSqlServer(
                configuration.GetConnectionString(
                    "NotificationServiceDb")));

        // ✅ Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration["Redis:ConnectionString"];
            options.InstanceName =
                "SwiftBite_Notification_";
        });

        // ✅ Repositories
        services.AddScoped<INotificationRepository,
            NotificationRepository>();
        services.AddScoped<IUserDeviceRepository,
            UserDeviceRepository>();

        // ✅ Cache
        services.AddScoped<ICacheService,
            RedisCacheService>();

        // 🔥 SignalR Sender
        services.AddScoped<INotificationSender,
            SignalRNotificationSender>();

        // 🎧 Kafka Consumer — runs as background service
        services.AddHostedService<KafkaConsumerService>();

        return services;
    }
}