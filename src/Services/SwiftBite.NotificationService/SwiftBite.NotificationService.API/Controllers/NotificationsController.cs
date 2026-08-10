using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.NotificationService.Application.Notifications.Commands.MarkAllRead;
using SwiftBite.NotificationService.Application.Notifications.Commands.RegisterDevice;
using SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetNotifications;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetUnreadCount;
using SwiftBite.NotificationService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get paginated notifications for current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (userId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetNotificationsQuery(
                userId, page, pageSize), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Notifications retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Get unread notification count - for bell icon badge.
    /// Called frequently by frontend.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetUnreadCount(
        CancellationToken ct = default)
    {
        var userId = GetUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (userId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var count = await _mediator.Send(
            new GetUnreadCountQuery(userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { unreadCount = count },
            "Unread count retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPut("mark-all-read")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> MarkAllRead(
        CancellationToken ct = default)
    {
        var userId = GetUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (userId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new MarkAllReadCommand(userId), ct);

        return Ok(ApiResponse.SuccessResponse(
            "All notifications marked as read ✅",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Register Firebase push notification token for device.
    /// </summary>
    [HttpPost("register-device")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> RegisterDevice(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (userId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        if (string.IsNullOrWhiteSpace(request.DeviceToken))
            throw new ValidationException(
                "Device token cannot be empty.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new RegisterDeviceCommand(
                userId,
                request.DeviceToken,
                request.DeviceType), ct);

        return Ok(ApiResponse.SuccessResponse(
            "Device registered successfully 📱",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Manual send notification - for admin/testing purposes.
    /// </summary>
    [HttpPost("send")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Send(
        [FromBody] SendNotificationRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new ValidationException(
                "User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException(
                "Notification title cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ValidationException(
                "Notification message cannot be empty.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new SendNotificationCommand(
                request.UserId,
                request.Title,
                request.Message,
                request.Type,
                NotificationChannel.SignalR,
                request.ReferenceId), ct);

        return Ok(ApiResponse.SuccessResponse(
            "Notification sent! 🔔",
            HttpContext.TraceIdentifier));
    }

    private string? GetUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record RegisterDeviceRequest(
    string DeviceToken,
    string DeviceType);

public record SendNotificationRequest(
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? ReferenceId = null);