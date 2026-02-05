using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class DeactivatePartnerUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenUserNotFound()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerUser?)null);

        var handler = new DeactivatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerUserCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.user_not_found", "Partner user not found."));
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorWhenPartnerMismatch()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partnerId = Guid.NewGuid();
        var user = new PartnerUser(Guid.NewGuid(), Guid.NewGuid(), new Email("user@acme.com"), "Jan", "Nowak");

        repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeactivatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerUserCommand(partnerId, user.Id), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.user_not_found", "Partner user not found."));
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateUser()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partnerId = Guid.NewGuid();
        var user = new PartnerUser(Guid.NewGuid(), partnerId, new Email("user@acme.com"), "Jan", "Nowak");

        repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeactivatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new DeactivatePartnerUserCommand(partnerId, user.Id), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(PartnerUserStatus.Inactive);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
