using MediatR;
using SwiftBite.NotificationService.Application.Notifications.DTOs;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Application.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery,
        NotificationListDto>
{
    private readonly INotificationRepository _repo;

    public GetNotificationsQueryHandler(
        INotificationRepository repo) => _repo = repo;

    public async Task<NotificationListDto> Handle(
        GetNotificationsQuery query, CancellationToken ct)
    {
        var notifications = await _repo
            .GetByUserIdAsync(
                query.UserId, query.Page,
                query.PageSize, ct);

        var unreadCount = await _repo
            .GetUnreadCountAsync(query.UserId, ct);

        var dtos = notifications.Select(n =>
            new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                ReferenceId = n.ReferenceId,
                ImageUrl = n.ImageUrl,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            }).ToList();

        return new NotificationListDto
        {
            Notifications = dtos,
            UnreadCount = unreadCount,
            TotalCount = dtos.Count
        };
    }
}