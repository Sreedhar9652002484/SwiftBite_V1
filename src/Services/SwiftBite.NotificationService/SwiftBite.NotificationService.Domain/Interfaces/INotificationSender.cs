using SwiftBite.NotificationService.Domain.Entities;

namespace SwiftBite.NotificationService.Domain.Interfaces;

// ✅ SignalR + Firebase implement this
public interface INotificationSender
{
    Task SendToUserAsync(
        string userId, string title,
        string message, object? data = null,
        CancellationToken ct = default);

    Task SendPushNotificationAsync(
        string deviceToken, string title,
        string message, object? data = null,
        CancellationToken ct = default);
}