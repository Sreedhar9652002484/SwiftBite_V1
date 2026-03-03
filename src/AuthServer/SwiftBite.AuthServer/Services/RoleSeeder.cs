using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SwiftBite.AuthServer.Data;

namespace SwiftBite.AuthServer.Services
{
    // Services/RoleSeeder.cs
    public class RoleSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public RoleSeeder(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await db.Database.MigrateAsync(ct); // ✅ add this


            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            // All SwiftBite roles
            string[] roles = [
             "Customer",
            "RestaurantAdmin",
            "DeliveryPartner",
            "Admin"
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
