using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class CreatePartnerUserCommandHandler(
    IPartnerRepository partnerRepository,
    IPartnerUserRepository partnerUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePartnerUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePartnerUserCommand command, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(command.PartnerId, cancellationToken: cancellationToken);
        if (partner is null)
        {
            return Result<Guid>.Fail("partners.not_found", "Partner not found.");
        }

        Email email;
        try
        {
            email = new Email(command.Email);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Fail("validation.email", ex.Message);
        }

        PartnerUser user;
        try
        {
            user = partner.AddUser(Guid.NewGuid(), email, command.FirstName, command.LastName, command.Phone, command.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Fail("validation.user", ex.Message);
        }

        await partnerUserRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
