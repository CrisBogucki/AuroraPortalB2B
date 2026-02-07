using AuroraPortalB2B.Partners.App.Abstractions.Tenancy;

namespace AuroraPortalB2B.Host.Configuration.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public string TenantId { get; private set; } = string.Empty;

    public void SetTenantId(string tenantId)
    {
        TenantId = tenantId;
    }
}
