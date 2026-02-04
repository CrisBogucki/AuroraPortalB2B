using AuroraPortalB2B.Partners.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Persistence;

public sealed class PartnersDbContext : DbContext
{
    public PartnersDbContext(DbContextOptions<PartnersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerUser> PartnerUsers => Set<PartnerUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartnersDbContext).Assembly);
    }
}
