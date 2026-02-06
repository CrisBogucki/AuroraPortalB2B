using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Domain.Integrations.Tests;

public sealed class PartnerAggregateIntegrationTests
{
    [Fact]
    public void AddUser_ShouldAttachUserToPartner()
    {
        // arrange
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"));
        var email = new Email("user@acme.com");

        // act
        var user = partner.AddUser(Guid.NewGuid(), "kc-user-1", email, "Jan", "Nowak");

        // assert
        partner.Users.Should().ContainSingle();
        partner.Users.First().Should().Be(user);
        user.PartnerId.Should().Be(partner.Id);
    }

    [Fact]
    public void CreatePartner_WithAddress_ShouldPersistAddressData()
    {
        // arrange
        var address = new Address("PL", "Krakow", "Main", "30-001");

        // act
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"), null, address);

        // assert
        partner.Address.Should().NotBeNull();
        partner.Address!.City.Should().Be("Krakow");
        partner.Address.PostalCode.Should().Be("30-001");
    }
}
