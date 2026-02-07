using System.Security.Claims;
using AuroraPortalB2B.Core.Mediator.Authorization;
using AuroraPortalB2B.Host.Configuration.Authentication;
using FluentAssertions;

namespace AuroraPortalB2B.Host.Tests.Unit.Authentication;

public sealed class KeycloakRoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsync_ShouldReturnSamePrincipal_WhenNotAuthenticated()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        var transformer = new KeycloakRoleClaimsTransformation();

        var result = await transformer.TransformAsync(principal);

        result.Should().BeSameAs(principal);
        result.Claims.Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_ShouldIgnoreMissingRealmAccess()
    {
        var identity = new ClaimsIdentity("Bearer");
        identity.AddClaim(new Claim("sub", "user-1"));
        var principal = new ClaimsPrincipal(identity);
        var transformer = new KeycloakRoleClaimsTransformation();

        var result = await transformer.TransformAsync(principal);

        result.Should().BeSameAs(principal);
        result.Claims.Should().ContainSingle(claim => claim.Type == "sub");
    }

    [Fact]
    public async Task TransformAsync_ShouldAddRolesAndAllPermissions_ForAdminRole()
    {
        var identity = new ClaimsIdentity("Bearer");
        identity.AddClaim(new Claim("realm_access", "{\"roles\":[\"admin\",\"partners.read\"]}"));
        var principal = new ClaimsPrincipal(identity);
        var transformer = new KeycloakRoleClaimsTransformation();

        var result = await transformer.TransformAsync(principal);

        result.Claims.Should().Contain(claim => claim.Type == "roles" && claim.Value == "admin");
        result.Claims.Should().Contain(claim => claim.Type == "roles" && claim.Value == "partners.read");

        foreach (var permission in PermissionNames.All)
        {
            result.Claims.Should().Contain(claim => claim.Type == PermissionNames.ClaimType && claim.Value == permission);
        }
    }

    [Fact]
    public async Task TransformAsync_ShouldAddPermissionClaims_ForMatchingRoles()
    {
        var identity = new ClaimsIdentity("Bearer");
        identity.AddClaim(new Claim("realm_access", "{\"roles\":[\"partners.read\",\"unknown\"]}"));
        var principal = new ClaimsPrincipal(identity);
        var transformer = new KeycloakRoleClaimsTransformation();

        var result = await transformer.TransformAsync(principal);

        result.Claims.Should().Contain(claim => claim.Type == PermissionNames.ClaimType && claim.Value == PermissionNames.PartnersRead);
        result.Claims.Should().NotContain(claim => claim.Type == PermissionNames.ClaimType && claim.Value == "unknown");
    }

    [Fact]
    public async Task TransformAsync_ShouldIgnoreInvalidJson()
    {
        var identity = new ClaimsIdentity("Bearer");
        identity.AddClaim(new Claim("realm_access", "not-json"));
        var principal = new ClaimsPrincipal(identity);
        var transformer = new KeycloakRoleClaimsTransformation();

        var result = await transformer.TransformAsync(principal);

        result.Claims.Should().ContainSingle(claim => claim.Type == "realm_access");
        result.Claims.Should().NotContain(claim => claim.Type == "roles");
    }
}
