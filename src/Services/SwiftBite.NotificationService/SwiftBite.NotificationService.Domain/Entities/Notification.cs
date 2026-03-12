using SwiftBite.NotificationService.Domain.Enums;

namespace SwiftBite.NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
        = string.Empty;
    public string Title { get; private set; }
        = string.Empty;
    public string Message { get; private set; }
        = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsRead { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        string userId, string title,
        string message, NotificationType type,
        NotificationChannel channel,
        string? referenceId = null,
        string? imageUrl = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Channel = channel,
            IsRead = false,
            ReferenceId = referenceId,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}