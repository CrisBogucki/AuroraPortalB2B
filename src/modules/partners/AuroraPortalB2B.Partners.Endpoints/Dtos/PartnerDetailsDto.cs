namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record PartnerDetailsDto(
    Guid Id,
    string Name,
    string Nip,
    string? Regon,
    string Status,
    AddressDto? Address);
