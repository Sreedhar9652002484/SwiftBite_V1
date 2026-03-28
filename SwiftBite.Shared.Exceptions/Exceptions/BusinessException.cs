namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class BusinessException : SwiftBiteException
{
    public BusinessException(
        string message,
        string errorCode = "BUSINESS_ERROR",
        Exception? innerException = null)
        : base(
            message,
            errorCode,
            422,
            "Business Rule Violation",
            null,
            innerException)
    {
    }
}