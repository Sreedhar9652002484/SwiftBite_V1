using System.Diagnostics;

namespace SwiftBite.ApiGateway.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
      public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // Log incoming request
            _logger.LogInformation(
                "[{RequestId}] ➡️ {Method} {Path} from {IP}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log outgoing response
                _logger.LogInformation(
                    "[{RequestId}] ⬅️ {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
