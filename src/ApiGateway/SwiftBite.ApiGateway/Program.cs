using AspNetCoreRateLimit;
using Serilog;
using SwiftBite.ApiGateway.Middleware;
using SwiftBite.ApiGateway.Services;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ✅ 2. YARP - load from yarp.json
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ✅ 3. Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"]
                            ?? "localhost:6379";
    options.InstanceName = "SwiftBite_GW_";
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379" },
        ConnectTimeout = 3000,
        AbortOnConnectFail = false,
        ConnectRetry = 2
    };
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// ✅ 4. Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// ✅ 5. OpenIddict Validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        var authority = builder.Configuration["AuthServer:Authority"];
        if (string.IsNullOrEmpty(authority))
            authority = "http://localhost:5149";

        options.SetIssuer(new Uri(authority));

        // ✅ Add ALL audiences
        options.AddAudiences("swiftbite-gateway");
        options.AddAudiences("swiftbite-userservice");
        options.AddAudiences("swiftbite-restaurantservice");
        options.AddAudiences("swiftbite-orderservice");
        options.AddAudiences("swiftbite-paymentservice");
        options.AddAudiences("swiftbite-deliveryservice");
        options.AddAudiences("swiftbite-notificationservice");

        options.UseIntrospection()
               .SetClientId("swiftbite-gateway")
               .SetClientSecret("gateway-secret-change-in-production");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

// ✅ Configure HttpClient
builder.Services.AddHttpClient();

// ✅ 6. Authentication & Authorization with POLICIES
builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

// ✅ ADD AUTHORIZATION POLICIES - USE CUSTOM NAMES (NOT "default"!)
builder.Services.AddAuthorization(options =>
{
    // ✅ Policy for PROTECTED routes (requires authentication)
    options.AddPolicy("RequireAuth", policy =>
    {
        policy.AddAuthenticationSchemes(
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    // ✅ Policy for PUBLIC routes (allows anonymous)
    options.AddPolicy("AllowPublic", policy =>
    {
        policy.RequireAssertion(_ => true);  // Always allow
    });
});

// ✅ 7. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBitePolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ✅ MIDDLEWARE PIPELINE - CRITICAL ORDER!
app.UseCors("SwiftBitePolicy");
app.UseIpRateLimiting();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<UserIdForwardingMiddleware>();

app.MapReverseProxy();

app.Run();
