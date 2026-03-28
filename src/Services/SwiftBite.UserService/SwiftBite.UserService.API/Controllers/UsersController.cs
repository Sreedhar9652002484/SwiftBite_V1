using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Users.Commands.CreateUser;
using SwiftBite.UserService.Application.Users.Commands.UpdateProfile;
using SwiftBite.UserService.Application.Users.Queries.GetProfile;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS
using System.Security.Claims;

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get current user profile.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var authUserId =
            User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (string.IsNullOrEmpty(authUserId))
            throw new UnauthorizedException(
                "User ID not found in claims.");

        var firstName =
            User.FindFirst("given_name")?.Value
            ?? User.FindFirst("name")?.Value
            ?? "User";

        var lastName =
            User.FindFirst("family_name")?.Value ?? "";

        var email =
            User.FindFirst("email")?.Value ?? "";

        var dobString =
            User.FindFirst("birthdate")?.Value
            ?? User.FindFirst(ClaimTypes.DateOfBirth)?.Value
            ?? User.FindFirst("dob")?.Value;

        DateTime dateOfBirth;
        if (!DateTime.TryParse(dobString, out dateOfBirth))
        {
            dateOfBirth = DateTime.MinValue;
        }

        var result = await _mediator.Send(
            new GetProfileQuery(
                authUserId, firstName,
                lastName, email, dateOfBirth), ct);

        // ✅ CHANGE: Wrap response with ApiResponse
        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Profile retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Create user profile (called right after registration).
    /// </summary>
    [HttpPost("profile")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 409)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new CreateUserCommand(
                request.AuthUserId,
                request.FirstName,
                request.LastName,
                request.Email,
                request.DateOfBirth), ct);

        // ✅ Middleware automatically catches InvalidOperationException
        // and converts to 409 Conflict

        return CreatedAtAction(
            nameof(GetProfile),
            ApiResponse<object>.SuccessResponse(
                result,
                "User profile created successfully.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Update user profile.
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new UpdateProfileCommand(
                authUserId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.ProfilePictureUrl), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Profile updated successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record CreateUserRequest(
    string AuthUserId,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? ProfilePictureUrl = null);