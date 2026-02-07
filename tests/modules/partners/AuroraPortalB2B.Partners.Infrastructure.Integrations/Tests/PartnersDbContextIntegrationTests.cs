using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Integrations.Helper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Integrations.Tests;

public sealed class PartnersDbContextIntegrationTests
{
    [Fact]
    public async Task SaveChanges_ShouldPersistOwnedValues()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(SaveChanges_ShouldPersistOwnedValues));
        var partner = new Partner(
            Guid.NewGuid(), "tenant-1", "Acme",
            new Nip("1234563218"),
            new Regon("852163975"),
            new Address("PL", "Krakow", "Main", "30-001"));

        dbContext.Partners.Add(partner);

        // act
        await dbContext.SaveChangesAsync();
        var stored = await dbContext.Partners.FirstAsync();

        // assert
        stored.Nip.Value.Should().Be("1234563218");
        stored.Regon.Should().NotBeNull();
        stored.Regon!.Value.Should().Be("852163975");
        stored.Address.Should().NotBeNull();
        stored.Address!.City.Should().Be("Krakow");
    }
}
