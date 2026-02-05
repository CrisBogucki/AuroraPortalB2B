using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed class ListPartnerUsersQueryHandler(IPartnerUserRepository partnerUserRepository)
    : IRequestHandler<ListPartnerUsersQuery, Result<IReadOnlyList<PartnerUser>>>
{
    public async Task<Result<IReadOnlyList<PartnerUser>>> Handle(ListPartnerUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await partnerUserRepository.ListByPartnerIdAsync(query.PartnerId, query.Limit, query.Offset, query.IncludeInactive, cancellationToken);
        return Result<IReadOnlyList<PartnerUser>>.Success(users);
    }
}
