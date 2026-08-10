using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftBite.NotificationService.Infrastructure.Persistence;

public class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(
        string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<NotificationDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;" +
            "Database=SwiftBite_NotificationDb;" +
            "User Id=sa;Password=SwiftBite@2024!;" +
            "TrustServerCertificate=True");

        return new NotificationDbContext(
            optionsBuilder.Options);
    }
}