using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class UpdatePartnerUserCommandHandler(
    IPartnerUserRepository partnerUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePartnerUserCommand, Result>
{
    public async Task<Result> Handle(UpdatePartnerUserCommand command, CancellationToken cancellationToken)
    {
        var user = await partnerUserRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user is null || user.PartnerId != command.PartnerId)
        {
            return Result.Fail("partners.user_not_found", "Partner user not found.");
        }

        Email email;
        try
        {
            email = new Email(command.Email);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail("validation.email", ex.Message);
        }

        try
        {
            user.ChangeEmail(email);
            user.Rename(command.FirstName, command.LastName);
            user.ChangePhone(command.Phone);
            user.ChangeNotes(command.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail("validation.user", ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
