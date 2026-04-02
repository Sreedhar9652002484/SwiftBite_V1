using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SwiftBite.AuthServer.Models;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SwiftBite.AuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TokenController> _logger;  

        public TokenController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<TokenController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
       [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("OpenIddict request not found");

            // ── Authorization Code + Refresh Token ──────────────────
            if (request.IsAuthorizationCodeGrantType() ||
                request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var userId = result.Principal?.GetClaim(Claims.Subject);
                var user = await _userManager.FindByIdAsync(userId!);

                if (user is null)
                    return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                return SignIn(
                    await BuildClaimsPrincipal(user),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // ── Password Flow (Direct login from Angular form) ───────
            if (request.IsPasswordGrantType())
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Username!);

                if (user is null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Invalid email or password."
                    });
                    return Forbid(properties,
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Validate password
                var validPassword = await _userManager.CheckPasswordAsync(user, request.Password!);
                if (!validPassword)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Invalid email or password."
                    });
                    return Forbid(properties,
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Check if account is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Account is locked out. Try again later."
                    });
                    return Forbid(properties,
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Reset lockout count on success
                await _userManager.ResetAccessFailedCountAsync(user);

                return SignIn(
                    await BuildClaimsPrincipal(user),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "Unsupported grant type" });
        }

        // ── Shared: Build ClaimsPrincipal for any grant type ────────
        private async Task<ClaimsPrincipal> BuildClaimsPrincipal(ApplicationUser user)
        {
            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            identity.AddClaim(Claims.Subject, user.Id);
            identity.AddClaim(Claims.Email, user.Email!);
            identity.AddClaim(Claims.GivenName, user.FirstName);
            identity.AddClaim(Claims.FamilyName, user.LastName);
            identity.AddClaim("firstName", user.FirstName);
            identity.AddClaim("lastName", user.LastName);

            identity.AddClaim(Claims.Audience, "swiftbite-gateway");
            identity.AddClaim(Claims.Audience, "swiftbite-userservice");
            identity.AddClaim(Claims.Audience, "swiftbite-restaurantservice");
            identity.AddClaim(Claims.Audience, "swiftbite-orderservice");
            identity.AddClaim(Claims.Audience, "swiftbite-paymentservice");
            identity.AddClaim(Claims.Audience, "swiftbite-notificationservice");
            identity.AddClaim(Claims.Audience, "swiftbite-deliveryservice");

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                identity.AddClaim(Claims.Role, role);
            // ← ADD THIS BLOCK (only 3 lines)
            if (user.RestaurantId.HasValue)
                identity.AddClaim("restaurantId", user.RestaurantId.Value.ToString());

            identity.SetDestinations(GetDestinations);

            return new ClaimsPrincipal(identity);
        }
        // ── UserInfo endpoint ────────────────────────────────────
        [HttpGet("~/connect/userinfo")]
        [HttpPost("~/connect/userinfo")]
        [Authorize(AuthenticationSchemes =OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Userinfo()
        {
            // ✅ Read "sub" claim directly — not GetUserAsync()
            var subject = User.GetClaim(OpenIddictConstants.Claims.Subject);

            Console.WriteLine($"Subject from token: {subject}");

            if (string.IsNullOrEmpty(subject))
                return Challenge(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // ✅ Find by ID using subject
            var user = await _userManager.FindByIdAsync(subject);

            if (user is null)
                return Challenge(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                sub = user.Id,
                email = user.Email,
                given_name = user.FirstName,
                family_name = user.LastName,
                name = $"{user.FirstName} {user.LastName}",
                firstName = user.FirstName,
                lastName = user.LastName,
                role = roles.Count == 1
                                ? (object)roles[0]
                                : roles.ToArray(),
                restaurantId = user.RestaurantId?.ToString() 
            });
        }

        // ── Which claims go to access_token vs id_token ──────────
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            return claim.Type switch
            {
                Claims.Name or Claims.Subject
                    => [Destinations.AccessToken, Destinations.IdentityToken],

                Claims.Email or Claims.GivenName or Claims.FamilyName
                    => [Destinations.AccessToken, Destinations.IdentityToken],

                Claims.Role
                    => [Destinations.AccessToken, Destinations.IdentityToken],
                "restaurantId"                              // ← ADD THIS CASE
                    => [Destinations.AccessToken, Destinations.IdentityToken],


                _ => [Destinations.AccessToken]
            };
        }

    }
}
