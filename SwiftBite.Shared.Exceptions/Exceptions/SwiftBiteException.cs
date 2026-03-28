namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class SwiftBiteException : Exception
{
    public string ErrorCode { get; set; }
    public int HttpStatusCode { get; set; }
    public string Title { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime Timestamp { get; set; }

    public SwiftBiteException(
        string message,
        string errorCode = "SWIFTBITE_ERROR",
        int httpStatusCode = 500,
        string title = "Internal Server Error",
        Dictionary<string, object>? metadata = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
        Title = title;
        Metadata = metadata ?? new Dictionary<string, object>();
        Timestamp = DateTime.UtcNow;
    }
}