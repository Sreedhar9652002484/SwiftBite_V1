using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;
using SwiftBite.NotificationService.Infrastructure;
using SwiftBite.NotificationService.Infrastructure.Persistence;
using SwiftBite.NotificationService.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// ? 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/notificationservice-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ? 2. Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SwiftBite NotificationService API",
        Version = "v1",
        Description = "Real-time notifications via SignalR + Kafka"
    });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models
                        .SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models
                        .ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models
                            .ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ? 3. SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval =
        TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval =
        TimeSpan.FromSeconds(30);
});

// ? 4. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(SendNotificationCommand).Assembly));

// ? 5. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(SendNotificationCommand).Assembly);

// ? 6. Infrastructure
builder.Services.AddInfrastructure(
    builder.Configuration);

// ? 7. CORS — allow Angular + Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBitePolicy", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // ? Required for SignalR!
});

// ? 8. OpenIddict Validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(
            builder.Configuration["AuthServer:Authority"]!);
        options.AddAudiences(
            "swiftbite-notificationservice");

        options.UseIntrospection()
               .SetClientId(
                    "swiftbite-notificationservice")
               .SetClientSecret(
                    "notificationservice-secret");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults
        .AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

// ? Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<NotificationDbContext>();
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

// ?? Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();