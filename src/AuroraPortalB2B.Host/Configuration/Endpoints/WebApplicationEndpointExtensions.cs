using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AuroraPortalB2B.Partners.Module.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AuroraPortalB2B.Host.Configuration.Endpoints;

public static class WebApplicationEndpointExtensions
{
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

        app.MapGet("/version", (IApiVersionDescriptionProvider provider, IHostEnvironment env) =>
            {
                var appVersion = ResolveChangelogVersion(env);

                var apiVersions = provider.ApiVersionDescriptions
                    .Select(description => new
                    {
                        version = description.ApiVersion.ToString(),
                        group = description.GroupName,
                        isDeprecated = description.IsDeprecated
                    })
                    .ToArray();

                return Results.Ok(new
                {
                    appVersion,
                    apiVersions
                });
            })
            .AllowAnonymous();

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

    private static string ResolveChangelogVersion(IHostEnvironment env)
    {
        var changelogPath = FindChangelogPath(env.ContentRootPath);
        if (changelogPath is null)
        {
            return "unknown";
        }

        foreach (var line in File.ReadLines(changelogPath))
        {
            if (!line.StartsWith("## ", StringComparison.Ordinal))
            {
                continue;
            }

            var header = line[3..].Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            if (header.Equals("LATEST", StringComparison.OrdinalIgnoreCase))
            {
                return "LATEST";
            }

            if (header.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return header;
            }
        }

        return "unknown";
    }

    private static string? FindChangelogPath(string contentRoot)
    {
        var directory = new DirectoryInfo(contentRoot);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "CHANGELOG.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
