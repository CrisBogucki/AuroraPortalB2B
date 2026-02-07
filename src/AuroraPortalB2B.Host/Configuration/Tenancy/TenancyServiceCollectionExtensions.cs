using AuroraPortalB2B.Partners.App.Abstractions.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Host.Configuration.Tenancy;

public static class TenancyServiceCollectionExtensions
{
    public static IServiceCollection AddHostTenancy(this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        return services;
    }
}
