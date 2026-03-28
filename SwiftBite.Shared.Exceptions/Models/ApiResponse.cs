using System.Text.Json.Serialization;

namespace SwiftBite.Shared.Exceptions.Models;  // ✅ CORRECT NAMESPACE

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    public static ApiResponse<T> SuccessResponse(
        T? data = default,
        string message = "Operation completed successfully.",
        string? traceId = null)
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };

    public static ApiResponse<T> ErrorResponse(
        string message,
        string? traceId = null)
        => new()
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };
}

public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    public static ApiResponse SuccessResponse(
        string message = "Operation completed successfully.",
        string? traceId = null)
        => new()
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };

    public static ApiResponse ErrorResponse(
        string message,
        string? errorCode = null,
        Dictionary<string, string[]>? errors = null,
        string? traceId = null)
        => new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };
}