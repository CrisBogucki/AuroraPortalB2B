using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.App.Integrations.Helper;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using AuroraPortalB2B.Partners.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.App.Integrations.Tests;

public sealed class PartnerHandlersIntegrationTests
{
    [Fact]
    public async Task CreatePartner_ShouldPersistPartner()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(CreatePartner_ShouldPersistPartner));
        var repo = new PartnerRepository(dbContext);
        var uow = new EfUnitOfWork(dbContext);
        var handler = new CreatePartnerCommandHandler(repo, uow);

        // act
        var result = await handler.Handle(new CreatePartnerCommand(
            "Acme",
            "1234563218",
            "852163975",
            "PL",
            "Krakow",
            "Main",
            "30-001"), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        var partner = await dbContext.Partners.SingleAsync();
        partner.Id.Should().Be(result.Value);
        partner.Nip.Value.Should().Be("1234563218");
        partner.Address.Should().NotBeNull();
    }

    [Fact]
    public async Task ListPartners_ShouldReturnOrderedItems()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(ListPartners_ShouldReturnOrderedItems));
        dbContext.Partners.AddRange(
            new Partner(Guid.NewGuid(), "Beta", new Nip("1111111111")),
            new Partner(Guid.NewGuid(), "Alpha", new Nip("1234563218")));
        await dbContext.SaveChangesAsync();

        var repo = new PartnerRepository(dbContext);
        var handler = new ListPartnersQueryHandler(repo);

        // act
        var result = await handler.Handle(new ListPartnersQuery(10, 0), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task CreatePartnerUser_ShouldPersistUserForPartner()
    {
        // arrange
        await using var dbContext = DbContextFactory.Create(nameof(CreatePartnerUser_ShouldPersistUserForPartner));
        var partner = new Partner(Guid.NewGuid(), "Acme", new Nip("1234563218"));
        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync();

        var partnerRepo = new PartnerRepository(dbContext);
        var userRepo = new PartnerUserRepository(dbContext);
        var uow = new EfUnitOfWork(dbContext);
        var handler = new CreatePartnerUserCommandHandler(partnerRepo, userRepo, uow);

        // act
        var result = await handler.Handle(new CreatePartnerUserCommand(
            partner.Id,
            "user@acme.com",
            "Jan",
            "Nowak"), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        var users = await dbContext.PartnerUsers.Where(u => u.PartnerId == partner.Id).ToListAsync();
        users.Should().HaveCount(1);
        users[0].Email.Value.Should().Be("user@acme.com");
    }
}
