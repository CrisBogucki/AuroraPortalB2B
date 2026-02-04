using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Queries;

public sealed class ListPartnerUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUsers()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        // ReSharper disable once CollectionNeverUpdated.Local
        var users = new List<PartnerUser>();
        repo.Setup(r => r.ListByPartnerIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var handler = new ListPartnerUsersQueryHandler(repo.Object);

        // act
        var result = await handler.Handle(new ListPartnerUsersQuery(Guid.NewGuid()), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(users);
    }
}
