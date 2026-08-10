using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Infrastructure.SignalR;

public class SignalRNotificationSender
    : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<SignalRNotificationSender>
        _logger;

    public SignalRNotificationSender(
        IHubContext<NotificationHub> hub,
        ILogger<SignalRNotificationSender> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    // ✅ Send real-time to specific user via SignalR
    public async Task SendToUserAsync(
        string userId, string title,
        string message, object? data = null,
        CancellationToken ct = default)
    {
        try
        {
            // Sends to ALL connections of this user!
            await _hub.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification",
                    new
                    {
                        title,
                        message,
                        data,
                        timestamp = DateTime.UtcNow
                    }, ct);

            _logger.LogInformation(
                "📨 SignalR sent | User: {UserId} | " +
                "Title: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ SignalR send failed | User: {UserId}",
                userId);
        }
    }

    // ✅ Firebase push — placeholder for now
    public async Task SendPushNotificationAsync(
        string deviceToken, string title,
        string message, object? data = null,
        CancellationToken ct = default)
    {
        // TODO: Wire up FirebaseAdmin SDK
        _logger.LogInformation(
            "📱 Push notification | Token: {Token} | " +
            "Title: {Title}",
            deviceToken[..10] + "...", title);

        await Task.CompletedTask;
    }
}