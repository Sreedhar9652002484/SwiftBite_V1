namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class ConflictException : SwiftBiteException
{
    public ConflictException(
        string message,
        string errorCode = "CONFLICT",
        Exception? innerException = null)
        : base(
            message,
            errorCode,
            409,
            "Conflict",
            null,
            innerException)
    {
    }
}