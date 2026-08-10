
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.AuthServer.Models;

namespace SwiftBite.AuthServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // ── POST /api/auth/register ──────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register( [FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest(new
            {
                errors = new[] { "Email is already registered." }
            });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });

        await _userManager.AddToRoleAsync(user, "Customer");

        return Ok(new
        {
            message = "Registration successful. Please login."
        });
    }

    // ── GET /api/auth/me ─────────────────────────────────────
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles = await _userManager.GetRolesAsync(user)
        });
    }
}