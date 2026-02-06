using Microsoft.Extensions.Diagnostics.HealthChecks;

using AuroraPortalB2B.Host.Configuration.HealthChecks.Checks;

namespace AuroraPortalB2B.Host.Configuration.HealthChecks.Registration;

public static class HealthChecksServiceCollectionExtensions
{
    public static IServiceCollection AddHostHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("service is running"), tags: ["live", "ready"])
            .AddCheck<PartnersDbHealthCheck>("partners-db", tags: ["ready"]);

        return services;
    }
}
