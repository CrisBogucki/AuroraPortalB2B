namespace AuroraPortalB2B.Partners.Endpoints.Dtos;

public sealed record CreatePartnerUserRequest(
    string Email,
    string FirstName,
    string LastName);
