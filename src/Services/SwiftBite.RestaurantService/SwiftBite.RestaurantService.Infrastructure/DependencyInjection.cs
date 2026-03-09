using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Domain.Interfaces;
using SwiftBite.RestaurantService.Infrastructure.Caching;
using SwiftBite.RestaurantService.Infrastructure.Persistence;
using SwiftBite.RestaurantService.Infrastructure.Persistence.Repositories;


namespace SwiftBite.UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRestaurantInfraStructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ EF Core — Separate UserService DB
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("RestaurantServiceDb")));

        // ✅ Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = "SwiftBite_RestaurantDb";
        });

        // ✅ Repositories
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        // ✅ Cache Service
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}