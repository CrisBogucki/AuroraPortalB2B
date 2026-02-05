using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class CreatePartnerUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenPartnerMissing()
    {
        // arrange
        var partnerRepo = new Mock<IPartnerRepository>();
        var userRepo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        partnerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var handler = new CreatePartnerUserCommandHandler(partnerRepo.Object, userRepo.Object, uow.Object);

        // act
        var result = await handler.Handle(new CreatePartnerUserCommand(
            Guid.NewGuid(),
            "user@acme.com",
            "Jan",
            "Kowalski"), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.not_found", "Partner not found."));
    }

    [Fact]
    public async Task Handle_ShouldCreateUser()
    {
        // arrange
        var partnerRepo = new Mock<IPartnerRepository>();
        var userRepo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"));

        partnerRepo.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var handler = new CreatePartnerUserCommandHandler(partnerRepo.Object, userRepo.Object, uow.Object);

        // act
        var result = await handler.Handle(new CreatePartnerUserCommand(
            partner.Id,
            "user@acme.com",
            "Jan",
            "Kowalski"), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        userRepo.Verify(r => r.AddAsync(It.IsAny<PartnerUser>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
