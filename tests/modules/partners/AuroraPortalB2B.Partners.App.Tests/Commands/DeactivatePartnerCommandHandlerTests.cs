using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class DeactivatePartnerCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenPartnerNotFound()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var userRepo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var handler = new DeactivatePartnerCommandHandler(repo.Object, userRepo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerCommand(Guid.NewGuid()), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.not_found", "Partner not found."));
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeactivatePartner()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var userRepo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"));

        repo.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        userRepo.Setup(r => r.ListByPartnerIdAsync(partner.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PartnerUser>());

        var handler = new DeactivatePartnerCommandHandler(repo.Object, userRepo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerCommand(partner.Id), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        partner.Status.Should().Be(PartnerStatus.Inactive);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeactivatePartnerUsers()
    {
        // arrange
        var repo = new Mock<IPartnerRepository>();
        var userRepo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partnerId = Guid.NewGuid();
        var partner = new Partner(partnerId, "Acme", new Nip("1234563218"));
        var user1 = new PartnerUser(Guid.NewGuid(), partnerId, new Email("user1@acme.com"), "Jan", "Nowak");
        var user2 = new PartnerUser(Guid.NewGuid(), partnerId, new Email("user2@acme.com"), "Anna", "Kowalska");

        repo.Setup(r => r.GetByIdAsync(partnerId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        userRepo.Setup(r => r.ListByPartnerIdAsync(partnerId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user1, user2 });

        var handler = new DeactivatePartnerCommandHandler(repo.Object, userRepo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerCommand(partnerId), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        user1.Status.Should().Be(PartnerUserStatus.Inactive);
        user2.Status.Should().Be(PartnerUserStatus.Inactive);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
