using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Queries;

public sealed class ListPartnersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPartners()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        // ReSharper disable once CollectionNeverUpdated.Local
        var partners = new List<Partner>();
        repo.Setup(r => r.ListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partners);

        var handler = new ListPartnersQueryHandler(repo.Object);

        // act
        var result = await handler.Handle(new ListPartnersQuery(), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(partners);
    }
}
