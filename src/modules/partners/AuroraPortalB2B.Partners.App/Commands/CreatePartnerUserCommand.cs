using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed record CreatePartnerUserCommand(
    Guid PartnerId,
    string Email,
    string FirstName,
    string LastName) : IRequest<Result<Guid>>;
