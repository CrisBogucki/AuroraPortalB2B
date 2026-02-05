using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class DeactivatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    IPartnerUserRepository partnerUserRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivatePartnerCommand, Result>
{
    public async Task<Result> Handle(DeactivatePartnerCommand command, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(command.Id, includeInactive: true, cancellationToken: cancellationToken);
        if (partner is null)
        {
            return Result.Fail("partners.not_found", "Partner not found.");
        }

        if (partner.Status != PartnerStatus.Inactive)
        {
            partner.Deactivate();
        }

        var users = await partnerUserRepository.ListByPartnerIdAsync(
            command.Id,
            limit: 200,
            offset: 0,
            includeInactive: true,
            cancellationToken: cancellationToken);

        foreach (var user in users)
        {
            if (user.Status != PartnerUserStatus.Inactive)
            {
                user.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
