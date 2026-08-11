using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using SwiftBite.AuthServer.Data;
using SwiftBite.AuthServer.Models;
using SwiftBite.AuthServer.Services;
using SwiftBite.Shared.Exceptions.Exceptions;
using SwiftBite.Shared.Exceptions.Models;

namespace SwiftBite.AuthServer.Controllers;

[Route("api/partner-applications")]
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class PartnerApplicationsController : ControllerBase
{
    private static readonly string[] AllowedRoles = ["RestaurantAdmin", "DeliveryPartner"];

    private readonly AuthDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRestaurantProvisioningService _restaurantProvisioning;

    public PartnerApplicationsController(
        AuthDbContext db,
        UserManager<ApplicationUser> userManager,
        IRestaurantProvisioningService restaurantProvisioning)
    {
        _db = db;
        _userManager = userManager;
        _restaurantProvisioning = restaurantProvisioning;
    }

    // ── POST /api/partner-applications ───────────────────────
    // Any authenticated user applies to become a restaurant admin or delivery partner.
    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] PartnerApplicationRequest request, CancellationToken ct)
    {
        if (!AllowedRoles.Contains(request.RequestedRole))
            throw new ValidationException($"RequestedRole must be one of: {string.Join(", ", AllowedRoles)}");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ValidationException("Phone is required.");

        if (request.RequestedRole == "RestaurantAdmin" && string.IsNullOrWhiteSpace(request.BusinessName))
            throw new ValidationException("BusinessName is required for restaurant applications.");

        if (request.RequestedRole == "RestaurantAdmin" && string.IsNullOrWhiteSpace(request.City))
            throw new ValidationException("City is required for restaurant applications.");

        if (request.RequestedRole == "DeliveryPartner" && string.IsNullOrWhiteSpace(request.VehicleType))
            throw new ValidationException("VehicleType is required for delivery partner applications.");

        var userId = GetUserId();

        var alreadyPending = await _db.PartnerApplications
            .AnyAsync(a => a.UserId == userId && a.Status == "Pending", ct);
        if (alreadyPending)
            throw new ValidationException("You already have a pending application.");

        var application = new PartnerApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RequestedRole = request.RequestedRole,
            BusinessName = request.BusinessName,
            City = request.City,
            VehicleType = request.VehicleType,
            LicenseNumber = request.LicenseNumber,
            Phone = request.Phone,
            Note = request.Note
        };

        _db.PartnerApplications.Add(application);
        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            new { id = application.Id },
            "Application submitted. An admin will review it soon.",
            HttpContext.TraceIdentifier));
    }

    // ── GET /api/partner-applications?status=Pending ─────────
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string status = "Pending", CancellationToken ct = default)
    {
        RequireAdmin();

        var applications = await _db.PartnerApplications
            .Where(a => a.Status == status)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);

        var userIds = applications.Select(a => a.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var result = applications.Select(a => new
        {
            a.Id,
            a.RequestedRole,
            a.Status,
            a.BusinessName,
            a.City,
            a.VehicleType,
            a.LicenseNumber,
            a.Phone,
            a.Note,
            a.CreatedAt,
            applicantName = users.TryGetValue(a.UserId, out var u) ? $"{u.FirstName} {u.LastName}" : "Unknown",
            applicantEmail = users.TryGetValue(a.UserId, out var u2) ? u2.Email : null
        });

        return Ok(ApiResponse<object>.SuccessResponse(result, "Applications retrieved successfully.", HttpContext.TraceIdentifier));
    }

    // ── POST /api/partner-applications/{id}/approve ──────────
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var adminId = RequireAdmin();

        var application = await _db.PartnerApplications.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new ResourceNotFoundException("PartnerApplication", id.ToString());

        if (application.Status != "Pending")
            throw new ValidationException("Only pending applications can be approved.");

        var user = await _userManager.FindByIdAsync(application.UserId)
            ?? throw new ResourceNotFoundException("User", application.UserId);

        if (application.RequestedRole == "RestaurantAdmin" && user.RestaurantId is null)
        {
            user.RestaurantId = await _restaurantProvisioning.CreateRestaurantAsync(user, application, ct);
            await _userManager.UpdateAsync(user);
        }

        if (!await _userManager.IsInRoleAsync(user, application.RequestedRole))
            await _userManager.AddToRoleAsync(user, application.RequestedRole);

        application.Status = "Approved";
        application.ReviewedAt = DateTime.UtcNow;
        application.ReviewedByUserId = adminId;

        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.SuccessResponse($"Approved. User is now {application.RequestedRole}.", HttpContext.TraceIdentifier));
    }

    // ── POST /api/partner-applications/{id}/reject ───────────
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectApplicationRequest? request, CancellationToken ct)
    {
        var adminId = RequireAdmin();

        var application = await _db.PartnerApplications.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new ResourceNotFoundException("PartnerApplication", id.ToString());

        if (application.Status != "Pending")
            throw new ValidationException("Only pending applications can be rejected.");

        application.Status = "Rejected";
        application.ReviewedAt = DateTime.UtcNow;
        application.ReviewedByUserId = adminId;
        application.ReviewNote = request?.Note;

        await _db.SaveChangesAsync(ct);

        return Ok(ApiResponse.SuccessResponse("Application rejected.", HttpContext.TraceIdentifier));
    }

    private string GetUserId()
        => User.GetClaim(OpenIddictConstants.Claims.Subject)
           ?? throw new UnauthorizedException("User ID not found in token.");

    // Manual claim check instead of [Authorize(Roles=...)] since OpenIddict's validation
    // handler preserves the short "role" claim type, not the long ClaimTypes.Role used by
    // ASP.NET Core's built-in role authorization by default.
    private string RequireAdmin()
    {
        var userId = GetUserId();
        if (!User.HasClaim(c => c.Type == OpenIddictConstants.Claims.Role && c.Value == "Admin"))
            throw new ForbiddenException("Admin role required.");
        return userId;
    }
}

public record PartnerApplicationRequest(
    string RequestedRole,
    string Phone,
    string? BusinessName = null,
    string? City = null,
    string? VehicleType = null,
    string? LicenseNumber = null,
    string? Note = null);

public record RejectApplicationRequest(string? Note);
