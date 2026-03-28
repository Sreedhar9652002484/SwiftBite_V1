using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Infrastructure;
using SwiftBite.OrderService.Infrastructure.Persistence;
using SwiftBite.Shared.Exceptions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ? 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/orderservice-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ? 2. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SwiftBite OrderService API",
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

// ? 3. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(PlaceOrderCommand).Assembly));

// ? 4. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(PlaceOrderCommand).Assembly);

// ? 5. Infrastructure
builder.Services.AddOrderInfrastructure(builder.Configuration);

// ? 6. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBitePolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ? 7. OpenIddict Validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(
            builder.Configuration["AuthServer:Authority"]!);
        options.AddAudiences("swiftbite-orderservice");

        options.UseIntrospection()
               .SetClientId("swiftbite-orderservice")
               .SetClientSecret("orderservice-secret");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseGlobalExceptionHandler();
// ? Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<OrderDbContext>();
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