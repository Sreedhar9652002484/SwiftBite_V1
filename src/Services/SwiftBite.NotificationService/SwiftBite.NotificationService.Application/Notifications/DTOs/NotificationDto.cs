using SwiftBite.NotificationService.Domain.Enums;

namespace SwiftBite.NotificationService.Application.Notifications.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public string? ReferenceId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationListDto
{
    public IEnumerable<NotificationDto> Notifications
    { get; set; } = new List<NotificationDto>();
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
}