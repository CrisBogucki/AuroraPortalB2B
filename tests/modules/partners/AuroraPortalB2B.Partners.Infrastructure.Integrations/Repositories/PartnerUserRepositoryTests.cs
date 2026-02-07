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
            new PartnerUser(Guid.NewGuid(), "tenant-1", partnerId, "kc-user-1", new Email("a@acme.com"), "A", "One"),
            new PartnerUser(Guid.NewGuid(), "tenant-1", partnerId, "kc-user-2", new Email("b@acme.com"), "B", "Two"),
            new PartnerUser(Guid.NewGuid(), "tenant-1", otherPartnerId, "kc-user-3", new Email("c@acme.com"), "C", "Three"));

        await dbContext.SaveChangesAsync();

        // act
        var result = await repo.ListByPartnerIdAsync(partnerId, 10, 0, includeInactive: true, cancellationToken: CancellationToken.None);

        // assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(user => user.PartnerId == partnerId);
    }
}
