using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SwiftBite.Shared.Exceptions.Exceptions;
using SwiftBite.Shared.Exceptions.Models;
using System.Net;
using System.Text.Json;

namespace SwiftBite.Shared.Exceptions.Middleware;  // ✅ CORRECT NAMESPACE

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = BuildExceptionResponse(
            exception,
            context.TraceIdentifier);

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };


        // ✅ Use this method that always exists
        var json = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(json);
    }

    private static ExceptionResponse BuildExceptionResponse(
        Exception exception,
        string? traceId)
    {
        return exception switch
        {
            SwiftBiteException swiftBiteEx => new ExceptionResponse
            {
                Success = false,
                Message = swiftBiteEx.Message,
                ErrorCode = swiftBiteEx.ErrorCode,
                Title = swiftBiteEx.Title,
                StatusCode = swiftBiteEx.HttpStatusCode,
                Timestamp = swiftBiteEx.Timestamp,
                TraceId = traceId,
                Metadata = swiftBiteEx.Metadata,
                Details = GetExceptionDetails(swiftBiteEx)
            },

            FluentValidation.ValidationException validationEx => new ExceptionResponse
            {
                Success = false,
                Message = "One or more validation errors occurred.",
                ErrorCode = "VALIDATION_ERROR",
                Title = "Validation Failed",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId,
                Errors = validationEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray())
            },

            KeyNotFoundException => new ExceptionResponse
            {
                Success = false,
                Message = exception.Message,
                ErrorCode = "RESOURCE_NOT_FOUND",
                Title = "Resource Not Found",
                StatusCode = (int)HttpStatusCode.NotFound,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId
            },

            UnauthorizedAccessException => new ExceptionResponse
            {
                Success = false,
                Message = "Access denied. You do not have permission to perform this operation.",
                ErrorCode = "FORBIDDEN",
                Title = "Access Forbidden",
                StatusCode = (int)HttpStatusCode.Forbidden,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId
            },

            InvalidOperationException => new ExceptionResponse
            {
                Success = false,
                Message = exception.Message,
                ErrorCode = "INVALID_OPERATION",
                Title = "Invalid Operation",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId,
                Details = exception.InnerException?.Message
            },

            ArgumentException argumentEx => new ExceptionResponse
            {
                Success = false,
                Message = $"Invalid argument: {argumentEx.ParamName}",
                ErrorCode = "INVALID_ARGUMENT",
                Title = "Invalid Argument",
                StatusCode = (int)HttpStatusCode.BadRequest,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId,
                Details = argumentEx.Message
            },

            _ => new ExceptionResponse
            {
                Success = false,
                Message = "An unexpected error occurred while processing your request.",
                ErrorCode = "INTERNAL_SERVER_ERROR",
                Title = "Internal Server Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId,
                Details = exception.Message
            }
        };
    }

    private static string? GetExceptionDetails(Exception exception)
    {
        return exception.InnerException?.Message;
    }
}