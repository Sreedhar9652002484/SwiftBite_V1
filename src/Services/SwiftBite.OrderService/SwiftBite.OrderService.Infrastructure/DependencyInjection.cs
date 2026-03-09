using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Domain.Interfaces;
using SwiftBite.OrderService.Infrastructure.Caching;
using SwiftBite.OrderService.Infrastructure.Messaging;
using SwiftBite.OrderService.Infrastructure.Persistence;
using SwiftBite.OrderService.Infrastructure.Persistence.Repositories;

namespace SwiftBite.OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ EF Core — Separate OrderService DB
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "OrderServiceDb")));

        // ✅ Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration["Redis:ConnectionString"];
            options.InstanceName = "SwiftBite_Order_";
        });

        // ✅ Repository
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ✅ Cache Service
        services.AddScoped<ICacheService, RedisCacheService>();

        // 🔥 Kafka Event Publisher
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}