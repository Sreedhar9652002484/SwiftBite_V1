using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SwiftBite.AuthServer.Data;
using SwiftBite.AuthServer.Models;
using SwiftBite.AuthServer.Models.Validators;
using SwiftBite.AuthServer.Services;

var builder = WebApplication.CreateBuilder(args);


var authIssuer = builder.Configuration["AuthServer:Issuer"]!;
var angularBaseUrl = builder.Configuration["AuthServer:AngularBaseUrl"]!;

//builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ?? 1. EF Core ??????????????????????????????????????????????
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection"));

    // Tell EF Core that OpenIddict will use this DbContext
    options.UseOpenIddict();
});

// ?? 2. ASP.NET Identity ?????????????????????????????????????
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // User rules
    options.User.RequireUniqueEmail = true;

    // Lockout rules
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";

        // Get extra claims from Google
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

// ?? 3. OpenIddict ????????????????????????????????????????????
builder.Services.AddOpenIddict()

    // Register OpenIddict core — manages applications, tokens, authorizations
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<AuthDbContext>();
    })

    // Register OpenIddict server — the actual OAuth2/OIDC server
    .AddServer(options =>
    {
        // ?? Endpoints exposed by Auth Server ??
        // ✅ Force issuer without trailing slash
        //options.SetIssuer(new Uri("http://localhost:5149"));
        options.SetIssuer(new Uri(authIssuer));
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserinfoEndpointUris("/connect/userinfo")
               .SetLogoutEndpointUris("/connect/logout")
               .SetIntrospectionEndpointUris("/connect/introspect");

        // ?? Supported flows ??
        options.AllowAuthorizationCodeFlow()   // For web/Angular apps
               .AllowRefreshTokenFlow()        // For refresh tokens
               .AllowClientCredentialsFlow()  // For service-to-service
               .AllowPasswordFlow(); // 👈 REQUIRED

        // ?? Token lifetimes ??
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(60))
               .SetRefreshTokenLifetime(TimeSpan.FromDays(7))
               .SetIdentityTokenLifetime(TimeSpan.FromMinutes(60));

        // ?? Scopes SwiftBite services will use ??
        options.RegisterScopes(
            "openid",
            "profile",
            "email",
            "offline_access",       // For refresh tokens
            "swiftbite.user",       // UserService scope
            "swiftbite.restaurant", // RestaurantService scope
            "swiftbite.order",      // OrderService scope
            "swiftbite.payment",    // PaymentService scope
            "swiftbite.delivery"    // DeliveryService scope
        );

        // ?? Development: use dev signing cert ??
        // Production: replace with real X.509 certificate
        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

            // Disable HTTPS requirement for local dev
            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough()
                   .DisableTransportSecurityRequirement()
                    .EnableStatusCodePagesIntegration();
        }
        else
        {
            // TODO: Load real cert from Azure Key Vault in production
            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough();
        }
    })

    // Register OpenIddict validation — validates tokens in THIS server
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// ?? 4. CORS — Allow Angular UI ??????????????????????????????
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwiftBiteUI", policy =>
         policy.WithOrigins(angularBaseUrl)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials());
});

// ?? 5. Register ClientSeeder ?????????????????????????????????
builder.Services.AddHostedService<RoleSeeder>();     
builder.Services.AddHostedService<ClientSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("SwiftBiteUI");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapControllers();
app.MapGet("/", () => Results.Redirect(angularBaseUrl));
app.Run();