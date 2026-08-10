using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftBite.UserService.Infrastructure.Persistence;

public class UserDbContextFactory
    : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=SwiftBite_UserDb;" +
            "User Id=sa;Password=SwiftBite@2024!;" +
            "TrustServerCertificate=True");

        return new UserDbContext(optionsBuilder.Options);
    }
}