namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class UnauthorizedException : SwiftBiteException
{
    public UnauthorizedException(
        string message = "User is not authenticated.",
        Exception? innerException = null)
        : base(
            message,
            "UNAUTHORIZED",
            401,
            "Unauthorized",
            null,
            innerException)
    {
    }
}