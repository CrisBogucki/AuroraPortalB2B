namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record CreatePartnerRequest(
    string Name,
    string Nip,
    string? Regon,
    AddressDto? Address);
