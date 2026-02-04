using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Tests.Unit;

public sealed class EfUnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldReturnAffectedRows()
    {
        // arrange
        var options = new DbContextOptionsBuilder<PartnersDbContext>()
            .UseInMemoryDatabase(nameof(SaveChangesAsync_ShouldReturnAffectedRows))
            .Options;

        await using var dbContext = new PartnersDbContext(options);
        var unitOfWork = new EfUnitOfWork(dbContext);

        // act
        var result = await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // assert
        result.Should().BeGreaterOrEqualTo(0);
    }
}
