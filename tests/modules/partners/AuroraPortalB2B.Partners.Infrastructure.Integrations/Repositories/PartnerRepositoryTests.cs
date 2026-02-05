using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using AuroraPortalB2B.Partners.Infrastructure.Integrations.Helper;
using FluentAssertions;

namespace AuroraPortalB2B.Partners.Infrastructure.Integrations.Repositories;

public sealed class PartnerRepositoryTests
{
    [Fact]
    public async Task ListAsync_ShouldOrderAndPaginate()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(ListAsync_ShouldOrderAndPaginate));
        var repo = new PartnerRepository(dbContext);

        dbContext.Partners.AddRange(
            new Partner(Guid.NewGuid(), "Beta", new Nip("1111111111")),
            new Partner(Guid.NewGuid(), "Alpha", new Nip("1234563218")),
            new Partner(Guid.NewGuid(), "Gamma", new Nip("2222222222")));
        await dbContext.SaveChangesAsync();

        // act
        var result = await repo.ListAsync(2, 0, includeInactive: true, cancellationToken: CancellationToken.None);

        // assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
    }
}
