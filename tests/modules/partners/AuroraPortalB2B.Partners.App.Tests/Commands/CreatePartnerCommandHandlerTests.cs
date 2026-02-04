using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class CreatePartnerCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenNipExists()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.ExistsByNipAsync(It.IsAny<Nip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreatePartnerCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new CreatePartnerCommand(
            "Acme",
            "1234563218",
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.exists", "Partner with given NIP already exists."));
    }

    [Fact]
    public async Task Handle_ShouldCreatePartner()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.ExistsByNipAsync(It.IsAny<Nip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreatePartnerCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new CreatePartnerCommand(
            "Acme",
            "1234563218",
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        repo.Verify(r => r.AddAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
