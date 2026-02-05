using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.Aggregates;

namespace AuroraPortalB2B.Partners.App.Queries;

public sealed record GetPartnerByIdQuery(Guid Id, bool IncludeInactive = false) : IRequest<Result<Partner>>;
