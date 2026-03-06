using SwiftBite.ApiGateway.Services;
using System.Text;

namespace SwiftBite.ApiGateway.Middleware;

public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cache;
    private readonly ILogger<CachingMiddleware> _logger;

    // ✅ Only cache these routes (GET requests only!)
    private static readonly Dictionary<string, TimeSpan> _cacheRules = new()
    {
        { "/api/restaurants",     TimeSpan.FromMinutes(5)  },  // Restaurant list
        { "/api/menu",            TimeSpan.FromMinutes(10) },  // Menu items
        { "/api/users/profile",   TimeSpan.FromMinutes(2)  },  // User profile
        { "/api/categories",      TimeSpan.FromMinutes(30) },  // Food categories
    };

    public CachingMiddleware(
        RequestDelegate next,
        ICacheService cache,
        ILogger<CachingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ✅ Only cache GET requests
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value?.ToLower() ?? "";

        // ✅ Check if this route should be cached
        var cacheRule = _cacheRules
            .FirstOrDefault(r => path.StartsWith(r.Key));

        if (cacheRule.Key == null)
        {
            await _next(context);
            return;
        }

        // ✅ Build cache key (include user ID for personalized data)
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "anonymous";
        var cacheKey = $"{path}:{userId}:{context.Request.QueryString}";

        // 🔍 Check cache first
        var cachedResponse = await _cache.GetAsync(cacheKey);
        if (cachedResponse != null)
        {
            _logger.LogInformation("⚡ Cache HIT: {Path}", path);
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Cache"] = "HIT";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        _logger.LogInformation("🔍 Cache MISS: {Path}", path);

        // 📥 Capture the response
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        // 💾 Cache successful responses only
        if (context.Response.StatusCode == 200)
        {
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            await _cache.SetAsync(cacheKey, responseBody, cacheRule.Value);
            context.Response.Headers["X-Cache"] = "MISS";

            _logger.LogInformation(
                "💾 Cached: {Path} for {Duration} mins",
                path, cacheRule.Value.TotalMinutes);

            memStream.Position = 0;
        }

        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}