using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SwiftBite.AuthServer.Models;

namespace SwiftBite.AuthServer.Services;

public interface IRestaurantProvisioningService
{
    Task<Guid> CreateRestaurantAsync(ApplicationUser owner, PartnerApplication application, CancellationToken ct);
}

// Provisions a Restaurant in RestaurantService when a RestaurantAdmin application is approved.
// Calls RestaurantService directly on its internal address, authenticating with the same
// client-credentials client/secret ClientSeeder already registers for the API Gateway.
public class RestaurantProvisioningService : IRestaurantProvisioningService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public RestaurantProvisioningService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<Guid> CreateRestaurantAsync(ApplicationUser owner, PartnerApplication application, CancellationToken ct)
    {
        var authIssuer = _config["AuthServer:Issuer"]!;
        var tokenResponse = await _http.PostAsync($"{authIssuer}/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "swiftbite-gateway",
            ["client_secret"] = _config["OpenIddictClients:GatewaySecret"]!,
            ["scope"] = "swiftbite.restaurant"
        }), ct);
        tokenResponse.EnsureSuccessStatusCode();
        using var tokenDoc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(ct));
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

        var restaurantServiceUrl = _config["Services:RestaurantServiceBaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: Services:RestaurantServiceBaseUrl");

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{restaurantServiceUrl}/api/restaurants");
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        createRequest.Headers.Add("X-User-Id", owner.Id);
        createRequest.Content = JsonContent.Create(new
        {
            Name = application.BusinessName,
            Description = "Newly onboarded restaurant — the owner can complete these details in Settings.",
            PhoneNumber = application.Phone,
            Email = owner.Email,
            Address = application.City,
            City = application.City,
            PinCode = "000000",
            Latitude = 0.0,
            Longitude = 0.0,
            CuisineType = 12, // MultiCuisine — owner can refine later in Restaurant Settings
            MinimumOrderAmount = 99m,
            AverageDeliveryTimeMinutes = 30
        });

        var createResponse = await _http.SendAsync(createRequest, ct);
        createResponse.EnsureSuccessStatusCode();

        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(ct));
        var id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        return Guid.Parse(id!);
    }
}
