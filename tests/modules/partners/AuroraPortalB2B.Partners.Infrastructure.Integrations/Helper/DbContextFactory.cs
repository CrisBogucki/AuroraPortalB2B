using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuroraPortalB2B.Partners.Infrastructure.Integrations.Helper;

public static class DbContextFactory
{
    public static PartnersDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PartnersDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new PartnersDbContext(options);
    }
}
