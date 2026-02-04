using Asp.Versioning;
using AuroraPortalB2B.Host;
using AuroraPortalB2B.Partners.Module.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Host.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"),
                    new QueryStringApiVersionReader("api-version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        var connectionString = configuration.GetConnectionString("partners")
            ?? "Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres";
        services.AddPartnersModule(connectionString);

        return services;
    }
}
