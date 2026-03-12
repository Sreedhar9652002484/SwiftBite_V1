using MediatR;

namespace SwiftBite.NotificationService.Application.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery(string UserId)
    : IRequest<int>;