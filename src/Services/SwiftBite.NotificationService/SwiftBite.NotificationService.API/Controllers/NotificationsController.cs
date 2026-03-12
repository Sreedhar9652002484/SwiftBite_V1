using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.NotificationService.Application.Notifications.Commands.MarkAllRead;
using SwiftBite.NotificationService.Application.Notifications.Commands.RegisterDevice;
using SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetNotifications;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetUnreadCount;
using SwiftBite.NotificationService.Domain.Enums;

namespace SwiftBite.NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
        => _mediator = mediator;

    // ── GET /api/notifications ────────────────────────────
    // Get paginated notifications for current user
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetNotificationsQuery(
                userId, page, pageSize), ct);

        return Ok(result);
    }

    // ── GET /api/notifications/unread-count ───────────────
    // Bell icon badge count — called frequently!
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var count = await _mediator.Send(
            new GetUnreadCountQuery(userId), ct);

        return Ok(new { unreadCount = count });
    }

    // ── PUT /api/notifications/mark-all-read ──────────────
    // Mark all notifications as read
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(
            new MarkAllReadCommand(userId), ct);

        return Ok(new
        {
            message = "All notifications marked as read ✅"
        });
    }

    // ── POST /api/notifications/register-device ───────────
    // Register Firebase push token
    [HttpPost("register-device")]
    public async Task<IActionResult> RegisterDevice(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await _mediator.Send(
            new RegisterDeviceCommand(
                userId,
                request.DeviceToken,
                request.DeviceType), ct);

        return Ok(new
        {
            message = "Device registered successfully 📱"
        });
    }

    // ── POST /api/notifications/send ──────────────────────
    // Manual send — for admin/testing purposes
    [HttpPost("send")]
    public async Task<IActionResult> Send(
        [FromBody] SendNotificationRequest request,
        CancellationToken ct = default)
    {
        await _mediator.Send(
            new SendNotificationCommand(
                request.UserId,
                request.Title,
                request.Message,
                request.Type,
                NotificationChannel.SignalR,
                request.ReferenceId), ct);

        return Ok(new
        {
            message = "Notification sent! 🔔"
        });
    }

    private string? GetUserId()
        => Request.Headers["X-User-Id"]
            .FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Models ────────────────────────────────────────
public record RegisterDeviceRequest(
    string DeviceToken,
    string DeviceType);

public record SendNotificationRequest(
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? ReferenceId = null);