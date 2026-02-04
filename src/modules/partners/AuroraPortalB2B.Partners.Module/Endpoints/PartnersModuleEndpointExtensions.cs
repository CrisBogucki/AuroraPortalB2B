using AuroraPortalB2B.Partners.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace AuroraPortalB2B.Partners.Module.Endpoints;

public static class PartnersModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapPartnersModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPartnersEndpoints();
        return endpoints;
    }
}
