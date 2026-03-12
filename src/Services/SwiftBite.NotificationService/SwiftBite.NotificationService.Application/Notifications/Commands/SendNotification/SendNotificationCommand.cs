using MediatR;
using SwiftBite.NotificationService.Domain.Enums;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;

public record SendNotificationCommand(
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    NotificationChannel Channel,
    string? ReferenceId = null,
    string? ImageUrl = null,
    object? Data = null
) : IRequest<bool>;