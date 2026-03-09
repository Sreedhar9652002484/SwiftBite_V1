using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Users.Commands.CreateUser;
using SwiftBite.UserService.Application.Users.Commands.UpdateProfile;
using SwiftBite.UserService.Application.Users.Queries.GetProfile;

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/users/profile ─────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized();

        try
        {
            var result = await _mediator.Send(new GetProfileQuery(authUserId), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST api/users/profile ────────────────────────────
    [HttpPost("profile")]
    [AllowAnonymous] // ✅ Called right after registration
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new CreateUserCommand(
                    request.AuthUserId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.DateOfBirth), ct);

            return CreatedAtAction( nameof(GetProfile), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ── PUT api/users/profile ─────────────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
            return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new UpdateProfileCommand(
                    authUserId,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    request.ProfilePictureUrl), ct);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Helper — reads X-User-Id from Gateway ─────────────
    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Models ────────────────────────────────────────
public record CreateUserRequest(
    string AuthUserId,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePictureUrl);