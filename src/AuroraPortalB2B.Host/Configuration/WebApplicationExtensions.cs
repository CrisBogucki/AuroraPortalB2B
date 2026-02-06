using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AuroraPortalB2B.Partners.Module.ApplicationBuilder;
using AuroraPortalB2B.Partners.Module.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Diagnostics;

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
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (context, _, _) =>
                context.Request.Path.StartsWithSegments("/hc")
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information;

            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var username = ResolveUsername(httpContext);
                diagnosticContext.Set("Username", username);
                diagnosticContext.Set("TraceId", ResolveTraceId(httpContext));
            };
        });
        app.UseAuthentication();
        app.Use((context, next) =>
        {
            using var traceId = LogContext.PushProperty("TraceId", ResolveTraceId(context));
            using var username = LogContext.PushProperty("Username", ResolveUsername(context));
            return next();
        });
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapHostEndpoints(this WebApplication app)
    {
        app.MapGet("/favicon.ico", () => Results.NoContent())
            .AllowAnonymous();

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

    private static string ResolveTraceId(HttpContext context)
        => Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

    private static string ResolveUsername(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.Identity?.Name
                ?? user.FindFirst("preferred_username")?.Value
                ?? user.FindFirst("sub")?.Value
                ?? "unknown";
        }

        return "anonymous";
    }
}
