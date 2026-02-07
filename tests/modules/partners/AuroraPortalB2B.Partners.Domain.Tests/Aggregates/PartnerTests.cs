using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Tests.Aggregates;

public sealed class PartnerTests
{
    [Fact]
    public void Constructor_ShouldSetDefaults()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var partner = new Partner(id, "tenant-1", "Acme", new Nip("1234563218"));

        // assert
        partner.Status.Should().Be(PartnerStatus.Active);
        partner.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        // arrange
        var partner = new Partner(Guid.NewGuid(), "tenant-1", "Acme", new Nip("1234563218"));

        // act
        partner.Rename("New Name");

        // assert
        partner.Name.Should().Be("New Name");
    }

    [Fact]
    public void AddUser_ShouldAttachUserToPartner()
    {
        // arrange
        var partner = new Partner(Guid.NewGuid(), "tenant-1", "Acme", new Nip("1234563218"));

        // act
        var user = partner.AddUser(Guid.NewGuid(), "kc-user-1", new Email("user@acme.com"), "Jan", "Kowalski");

        // assert
        partner.Users.Should().Contain(user);
        user.PartnerId.Should().Be(partner.Id);
    }
}
