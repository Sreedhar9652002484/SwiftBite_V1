using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence;

public class RestaurantDbContextFactory
    : IDesignTimeDbContextFactory<RestaurantDbContext>
{
    public RestaurantDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<RestaurantDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("RestaurantServiceDb"));

        return new RestaurantDbContext(optionsBuilder.Options);
    }
}
