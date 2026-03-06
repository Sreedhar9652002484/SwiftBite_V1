namespace SwiftBite.ApiGateway.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Routes that DON'T need auth
    private static readonly HashSet<string> _publicRoutes = new()
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh",
        "/api/auth/callback",
        "/connect/token",
        "/connect/authorize"
    };

    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // ✅ Skip auth for public routes
        if (_publicRoutes.Any(r => path.StartsWith(r)))
        {
            await _next(context);
            return;
        }

        // 🔐 Check Authorization header exists
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            _logger.LogWarning("🚫 No token provided for path: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Bearer token is required"
            });
            return;
        }

        // ✅ OpenIddict already validated token — just read claims
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "";
            var userRole = context.User.FindFirst("role")?.Value ?? "Customer";

            // Forward user info to downstream microservices
            context.Request.Headers["X-User-Id"] = userId;
            context.Request.Headers["X-User-Role"] = userRole;

            _logger.LogInformation("✅ Authenticated: UserId={UserId}, Role={Role}",
                userId, userRole);
        }

        await _next(context);
    }
}