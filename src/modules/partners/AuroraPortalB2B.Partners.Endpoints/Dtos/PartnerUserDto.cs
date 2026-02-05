namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record PartnerUserDto(
    Guid Id,
    Guid PartnerId,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    string? Phone,
    string? Notes);
