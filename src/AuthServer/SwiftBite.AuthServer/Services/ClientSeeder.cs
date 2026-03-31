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
                Permissions.Prefixes.Scope + "swiftbite.payment",
                Permissions.Prefixes.Scope + "swiftbite.delivery",
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

        // ── Client 4: UserService ─────────────────────────────
        var userServiceClient = await manager
            .FindByClientIdAsync("swiftbite-userservice", ct);

        if (userServiceClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-userservice",
                ClientSecret = "userservice-secret",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite UserService",
                Permissions =
        {
            Permissions.Endpoints.Introspection, // ✅ validate tokens
        }
            }, ct);
        }
        else
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(descriptor, userServiceClient, ct);
            descriptor.Permissions.Add(Permissions.Endpoints.Introspection);
            await manager.UpdateAsync(userServiceClient, descriptor, ct);
        }

        // ── Client 5: RestaurantService ───────────────────────
        var restaurantClient = await manager
            .FindByClientIdAsync("swiftbite-restaurantservice", ct);

        if (restaurantClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-restaurantservice",
                ClientSecret = "restaurantservice-secret",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite RestaurantService",
                Permissions =
        {
            Permissions.Endpoints.Introspection,
        }
            }, ct);
        }

        // ── Client 6: OrderService ────────────────────────────
        var orderClient = await manager
            .FindByClientIdAsync("swiftbite-orderservice", ct);

        if (orderClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-orderservice",
                ClientSecret = "orderservice-secret",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite OrderService",
                Permissions =
        {
            Permissions.Endpoints.Introspection,
        }
            }, ct);
        }

        // ── Client 7: PaymentService ──────────────────────────
        var paymentClient = await manager
            .FindByClientIdAsync("swiftbite-paymentservice", ct);

        if (paymentClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-paymentservice",
                ClientSecret = "paymentservice-secret",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite PaymentService",
                Permissions =
        {
            Permissions.Endpoints.Introspection,
        }
            }, ct);
        }

        // ── Client 8: NotificationService ─────────────────────
        var notifClient = await manager
            .FindByClientIdAsync(
                "swiftbite-notificationservice", ct);

        if (notifClient is null)
        {
            await manager.CreateAsync(
                new OpenIddictApplicationDescriptor
                {
                    ClientId = "swiftbite-notificationservice",
                    ClientSecret = "notificationservice-secret",
                    ClientType = ClientTypes.Confidential,
                    DisplayName = "SwiftBite NotificationService",
                    Permissions =
                    {
                Permissions.Endpoints.Introspection,
                    }
                }, ct);
        }

        // ── Client: DeliveryService ───────────────────────────────────
        var deliveryClient = await manager
            .FindByClientIdAsync("swiftbite-deliveryservice", ct);

        if (deliveryClient is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swiftbite-deliveryservice",
                ClientSecret = "deliveryservice-secret",
                ClientType = ClientTypes.Confidential,
                DisplayName = "SwiftBite DeliveryService",
                Permissions =
        {
            Permissions.Endpoints.Introspection,
        }
            }, ct);
        }
    }



    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

