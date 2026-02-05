using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed record DeactivatePartnerUserCommand(Guid PartnerId, Guid UserId) : IRequest<Result>;
