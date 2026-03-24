using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SwiftBite.AuthServer.Models;
using System.Security.Claims;

namespace SwiftBite.AuthServer.Controllers;

public class AuthorizeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthorizeController( UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(
                "OpenIddict request not found");

        // Check if user has Identity cookie
        var result = await HttpContext.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            // ✅ Not logged in
            // Build the full current URL to pass as returnUrl
            // So after login/Google → comes back here to complete flow
            var returnUri = Request.PathBase + Request.Path +
                QueryString.Create(
                    Request.HasFormContentType
                        ? Request.Form.ToList()
                        : Request.Query.ToList());

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = returnUri
                });
        }

        // ✅ User is logged in — build their claims
        var user = await _userManager.FindByIdAsync(
            result.Principal!.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        if (user is null)
            return Challenge(IdentityConstants.ApplicationScheme);

        // Build claims identity
        var identity = new ClaimsIdentity(
            authenticationType:
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.AddClaim(
            OpenIddictConstants.Claims.Subject, user.Id);
        identity.AddClaim(
            OpenIddictConstants.Claims.Email, user.Email!);
        identity.AddClaim(
            OpenIddictConstants.Claims.GivenName, user.FirstName);
        identity.AddClaim(
            OpenIddictConstants.Claims.FamilyName, user.LastName);
        identity.AddClaim("firstName", user.FirstName);
        identity.AddClaim("lastName", user.LastName);
        if (user.RestaurantId.HasValue)                    
            identity.AddClaim("restaurantId", user.RestaurantId.Value.ToString());

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            identity.AddClaim(OpenIddictConstants.Claims.Role, role);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());

        // Set destinations — which claims go in which token
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                OpenIddictConstants.Claims.Email or
                OpenIddictConstants.Claims.GivenName or
                OpenIddictConstants.Claims.FamilyName or
                "firstName" or "lastName" or "restaurantId" => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },
                _ => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken
                }
            });
        }

        // ✅ Issue authorization code → Angular exchanges for JWT
        return SignIn(principal,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}