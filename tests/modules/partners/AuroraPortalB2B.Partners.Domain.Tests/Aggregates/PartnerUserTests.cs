using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.Aggregates;

public sealed class PartnerUserTests
{
    [Fact]
    public void Rename_ShouldUpdateNames()
    {
        // arrange
        var user = new PartnerUser(Guid.NewGuid(), Guid.NewGuid(), "kc-user-1", new Email("user@acme.com"), "Jan", "Kowalski");

        // act
        user.Rename("Anna", "Nowak");

        // assert
        user.FirstName.Should().Be("Anna");
        user.LastName.Should().Be("Nowak");
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail()
    {
        // arrange
        var user = new PartnerUser(Guid.NewGuid(), Guid.NewGuid(), "kc-user-1", new Email("user@acme.com"), "Jan", "Kowalski");

        // act
        user.ChangeEmail(new Email("new@acme.com"));

        // assert
        user.Email.Value.Should().Be("new@acme.com");
    }
}
