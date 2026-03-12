using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.PaymentService.Application.Common.Interfaces;
using SwiftBite.PaymentService.Domain.Interfaces;
using SwiftBite.PaymentService.Infrastructure.Caching;
using SwiftBite.PaymentService.Infrastructure.Messaging;
using SwiftBite.PaymentService.Infrastructure.Persistence;
using SwiftBite.PaymentService.Infrastructure.Persistence.Repositories;
using SwiftBite.PaymentService.Infrastructure.Razorpay;

namespace SwiftBite.PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ EF Core
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "PaymentServiceDb")));

        // ✅ Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration["Redis:ConnectionString"];
            options.InstanceName = "SwiftBite_Payment_";
        });

        // ✅ Repository
        services.AddScoped<IPaymentRepository,
            PaymentRepository>();

        // ✅ Cache
        services.AddScoped<ICacheService,
            RedisCacheService>();

        // 💳 Razorpay Service
        services.AddScoped<IRazorpayService,
            RazorpayService>();

        // 🔥 Kafka Publisher
        services.AddSingleton<IEventPublisher,
            KafkaEventPublisher>();

        return services;
    }
}