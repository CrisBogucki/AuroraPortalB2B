using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed record UpdatePartnerCommand(
    Guid Id,
    string Name,
    string Nip,
    string? Regon,
    string? CountryCode,
    string? City,
    string? Street,
    string? PostalCode)
    : IRequest<Result>;
