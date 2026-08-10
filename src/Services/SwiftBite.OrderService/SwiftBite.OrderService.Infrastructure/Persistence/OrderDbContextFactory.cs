using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.OrderService.Infrastructure.Persistence;

public class OrderDbContextFactory
    : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<OrderDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("OrderServiceDb"));

        return new OrderDbContext(optionsBuilder.Options);
    }
}
