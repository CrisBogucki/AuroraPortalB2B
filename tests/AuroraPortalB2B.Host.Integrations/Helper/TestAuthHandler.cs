using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuroraPortalB2B.Host.Integrations.Helper;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValue))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        var header = headerValue.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme."));
        }

        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "test-user"));
        identity.AddClaim(new Claim(ClaimTypes.Name, "test-user"));
        identity.AddClaim(new Claim("sub", "test-user"));
        identity.AddClaim(new Claim("roles", "admin"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
        identity.AddClaim(new Claim("permissions", "partners.read"));
        identity.AddClaim(new Claim("permissions", "partners.write"));
        identity.AddClaim(new Claim("permissions", "partnerUsers.read"));
        identity.AddClaim(new Claim("permissions", "partnerUsers.write"));
        identity.AddClaim(new Claim("tenant_id", "tenant-1"));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
