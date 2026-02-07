using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class UpdatePartnerCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenPartnerNotFound()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var handler = new UpdatePartnerCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerCommand(
            Guid.NewGuid(),
            "Acme",
            "1234563218",
            null,
            null,
            null,
            null,
            null), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.not_found", "Partner not found."));
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorWhenNipExists()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partner = new Partner(Guid.NewGuid(), "tenant-1", "Acme", new Nip("1234563218"));

        repo.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        repo.Setup(r => r.ExistsByNipAsync(It.IsAny<Nip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdatePartnerCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerCommand(
            partner.Id,
            "Acme",
            "9876543210",
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
    public async Task Handle_ShouldUpdatePartner()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partner = new Partner(Guid.NewGuid(), "tenant-1", "Acme", new Nip("1234563218"));

        repo.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        repo.Setup(r => r.ExistsByNipAsync(It.IsAny<Nip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdatePartnerCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerCommand(
            partner.Id,
            "Acme Updated",
            "1234563218",
            "852163975",
            "PL",
            "Krakow",
            "Main 1",
            "30-001"), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        partner.Name.Should().Be("Acme Updated");
        partner.Regon!.Value.Should().Be("852163975");
        partner.Address.Should().NotBeNull();
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
