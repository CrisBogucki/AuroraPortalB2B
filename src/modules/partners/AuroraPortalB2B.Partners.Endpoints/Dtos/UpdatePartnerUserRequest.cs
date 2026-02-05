namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record UpdatePartnerUserRequest(
    string Email,
    string FirstName,
    string LastName);
