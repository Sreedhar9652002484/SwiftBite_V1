namespace SwiftBite.ApiGateway.Middleware;

/// <summary>
/// Reads the authenticated user's "sub" claim from the JWT
/// and injects it as X-User-Id header before YARP forwards the request.
/// This lets downstream microservices call GetAuthUserId() reliably.
/// </summary>
public class UserIdForwardingMiddleware
{
    private readonly RequestDelegate _next;

    public UserIdForwardingMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst("sub")?.Value
                   ?? context.User.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var role = context.User.FindFirst("role")?.Value
                    ?? context.User.FindFirst(
                        System.Security.Claims.ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(sub))
                context.Request.Headers["X-User-Id"] = sub;

            if (!string.IsNullOrEmpty(role))
                context.Request.Headers["X-User-Role"] = role;
        }

        await _next(context);
    }
}