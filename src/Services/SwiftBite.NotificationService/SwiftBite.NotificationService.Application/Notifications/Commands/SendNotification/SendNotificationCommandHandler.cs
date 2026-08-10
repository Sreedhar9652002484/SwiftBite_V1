using MediatR;
using SwiftBite.NotificationService.Domain.Entities;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler
    : IRequestHandler<SendNotificationCommand, bool>
{
    private readonly INotificationRepository _repo;
    private readonly INotificationSender _sender;

    public SendNotificationCommandHandler(
        INotificationRepository repo,
        INotificationSender sender)
    {
        _repo = repo;
        _sender = sender;
    }

    public async Task<bool> Handle(
        SendNotificationCommand cmd,
        CancellationToken ct)
    {
        // ✅ Save to DB
        var notification = Notification.Create(
            cmd.UserId, cmd.Title, cmd.Message,
            cmd.Type, cmd.Channel,
            cmd.ReferenceId, cmd.ImageUrl);

        await _repo.AddAsync(notification, ct);
        await _repo.SaveChangesAsync(ct);

        // ✅ Send real-time via SignalR
        await _sender.SendToUserAsync(
            cmd.UserId, cmd.Title,
            cmd.Message, cmd.Data, ct);

        return true;
    }
}