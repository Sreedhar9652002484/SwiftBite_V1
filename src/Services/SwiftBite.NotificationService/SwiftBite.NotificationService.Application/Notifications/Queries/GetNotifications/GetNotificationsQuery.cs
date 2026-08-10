using MediatR;
using SwiftBite.NotificationService.Application.Notifications.DTOs;

namespace SwiftBite.NotificationService.Application.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    string UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<NotificationListDto>;