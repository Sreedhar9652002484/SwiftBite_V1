using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence;

public class RestaurantDbContextFactory
    : IDesignTimeDbContextFactory<RestaurantDbContext>
{
    public RestaurantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<RestaurantDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=SwiftBite_RestaurantDb;" +
            "User Id=sa;Password=SwiftBite@2024!;" +
            "TrustServerCertificate=True");

        return new RestaurantDbContext(optionsBuilder.Options);
    }
}