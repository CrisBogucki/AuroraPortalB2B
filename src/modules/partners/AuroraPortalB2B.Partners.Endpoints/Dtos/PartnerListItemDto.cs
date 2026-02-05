namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record PartnerListItemDto(
    Guid Id,
    string Name,
    string Nip,
    string? Regon,
    string Status,
    string? Phone,
    string? Notes);
