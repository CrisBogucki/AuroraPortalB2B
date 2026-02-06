namespace AuroraPortalB2B.Core.Mediator.Authorization;

public static class PermissionNames
{
    public const string ClaimType = "permissions";

    public const string PartnersRead = "partners.read";
    public const string PartnersWrite = "partners.write";
    public const string PartnerUsersRead = "partnerUsers.read";
    public const string PartnerUsersWrite = "partnerUsers.write";

    public static readonly string[] All =
    [
        PartnersRead,
        PartnersWrite,
        PartnerUsersRead,
        PartnerUsersWrite
    ];
}
