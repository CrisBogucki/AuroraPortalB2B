using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed class ListPartnersQueryHandler(IPartnerRepository partnerRepository)
    : IRequestHandler<ListPartnersQuery, Result<IReadOnlyList<Partner>>>
{
    public async Task<Result<IReadOnlyList<Partner>>> Handle(ListPartnersQuery query, CancellationToken cancellationToken)
    {
        var partners = await partnerRepository.ListAsync(query.Limit, query.Offset, query.IncludeInactive, cancellationToken);
        return Result<IReadOnlyList<Partner>>.Success(partners);
    }
}
