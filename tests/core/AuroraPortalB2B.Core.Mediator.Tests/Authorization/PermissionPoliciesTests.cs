using AuroraPortalB2B.Core.Mediator.Authorization;
using FluentAssertions;

namespace AuroraPortalB2B.Core.Mediator.Tests.Authorization;

public sealed class PermissionPoliciesTests
{
    [Fact]
    public void All_ShouldContainAllPolicies()
    {
        PermissionPolicies.All.Should().Contain(new[]
        {
            PermissionPolicies.PartnersRead,
            PermissionPolicies.PartnersWrite,
            PermissionPolicies.PartnerUsersRead,
            PermissionPolicies.PartnerUsersWrite
        });
    }

    [Fact]
    public void All_ShouldHaveUniqueNames()
    {
        PermissionPolicies.All
            .Select(policy => policy.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count()
            .Should()
            .Be(PermissionPolicies.All.Length);
    }
}
