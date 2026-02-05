using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class DeactivatePartnerUserCommandHandler(
    IPartnerUserRepository partnerUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivatePartnerUserCommand, Result>
{
    public async Task<Result> Handle(DeactivatePartnerUserCommand command, CancellationToken cancellationToken)
    {
        var user = await partnerUserRepository.GetByIdAsync(command.UserId, includeInactive: true, cancellationToken: cancellationToken);
        if (user is null || user.PartnerId != command.PartnerId)
        {
            return Result.Fail("partners.user_not_found", "Partner user not found.");
        }

        if (user.Status != PartnerUserStatus.Inactive)
        {
            user.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
