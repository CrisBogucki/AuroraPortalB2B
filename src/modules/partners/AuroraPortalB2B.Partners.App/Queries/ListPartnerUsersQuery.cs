using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed record ListPartnerUsersQuery(Guid PartnerId, int Limit = 50, int Offset = 0)
    : IRequest<Result<IReadOnlyList<PartnerUser>>>;
