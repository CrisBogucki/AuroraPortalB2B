namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record AddressDto(
    string CountryCode,
    string City,
    string Street,
    string PostalCode);
