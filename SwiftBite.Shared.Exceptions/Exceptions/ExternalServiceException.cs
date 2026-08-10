namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class ExternalServiceException : SwiftBiteException
{
    public string ServiceName { get; set; }

    public ExternalServiceException(
        string serviceName,
        string message,
        Exception? innerException = null)
        : base(
            $"External service '{serviceName}' error: {message}",
            $"{serviceName.ToUpper()}_ERROR",
            503,
            "Service Unavailable",
            new Dictionary<string, object>
            {
                { "ServiceName", serviceName }
            },
            innerException)
    {
        ServiceName = serviceName;
    }
}