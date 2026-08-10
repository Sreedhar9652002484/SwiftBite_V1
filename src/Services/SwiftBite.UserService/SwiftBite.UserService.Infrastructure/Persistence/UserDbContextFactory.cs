using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.UserService.Infrastructure.Persistence;

public class UserDbContextFactory
    : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("UserServiceDb"));

        return new UserDbContext(optionsBuilder.Options);
    }
}
