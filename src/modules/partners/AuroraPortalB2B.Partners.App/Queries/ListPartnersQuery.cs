using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed record ListPartnersQuery(int Limit = 50, int Offset = 0, bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<Partner>>>;
