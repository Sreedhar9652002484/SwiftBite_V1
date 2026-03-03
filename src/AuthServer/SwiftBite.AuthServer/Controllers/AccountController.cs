using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.AuthServer.Models;
using System.Security.Claims;

namespace SwiftBite.AuthServer.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _angularBaseUrl;

    public AccountController( SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,IConfiguration config)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _angularBaseUrl = config["AuthServer:AngularBaseUrl"]!;
    }

    // ── GET /Account/Login ───────────────────────────────────
    [HttpGet("~/Account/Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST /Account/Login ──────────────────────────────────
    [HttpPost("~/Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login( string email,string password,string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var result = await _signInManager.PasswordSignInAsync(
            email, password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return Redirect("/");
        }

        ViewData["Error"] = result.IsLockedOut
            ? "Account is locked. Try again later."
            : "Invalid email or password.";

        return View();
    }

    // ── GET /Account/ExternalLogin ───────────────────────────
    // Used by Login.cshtml Google button
    [HttpGet("~/Account/ExternalLogin")]
    public IActionResult ExternalLogin(string provider, string? returnUrl)
    {
        var redirectUrl = Url.Action(
            nameof(ExternalLoginCallback), "Account",
            new { returnUrl });

        var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);

        return Challenge(properties, provider);
    }

    // ── GET /Account/ExternalLoginCallback ───────────────────
    [HttpGet("~/Account/ExternalLoginCallback")]
    public async Task<IActionResult> ExternalLoginCallback(
        string? returnUrl)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Redirect(
                $"{_angularBaseUrl}/auth/login?error=google_failed");

        // Try sign in with existing Google account
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false);

        if (!result.Succeeded)
        {
            // First time Google login — create user
            var email = info.Principal
                .FindFirstValue(ClaimTypes.Email)!;
            var firstName = info.Principal
                .FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = info.Principal
                .FindFirstValue(ClaimTypes.Surname) ?? "";

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager
                    .CreateAsync(user);

                if (!createResult.Succeeded)
                    return Redirect(
                        $"{_angularBaseUrl}/auth/login?error=create_failed");

                await _userManager.AddToRoleAsync(user, "Customer");
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        // ✅ returnUrl = full /connect/authorize?... URL
        // Redirecting here completes OAuth2 flow
        // Angular /auth/callback gets the auth code → exchanges for JWT
        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return Redirect("/");
    }

    // ── GET /Account/SignOut ─────────────────────────────────
    [HttpGet("~/Account/SignOut")]
    public async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        return Ok(); // Angular handles redirect
    }

    // ── GET /connect/logout ──────────────────────────────────
    [HttpGet("~/connect/logout")]
    public async Task<IActionResult> OidcLogout(
        string? id_token_hint = null,
        string? post_logout_redirect_uri = null)
    {
        await _signInManager.SignOutAsync();

        var redirect = !string.IsNullOrEmpty(post_logout_redirect_uri)
            ? post_logout_redirect_uri
            : $"{_angularBaseUrl}/auth/login";

        return Redirect(redirect);
    }

    // ── POST /connect/logout ─────────────────────────────────
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> OidcLogoutPost()
    {
        await _signInManager.SignOutAsync();
        return Redirect($"{_angularBaseUrl}/auth/login");
    }
}