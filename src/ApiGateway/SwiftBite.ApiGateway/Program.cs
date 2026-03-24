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

// ✅ 3. Redis Cache (your existing Docker Redis!)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"]
                            ?? "localhost:6379";
    options.InstanceName = "SwiftBite_GW_";
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379" },
        ConnectTimeout = 3000,
        AbortOnConnectFail = false,  // ✅ Don't crash if Redis is down!
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
        options.SetIssuer(builder.Configuration["AuthServer:Authority"]!);
        options.AddAudiences("swiftbite-gateway");

        options.UseIntrospection()
               .SetClientId("swiftbite-gateway")
               .SetClientSecret("gateway-secret-change-in-production");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

// ✅ Configure HttpClient to not fail on startup
builder.Services.AddHttpClient();
// ✅ 6. Set default auth scheme
builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBitePolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Middleware Pipeline (ORDER MATTERS!)
app.UseIpRateLimiting();            // 1️⃣ Rate limit first
app.UseSerilogRequestLogging();     // 2️⃣ Log all requests
app.UseCors("SwiftBitePolicy");     // 3️⃣ CORS for Angular
app.UseMiddleware<LoggingMiddleware>();          // 4️⃣ Custom logging
app.UseMiddleware<AuthenticationMiddleware>();   // 5️⃣ JWT validation
app.UseMiddleware<CachingMiddleware>(); // 6️⃣ 🆕 Cache GET responses!

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserIdForwardingMiddleware>();  // ← here

app.MapReverseProxy();              // 6️⃣ Forward to services

app.Run();