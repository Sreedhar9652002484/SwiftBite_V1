using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftBite.OrderService.Infrastructure.Persistence;

public class OrderDbContextFactory
    : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<OrderDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=SwiftBite_OrderDb;" +
            "User Id=sa;Password=SwiftBite@2024!;" +
            "TrustServerCertificate=True");

        return new OrderDbContext(optionsBuilder.Options);
    }
}