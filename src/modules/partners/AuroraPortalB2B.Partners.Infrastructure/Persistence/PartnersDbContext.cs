using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.App.Abstractions.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Persistence;

public sealed class PartnersDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public PartnersDbContext(DbContextOptions<PartnersDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerUser> PartnerUsers => Set<PartnerUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partner>()
            .HasQueryFilter(partner => partner.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<PartnerUser>()
            .HasQueryFilter(user => user.TenantId == _tenantContext.TenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartnersDbContext).Assembly);
    }
}
