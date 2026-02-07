using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using AuroraPortalB2B.Partners.App.Abstractions.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Integrations.Helper;

public static class DbContextFactory
{
    public static PartnersDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PartnersDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new PartnersDbContext(options, new TestTenantContext());
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public string TenantId { get; } = "tenant-1";
    }
}
