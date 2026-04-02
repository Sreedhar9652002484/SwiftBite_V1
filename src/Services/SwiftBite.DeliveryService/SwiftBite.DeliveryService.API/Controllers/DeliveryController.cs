using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;
using SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetActiveJob;
using SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetActiveJobsQuery;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.RegisterPartner;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.UpdateAvailability;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetEarnings;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetPartnerProfile;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS


namespace SwiftBite.DeliveryService.API.Controllers;

[ApiController]
[Route("api/delivery")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryController(IMediator mediator)
        => _mediator = mediator;

    // ── POST api/delivery/register ────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPartnerRequest request,
        CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(new RegisterPartnerCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.VehicleType,
            request.VehicleNumber), ct);

        return CreatedAtAction(nameof(GetProfile),
            ApiResponse<object>.SuccessResponse(
                result,
                "Delivery partner registered successfully.",
                HttpContext.TraceIdentifier));
    }

    // ── GET api/delivery/profile ──────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new GetPartnerProfileQuery(userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Profile retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── PUT api/delivery/availability ─────────────────────
    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability(
        [FromBody] UpdateAvailabilityRequest request,
        CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new UpdateAvailabilityCommand(userId, request.IsAvailable), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Availability updated successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── GET api/delivery/jobs ─────────────────────────────
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new GetPartnerJobsQuery(userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Jobs retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── GET api/delivery/jobs/active ──────────────────────
    [HttpGet("jobs/active")]
    public async Task<IActionResult> GetActiveJobs(CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new GetActiveJobsQuery(userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Active jobs retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── PUT api/delivery/jobs/{id}/accept ─────────────────
    [HttpPut("jobs/{id:guid}/accept")]
    public async Task<IActionResult> AcceptJob(Guid id, CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new AcceptJobCommand(id, userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Job accepted successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── PUT api/delivery/jobs/{id}/status ─────────────────
    [HttpPut("jobs/{id:guid}/status")]
    public async Task<IActionResult> UpdateJobStatus(
        Guid id,
        [FromBody] UpdateJobStatusRequest request,
        CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new UpdateJobStatusCommand(id, userId, request.Status), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Job status updated successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── GET api/delivery/earnings ─────────────────────────
    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings(CancellationToken ct)
    {
        var userId = GetAuthUserId();

        if (userId is null)
            throw new UnauthorizedException("User ID not found.");

        var result = await _mediator.Send(
            new GetEarningsQuery(userId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Earnings retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Models ────────────────────────────────────────
public record RegisterPartnerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    VehicleType VehicleType,
    string VehicleNumber);

public record UpdateAvailabilityRequest(bool IsAvailable);

public record UpdateJobStatusRequest(JobStatus Status);