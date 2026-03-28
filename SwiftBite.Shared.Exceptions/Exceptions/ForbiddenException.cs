namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class ForbiddenException : SwiftBiteException
{
    public ForbiddenException(
        string message = "Access forbidden.",
        Exception? innerException = null)
        : base(
            message,
            "FORBIDDEN",
            403,
            "Access Forbidden",
            null,
            innerException)
    {
    }
}