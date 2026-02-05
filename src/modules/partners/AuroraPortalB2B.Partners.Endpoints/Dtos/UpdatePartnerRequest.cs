namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record UpdatePartnerRequest(
    string Name,
    string Nip,
    string? Regon,
    AddressDto? Address);
