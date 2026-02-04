using AuroraPortalB2B.Partners.Module.DependencyInjection;

namespace AuroraPortalB2B.Host.Configuration.Modules;

public static class ModulesServiceCollectionExtensions
{
    public static IServiceCollection AddHostModules(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("partners")
            ?? "Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres";

        services.AddPartnersModule(connectionString);

        return services;
    }
}
