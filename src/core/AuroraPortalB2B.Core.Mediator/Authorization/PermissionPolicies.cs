namespace AuroraPortalB2B.Core.Mediator.Authorization;

public static class PermissionPolicies
{
    public sealed record Policy(string Name, string Description);

    public static readonly Policy PartnersRead = new(
        "PartnersRead",
        "Allows reading partner data.");

    public static readonly Policy PartnersWrite = new(
        "PartnersWrite",
        "Allows creating, updating, and deactivating partners.");

    public static readonly Policy PartnerUsersRead = new(
        "PartnerUsersRead",
        "Allows reading partner users.");

    public static readonly Policy PartnerUsersWrite = new(
        "PartnerUsersWrite",
        "Allows creating, updating, and deactivating partner users.");

    public static readonly Policy[] All =
    [
        PartnersRead,
        PartnersWrite,
        PartnerUsersRead,
        PartnerUsersWrite
    ];
}
