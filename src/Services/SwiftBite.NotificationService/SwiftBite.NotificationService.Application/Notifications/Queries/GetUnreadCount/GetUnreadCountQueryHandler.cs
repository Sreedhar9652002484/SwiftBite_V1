using MediatR;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Application.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _repo;

    public GetUnreadCountQueryHandler(
        INotificationRepository repo) => _repo = repo;

    public async Task<int> Handle(
        GetUnreadCountQuery query, CancellationToken ct)
        => await _repo.GetUnreadCountAsync(
            query.UserId, ct);
}