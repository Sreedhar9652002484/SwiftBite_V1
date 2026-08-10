using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftBite.UserService.Application.Common.Interfaces;
using SwiftBite.UserService.Domain.Interfaces;
using SwiftBite.UserService.Infrastructure.Caching;
using SwiftBite.UserService.Infrastructure.Persistence;
using SwiftBite.UserService.Infrastructure.Persistence.Repositories;

namespace SwiftBite.UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure( this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ EF Core — Separate UserService DB
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("UserServiceDb")));

        // ✅ Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = "SwiftBite_User_";
        });

        // ✅ Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();

        // ✅ Cache Service
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}