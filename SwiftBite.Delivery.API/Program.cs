using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using SwiftBite.DeliveryService.Application;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.RegisterPartner;
using SwiftBite.DeliveryService.Infrastructure;
using SwiftBite.DeliveryService.Infrastructure.Persistence;
using SwiftBite.Shared.Exceptions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ? 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/deliveryservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ? 2. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SwiftBite DeliveryService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ? 3. MediatR — scans Application assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(RegisterPartnerCommand).Assembly));

// ? 4. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(RegisterPartnerCommand).Assembly);

// ? 5. Infrastructure (EF Core + Repositories)
builder.Services.AddDeliveryInfrastructure(builder.Configuration);

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

// ? 7. OpenIddict Validation — local JWT (no introspection)
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["AuthServer:Authority"]!);
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
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
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