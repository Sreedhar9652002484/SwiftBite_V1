using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;
using SwiftBite.UserService.Application.Preferences.Queries.GetPreferences;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users/preferences")]
[Authorize]
public class PreferencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreferencesController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/users/preferences ─────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetPreferences(
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new GetPreferencesQuery(authUserId), ct);

            if (result is null)
                return Ok(new { message = "No preferences set yet." });

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT api/users/preferences ─────────────────────────
    [HttpPut]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new UpdatePreferencesCommand(
                    authUserId,
                    request.DietaryPreference,
                    request.EmailNotifications,
                    request.PushNotifications,
                    request.SmsNotifications,
                    request.PreferredCuisines,
                    request.AllergiesInfo), ct);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Model ─────────────────────────────────────────
public record UpdatePreferencesRequest(
    DietaryPreference DietaryPreference,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    string PreferredCuisines,
    string AllergiesInfo);