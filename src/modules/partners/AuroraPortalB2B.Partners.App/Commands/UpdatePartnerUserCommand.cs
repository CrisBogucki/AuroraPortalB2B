using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed record UpdatePartnerUserCommand(
    Guid PartnerId,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName)
    : IRequest<Result>;
