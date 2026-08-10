using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftBite.PaymentService.Infrastructure.Persistence;

public class PaymentDbContextFactory
    : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<PaymentDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=SwiftBite_PaymentDb;" +
            "User Id=sa;Password=SwiftBite@2024!;" +
            "TrustServerCertificate=True");

        return new PaymentDbContext(optionsBuilder.Options);
    }
}