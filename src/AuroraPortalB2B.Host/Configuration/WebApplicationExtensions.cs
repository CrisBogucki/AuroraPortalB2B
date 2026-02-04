using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AuroraPortalB2B.Partners.Module.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace AuroraPortalB2B.Host.Configuration;

public static class WebApplicationExtensions
{
    public static WebApplication UseHostPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });
        }

        app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication MapHostEndpoints(this WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        api.MapPartnersModule();

        return app;
    }
}
