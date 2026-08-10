using MediatR;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.MarkAllRead;

public record MarkAllReadCommand(string UserId)
    : IRequest<bool>;