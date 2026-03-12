using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;
using SwiftBite.PaymentService.Infrastructure;
using SwiftBite.PaymentService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ? 1. Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/paymentservice-.txt",
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
        Title = "SwiftBite PaymentService API",
        Version = "v1",
        Description = "Razorpay payment integration for SwiftBite"
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
        typeof(InitiatePaymentCommand).Assembly));

// ? 4. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(InitiatePaymentCommand).Assembly);

// ? 5. Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

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
        options.AddAudiences("swiftbite-paymentservice");

        options.UseIntrospection()
               .SetClientId("swiftbite-paymentservice")
               .SetClientSecret("paymentservice-secret");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

// ? Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<PaymentDbContext>();
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