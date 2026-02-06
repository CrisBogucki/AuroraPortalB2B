using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AuroraPortalB2B.Partners.Module.ApplicationBuilder;
using AuroraPortalB2B.Partners.Module.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuroraPortalB2B.Host.Configuration;

public static class WebApplicationExtensions
{
    public static WebApplication UseHostPipeline(this WebApplication app)
    {
        app.UsePartnersModule();

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
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapHostEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteLivenessResponseAsync
        }).AllowAnonymous();

        app.MapHealthChecks("/hc/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteReadinessResponseAsync
        }).AllowAnonymous();

        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet)
            .RequireAuthorization();

        api.MapPartnersModule();

        return app;
    }

    private static Task WriteLivenessResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        using var writer = new Utf8JsonWriter(context.Response.BodyWriter);
        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());
        writer.WriteEndObject();
        writer.Flush();

        return Task.CompletedTask;
    }

    private static Task WriteReadinessResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = report.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        using var writer = new Utf8JsonWriter(context.Response.BodyWriter);
        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());
        writer.WriteStartArray("checks");

        foreach (var (name, entry) in report.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteString("status", entry.Status.ToString());
            writer.WriteString("description", entry.Description ?? string.Empty);
            if (entry.Exception is not null)
            {
                writer.WriteString("error", entry.Exception.Message);
            }
            writer.WriteNumber("durationMs", entry.Duration.TotalMilliseconds);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Task.CompletedTask;
    }
}
