using AuroraPortalB2B.Core.Mediator.Authorization;
using FluentAssertions;

namespace AuroraPortalB2B.Core.Mediator.Tests.Authorization;

public sealed class PermissionNamesTests
{
    [Fact]
    public void All_ShouldContainAllPermissions()
    {
        PermissionNames.All.Should().Contain(new[]
        {
            PermissionNames.PartnersRead,
            PermissionNames.PartnersWrite,
            PermissionNames.PartnerUsersRead,
            PermissionNames.PartnerUsersWrite
        });
    }

    [Fact]
    public void All_ShouldNotContainDuplicates()
    {
        PermissionNames.All.Distinct(StringComparer.OrdinalIgnoreCase)
            .Count()
            .Should()
            .Be(PermissionNames.All.Length);
    }
}
