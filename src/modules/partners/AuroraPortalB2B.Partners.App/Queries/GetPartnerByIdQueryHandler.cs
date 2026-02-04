using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed class GetPartnerByIdQueryHandler(IPartnerRepository partnerRepository)
    : IRequestHandler<GetPartnerByIdQuery, Result<Partner>>
{
    public async Task<Result<Partner>> Handle(GetPartnerByIdQuery query, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(query.Id, cancellationToken);
        return partner is null
            ? Result<Partner>.Fail("partners.not_found", "Partner not found.")
            : Result<Partner>.Success(partner);
    }
}
