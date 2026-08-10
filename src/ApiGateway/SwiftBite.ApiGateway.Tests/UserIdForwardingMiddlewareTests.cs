using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SwiftBite.ApiGateway.Middleware;

namespace SwiftBite.ApiGateway.Tests;

public class UserIdForwardingMiddlewareTests
{
    private static HttpContext BuildContext(ClaimsPrincipal user)
    {
        var context = new DefaultHttpContext();
        context.User = user;
        return context;
    }

    private static ClaimsPrincipal AuthenticatedUser(string sub, string? role = null)
    {
        var claims = new List<Claim> { new("sub", sub) };
        if (role is not null)
            claims.Add(new Claim("role", role));

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Authenticated_user_gets_id_and_role_forwarded()
    {
        var context = BuildContext(AuthenticatedUser("user-42", "Customer"));
        var middleware = new UserIdForwardingMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("user-42", context.Request.Headers["X-User-Id"]);
        Assert.Equal("Customer", context.Request.Headers["X-User-Role"]);
    }

    [Fact]
    public async Task Unauthenticated_user_does_not_get_headers_forwarded()
    {
        // Anonymous identity (no authentication type) => IsAuthenticated is false.
        var context = BuildContext(new ClaimsPrincipal(new ClaimsIdentity()));
        var middleware = new UserIdForwardingMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.True(string.IsNullOrEmpty(context.Request.Headers["X-User-Id"]));
        Assert.True(string.IsNullOrEmpty(context.Request.Headers["X-User-Role"]));
    }

    [Fact]
    public async Task Client_supplied_spoofed_header_is_overwritten_for_authenticated_user()
    {
        // A malicious client sending its own X-User-Id must not survive once
        // the real identity from the validated JWT is forwarded.
        var context = BuildContext(AuthenticatedUser("real-user"));
        context.Request.Headers["X-User-Id"] = "spoofed-admin";
        var middleware = new UserIdForwardingMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("real-user", context.Request.Headers["X-User-Id"]);
    }

    [Fact]
    public async Task Next_delegate_is_always_invoked()
    {
        var called = false;
        var context = BuildContext(new ClaimsPrincipal(new ClaimsIdentity()));
        var middleware = new UserIdForwardingMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }
}
