using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;  // ✅ Add this line!


namespace SwiftBite.NotificationService.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        ILogger<NotificationHub> logger)
        => _logger = logger;

    // ✅ Client connects — join their personal group
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId is not null)
        {
            // Each user has their own SignalR group!
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user_{userId}");

            _logger.LogInformation(
                "🔌 SignalR connected | " +
                "User: {UserId} | ConnectionId: {Id}",
                userId, Context.ConnectionId);
        }

        if (IsDeliveryPartner())
        {
            // Delivery jobs aren't assigned to a specific partner until
            // accepted, so new-job pushes go to a shared broadcast group.
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                "delivery_partners");
        }

        await base.OnConnectedAsync();
    }

    // ✅ Client disconnects
    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        var userId = GetUserId();
        if (userId is not null)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"user_{userId}");

            _logger.LogInformation(
                "🔌 SignalR disconnected | User: {UserId}",
                userId);
        }

        if (IsDeliveryPartner())
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                "delivery_partners");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ✅ Client can ping to test connection
    public async Task Ping()
        => await Clients.Caller.SendAsync(
            "Pong", DateTime.UtcNow);

    private string? GetUserId()
      => Context.User?.FindFirst("sub")?.Value
      ?? Context.User?.FindFirst(
          System.Security.Claims.ClaimTypes.NameIdentifier)
          ?.Value;

    // OpenIddict issues the short "role" claim, not ClaimTypes.Role.
    private bool IsDeliveryPartner()
      => Context.User?.HasClaim(c =>
          c.Type == "role" && c.Value == "DeliveryPartner") ?? false;

    // Add this method to NotificationHub
    public async Task JoinOrderTracking(string orderId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"order_{orderId}");

        _logger.LogInformation(
            "🗺️ Joined order tracking | OrderId: {OrderId}",
            orderId);
    }

    public async Task LeaveOrderTracking(string orderId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"order_{orderId}");
    }
}