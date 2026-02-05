using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Queries;

public sealed class GetPartnerByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenMissing()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var handler = new GetPartnerByIdQueryHandler(repo.Object);

        // act
        var result = await handler.Handle(new GetPartnerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.not_found", "Partner not found."));
    }

    [Fact]
    public async Task Handle_ShouldReturnPartner()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"));
        repo.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var handler = new GetPartnerByIdQueryHandler(repo.Object);

        // act
        var result = await handler.Handle(new GetPartnerByIdQuery(partner.Id), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(partner);
    }
}
