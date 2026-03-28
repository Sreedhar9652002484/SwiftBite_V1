namespace SwiftBite.Shared.Exceptions.Exceptions;  // ✅ CORRECT NAMESPACE

public class ResourceNotFoundException : SwiftBiteException
{
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }

    public ResourceNotFoundException(
        string resourceType,
        string resourceId,
        Exception? innerException = null)
        : base(
            $"{resourceType} with ID '{resourceId}' not found.",
            "RESOURCE_NOT_FOUND",
            404,
            "Resource Not Found",
            new Dictionary<string, object>
            {
                { "ResourceType", resourceType },
                { "ResourceId", resourceId }
            },
            innerException)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}