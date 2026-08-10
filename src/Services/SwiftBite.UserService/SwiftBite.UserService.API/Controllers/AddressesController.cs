using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Addresses.Commands.AddAddress;
using SwiftBite.UserService.Application.Addresses.Commands.DeleteAddress;
using SwiftBite.UserService.Application.Addresses.Commands.SetDefaultAddress;
using SwiftBite.UserService.Application.Addresses.Queries.GetAddresses;
using SwiftBite.UserService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddressesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get all addresses for current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetAddresses(CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetAddressesQuery(authUserId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Addresses retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Add new address for current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddAddressRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new AddAddressCommand(
                authUserId,
                request.Label,
                request.FullAddress,
                request.Street,
                request.City,
                request.State,
                request.PinCode,
                request.Latitude,
                request.Longitude,
                request.Type,
                request.Landmark), ct);

        return CreatedAtAction(
            nameof(GetAddresses),
            ApiResponse<object>.SuccessResponse(
                result,
                "Address added successfully.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Delete address by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> DeleteAddress(
        Guid id,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new DeleteAddressCommand(authUserId, id), ct);

        return NoContent();
    }

    /// <summary>
    /// Set address as default.
    /// </summary>
    [HttpPut("{id:guid}/default")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> SetDefault(
        Guid id,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (authUserId is null)
            throw new UnauthorizedException(
                "User ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new SetDefaultAddressCommand(authUserId, id), ct);

        return Ok(ApiResponse.SuccessResponse(
            "Default address updated successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record AddAddressRequest(
    string Label,
    string FullAddress,
    string Street,
    string City,
    string State,
    string PinCode,
    double Latitude,
    double Longitude,
    AddressType Type,
    string? Landmark = null);