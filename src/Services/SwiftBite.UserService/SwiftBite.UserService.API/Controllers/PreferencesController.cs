using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;
using SwiftBite.UserService.Application.Preferences.Queries.GetPreferences;
using SwiftBite.UserService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users/preferences")]
[Authorize]
public class PreferencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreferencesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get user preferences.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetPreferencesQuery(authUserId), ct);

        if (result is null)
            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "No preferences set yet.",
                HttpContext.TraceIdentifier));

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Preferences retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Update user preferences.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new UpdatePreferencesCommand(
                authUserId,
                request.DietaryPreference,
                request.EmailNotifications,
                request.PushNotifications,
                request.SmsNotifications,
                request.PreferredCuisines,
                request.AllergiesInfo), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Preferences updated successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record UpdatePreferencesRequest(
    DietaryPreference DietaryPreference,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    string PreferredCuisines,
    string AllergiesInfo);