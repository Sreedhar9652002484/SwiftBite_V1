using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SwiftBite.AuthServer.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SwiftBite.AuthServer.Services;

public class ClientSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ClientSeeder(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        // ✅ Ensure DB is migrated before seeding
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync(ct);

        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        // ── Client 1: Angular Customer App ──────────────────
        if (await manager.FindByClientIdAsync("swiftbite-angular", ct) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-angular",
                ClientType = ClientTypes.Public, 
                DisplayName = "SwiftBite Angular App",
                RedirectUris =
                {
                    new Uri("http://localhost:4200/auth/callback")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:4200/auth/logout")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Logout,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.ResponseTypes.Code,

                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Prefixes.Scope + "profile",
                    Permissions.Prefixes.Scope + "email",
                    Permissions.Prefixes.Scope + "offline_access",
                    Permissions.Prefixes.Scope + "swiftbite.user",
                    Permissions.Prefixes.Scope + "swiftbite.order",
                    Permissions.Prefixes.Scope + "swiftbite.restaurant",
                }
            }, ct);
        }

        // ── Client 2: API Gateway (service-to-service) ───────
        var gatewayClient = await manager.FindByClientIdAsync("swiftbite-gateway", ct);
        if (gatewayClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-gateway",
                ClientSecret = "gateway-secret-change-in-production",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite API Gateway",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,        // ✅ Gateway can validate tokens
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "swiftbite.user",
                    Permissions.Prefixes.Scope + "swiftbite.restaurant",
                    Permissions.Prefixes.Scope + "swiftbite.order",
                    Permissions.Prefixes.Scope + "swiftbite.payment",
                    Permissions.Prefixes.Scope + "swiftbite.delivery",
                }
            }, ct);
        }
        else
        {
            // ✅ Update existing client to add Introspection permission
            var descriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(descriptor, gatewayClient, ct);

            descriptor.Permissions.Add(Permissions.Endpoints.Introspection);

            await manager.UpdateAsync(gatewayClient, descriptor, ct);
        }
        // ── Client 3: Restaurant Admin App ───────────────────
        if (await manager.FindByClientIdAsync("swiftbite-restaurant-portal", ct) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-restaurant-portal",
                ClientType = ClientTypes.Public,
                DisplayName = "SwiftBite Restaurant Portal",
                RedirectUris =
                {
                    new Uri("http://localhost:4200/auth/callback")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:4200/auth/logout")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Logout,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Prefixes.Scope + "profile",
                    Permissions.Prefixes.Scope + "swiftbite.restaurant",
                }
            }, ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

