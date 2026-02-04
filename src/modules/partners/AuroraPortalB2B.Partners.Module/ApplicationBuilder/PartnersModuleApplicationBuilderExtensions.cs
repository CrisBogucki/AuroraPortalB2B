using AuroraPortalB2B.Partners.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Partners.Module.ApplicationBuilder;

public static class PartnersModuleApplicationBuilderExtensions
{
    public static WebApplication UsePartnersModule(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PartnersDbContext>();
        var providerName = dbContext.Database.ProviderName ?? string.Empty;
        if (!providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase)
            && dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }
}
