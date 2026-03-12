namespace SwiftBite.NotificationService.Domain.Enums;

public enum NotificationChannel
{
    SignalR = 1,  // Real-time web push
    Push = 2,  // Firebase mobile push
    Email = 3,  // Email notification
    SMS = 4   // SMS via Twilio
}