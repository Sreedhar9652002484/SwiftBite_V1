using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.PaymentService.Infrastructure.Persistence;

public class PaymentDbContextFactory
    : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<PaymentDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("PaymentServiceDb"));

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
