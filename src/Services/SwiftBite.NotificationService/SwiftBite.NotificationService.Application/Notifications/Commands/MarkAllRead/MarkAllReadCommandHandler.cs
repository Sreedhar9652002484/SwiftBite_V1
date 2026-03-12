using MediatR;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.MarkAllRead;

public class MarkAllReadCommandHandler
    : IRequestHandler<MarkAllReadCommand, bool>
{
    private readonly INotificationRepository _repo;

    public MarkAllReadCommandHandler(
        INotificationRepository repo) => _repo = repo;

    public async Task<bool> Handle(
        MarkAllReadCommand cmd, CancellationToken ct)
    {
        await _repo.MarkAllReadAsync(cmd.UserId, ct);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}