namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class ValidationException : SwiftBiteException
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationException(
        Dictionary<string, string[]> errors,
        Exception? innerException = null)
        : base(
            "One or more validation errors occurred.",
            "VALIDATION_ERROR",
            400,
            "Validation Failed",
            null,
            innerException)
    {
        Errors = errors;
        Metadata = new Dictionary<string, object>
        {
            { "ValidationErrors", errors }
        };
    }

    public ValidationException(string message)
        : base(
            message,
            "VALIDATION_ERROR",
            400,
            "Validation Failed")
    {
        Errors = new Dictionary<string, string[]>();
    }
}