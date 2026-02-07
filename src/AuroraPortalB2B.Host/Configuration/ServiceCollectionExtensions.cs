using AuroraPortalB2B.Host.Configuration.ApiVersioning;
using AuroraPortalB2B.Host.Configuration.Authentication;
using AuroraPortalB2B.Host.Configuration.HealthChecks.Registration;
using AuroraPortalB2B.Host.Configuration.Modules;
using AuroraPortalB2B.Host.Configuration.Swagger;
using AuroraPortalB2B.Host.Configuration.Tenancy;

namespace AuroraPortalB2B.Host.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostTenancy();
        services.AddHostAuthentication(configuration);
        services.AddHostApiVersioning();
        services.AddHostSwagger();
        services.AddHostModules(configuration);
        services.AddHostHealthChecks();

        return services;
    }
}
