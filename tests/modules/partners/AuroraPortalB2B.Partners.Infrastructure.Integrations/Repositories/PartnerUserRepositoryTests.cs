using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using AuroraPortalB2B.Partners.Infrastructure.Integrations.Helper;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Infrastructure.Integrations.Repositories;

public sealed class PartnerUserRepositoryTests
{
    [Fact]
    public async Task ListByPartnerIdAsync_ShouldReturnUsersForPartner()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(ListByPartnerIdAsync_ShouldReturnUsersForPartner));
        var repo = new PartnerUserRepository(dbContext);

        var partnerId = Guid.NewGuid();
        var otherPartnerId = Guid.NewGuid();

        dbContext.PartnerUsers.AddRange(
            new PartnerUser(Guid.NewGuid(), partnerId, new Email("a@acme.com"), "A", "One"),
            new PartnerUser(Guid.NewGuid(), partnerId, new Email("b@acme.com"), "B", "Two"),
            new PartnerUser(Guid.NewGuid(), otherPartnerId, new Email("c@acme.com"), "C", "Three"));

        await dbContext.SaveChangesAsync();

        // act
        var result = await repo.ListByPartnerIdAsync(partnerId, 10, 0, CancellationToken.None);

        // assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(user => user.PartnerId == partnerId);
    }
}
