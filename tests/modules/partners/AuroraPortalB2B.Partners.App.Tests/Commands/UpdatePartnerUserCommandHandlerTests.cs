using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuroraPortalB2B.Partners.App.Tests.Commands;

public sealed class UpdatePartnerUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnErrorWhenUserNotFound()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerUser?)null);

        var handler = new UpdatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerUserCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "user@acme.com",
            "Jan",
            "Nowak"), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.user_not_found", "Partner user not found."));
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorWhenPartnerMismatch()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var user = new PartnerUser(Guid.NewGuid(), Guid.NewGuid(), "kc-user-1", new Email("user@acme.com"), "Jan", "Nowak");

        repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerUserCommand(
            Guid.NewGuid(),
            user.Id,
            "user@acme.com",
            "Jan",
            "Nowak"), CancellationToken.None);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("partners.user_not_found", "Partner user not found."));
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser()
    {
        // arrange
        var repo = new Mock<IPartnerUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var partnerId = Guid.NewGuid();
        var user = new PartnerUser(Guid.NewGuid(), partnerId, "kc-user-1", new Email("user@acme.com"), "Jan", "Nowak");

        repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdatePartnerUserCommandHandler(repo.Object, uow.Object);

        // act
        var result = await handler.Handle(new UpdatePartnerUserCommand(
            partnerId,
            user.Id,
            "new@acme.com",
            "Anna",
            "Kowalska"), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be("new@acme.com");
        user.FirstName.Should().Be("Anna");
        user.LastName.Should().Be("Kowalska");
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
