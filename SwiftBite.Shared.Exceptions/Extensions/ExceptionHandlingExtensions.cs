using Microsoft.AspNetCore.Builder;
using SwiftBite.Shared.Exceptions.Middleware;

namespace SwiftBite.Shared.Exceptions.Extensions;  // ✅ CORRECT NAMESPACE

public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the pipeline.
    /// Must be called FIRST in the middleware pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}