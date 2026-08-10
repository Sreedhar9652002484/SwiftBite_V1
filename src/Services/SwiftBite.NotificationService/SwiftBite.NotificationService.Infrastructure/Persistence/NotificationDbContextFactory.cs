using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.NotificationService.Infrastructure.Persistence;

public class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(
        string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<NotificationDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("NotificationServiceDb"));

        return new NotificationDbContext(
            optionsBuilder.Options);
    }
}
