using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed record CreatePartnerCommand(
    string Name,
    string Nip,
    string? Regon,
    string? CountryCode,
    string? City,
    string? Street,
    string? PostalCode,
    string? Phone = null,
    string? Notes = null) : IRequest<Result<Guid>>;
