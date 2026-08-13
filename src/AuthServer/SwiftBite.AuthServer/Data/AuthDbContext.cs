using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SwiftBite.AuthServer.Data.Converters;
using SwiftBite.AuthServer.Models;

namespace SwiftBite.AuthServer.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<PartnerApplication> PartnerApplications => Set<PartnerApplication>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.UseOpenIddict();

            // Additional configuration can be added here if needed
        }

        protected override void ConfigureConventions(
            ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // Globally ensure every DateTime/DateTime? property (e.g. ApplicationUser.CreatedAt,
            // PartnerApplication.CreatedAt/ReviewedAt) round-trips as UTC, since SQL Server's
            // datetime2 does not persist DateTimeKind.
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<UtcDateTimeConverter>();

            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableUtcDateTimeConverter>();
        }
    }
}
