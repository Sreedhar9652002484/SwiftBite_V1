using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.UserService.Application.Addresses.Commands.AddAddress;
using SwiftBite.UserService.Application.Addresses.Commands.DeleteAddress;
using SwiftBite.UserService.Application.Addresses.Commands.SetDefaultAddress;
using SwiftBite.UserService.Application.Addresses.Queries.GetAddresses;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.API.Controllers;

[ApiController]
[Route("api/users/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddressesController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/users/addresses ───────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAddresses(
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new GetAddressesQuery(authUserId), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST api/users/addresses ──────────────────────────
    [HttpPost]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddAddressRequest request,
        CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
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
                nameof(GetAddresses), result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── DELETE api/users/addresses/{id} ───────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress(
        Guid id, CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
            await _mediator.Send(
                new DeleteAddressCommand(authUserId, id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    // ── PUT api/users/addresses/{id}/default ──────────────
    [HttpPut("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(
        Guid id, CancellationToken ct)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null) return Unauthorized();

        try
        {
            await _mediator.Send(
                new SetDefaultAddressCommand(authUserId, id), ct);
            return Ok(new { message = "Default address updated." });
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