using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Infrastructure;
using SwiftBite.RestaurantService.Infrastructure.Persistence;
using SwiftBite.Shared.Exceptions.Extensions;
using SwiftBite.UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/restaurantservice-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ✅ 2. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SwiftBite RestaurantService API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new()
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id   = "Bearer"
            }},
            Array.Empty<string>()
        }
    });
});

// ✅ 3. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(CreateRestaurantCommand).Assembly));

// ✅ 4. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(CreateRestaurantCommand).Assembly);

// ✅ 5. Infrastructure
builder.Services.AddRestaurantInfraStructure(builder.Configuration);


// Replace this line:
// builder.Services.AddInfrastructure(builder.Configuration);

// ✅ 6. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBitePolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ✅ 7. OpenIddict Validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(
            builder.Configuration["AuthServer:Authority"]!);
        options.AddAudiences("swiftbite-restaurantservice");

        var clientSecret = builder.Configuration["OpenIddictClients:RestaurantServiceSecret"]
            ?? throw new InvalidOperationException("Missing required configuration: OpenIddictClients:RestaurantServiceSecret");

        options.UseIntrospection()
               .SetClientId("swiftbite-restaurantservice")
               .SetClientSecret(clientSecret);

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();


builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("RestaurantServiceDb"),
        name: "SwiftBite_RestaurantDb")
    .AddKafka(
        config => {
            config.BootstrapServers =
                builder.Configuration["Kafka:BootstrapServers"];
            // e.g. "localhost:9092" or "kafka:29092" inside docker
        },
        name: "kafka",
        tags: new[] { "messaging" });

var app = builder.Build();

app.UseGlobalExceptionHandler();
// ✅ Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<RestaurantDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SwiftBitePolicy");
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();