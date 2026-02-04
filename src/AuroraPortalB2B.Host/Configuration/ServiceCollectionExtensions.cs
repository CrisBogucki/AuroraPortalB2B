using AuroraPortalB2B.Host.Configuration.ApiVersioning;
using AuroraPortalB2B.Host.Configuration.Authentication;
using AuroraPortalB2B.Host.Configuration.Modules;
using AuroraPortalB2B.Host.Configuration.Swagger;

namespace AuroraPortalB2B.Host.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostAuthentication(configuration);
        services.AddHostApiVersioning();
        services.AddHostSwagger();
        services.AddHostModules(configuration);

        return services;
    }
}
