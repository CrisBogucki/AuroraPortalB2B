using Asp.Versioning.ApiExplorer;
using AuroraPortalB2B.Partners.Module.ApplicationBuilder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Diagnostics;

namespace AuroraPortalB2B.Host.Configuration.Pipeline;

public static class WebApplicationPipelineExtensions
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

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
        app.Use(async (context, next) =>
        {
            var correlationId = GetOrCreateCorrelationId(context);
            context.Items[CorrelationIdHeader] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            using var correlation = LogContext.PushProperty("CorrelationId", correlationId);
            using var trace = LogContext.PushProperty("TraceId", correlationId);
            await next();
        });
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
                diagnosticContext.Set("TraceId", ResolveCorrelationId(httpContext));
                diagnosticContext.Set("CorrelationId", ResolveCorrelationId(httpContext));
            };
        });
        app.UseAuthentication();
        app.Use((context, next) =>
        {
            using var username = LogContext.PushProperty("Username", ResolveUsername(context));
            return next();
        });
        app.UseAuthorization();

        return app;
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdHeader, out var value)
            && value is string stored
            && !string.IsNullOrWhiteSpace(stored))
        {
            return stored;
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

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

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue!;
        }

        return Guid.NewGuid().ToString("N");
    }
}
