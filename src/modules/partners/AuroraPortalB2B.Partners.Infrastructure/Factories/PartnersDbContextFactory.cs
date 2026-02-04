using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuroraPortalB2B.Partners.Infrastructure.Factories;

public sealed class PartnersDbContextFactory : IDesignTimeDbContextFactory<PartnersDbContext>
{
    public PartnersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PARTNERS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PartnersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PartnersDbContext(options);
    }
}
