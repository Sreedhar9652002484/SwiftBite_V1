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
}