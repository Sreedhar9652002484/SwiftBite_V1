using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Interfaces;
using SwiftBite.DeliveryService.Infrastructure.Persistence;
using SwiftBite.DeliveryService.Infrastructure.Persistence.Repositories;

namespace SwiftBite.DeliveryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeliveryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("RestaurantServiceDb")));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration["Redis:ConnectionString"];
            options.InstanceName = "SwiftBite_Delivery_";
        });

        services.AddScoped<IDeliveryPartnerRepository, DeliveryPartnerRepository>();
        services.AddScoped<IDeliveryJobRepository, DeliveryJobRepository>();

        return services;
    }
}