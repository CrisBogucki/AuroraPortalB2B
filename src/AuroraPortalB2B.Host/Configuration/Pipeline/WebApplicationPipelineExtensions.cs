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
            using var requestMethod = LogContext.PushProperty("RequestMethod", context.Request.Method);
            using var requestScheme = LogContext.PushProperty("RequestScheme", context.Request.Scheme);
            using var requestHost = LogContext.PushProperty("RequestHost", context.Request.Host.Value ?? string.Empty);
            using var requestPath = LogContext.PushProperty("RequestPath", context.Request.Path.Value ?? string.Empty);
            using var requestQuery = LogContext.PushProperty("RequestQuery", context.Request.QueryString.Value ?? string.Empty);
            using var requestContentType = LogContext.PushProperty("RequestContentType", context.Request.ContentType ?? string.Empty);
            using var requestContentLength = LogContext.PushProperty("RequestContentLength", context.Request.ContentLength ?? 0);
            using var responseContentType = LogContext.PushProperty("ResponseContentType", context.Response.ContentType ?? string.Empty);
            using var responseContentLength = LogContext.PushProperty("ResponseContentLength", context.Response.ContentLength ?? 0);
            using var userAgent = LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString());
            using var remoteIp = LogContext.PushProperty("RemoteIp", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty);

            Log.Information("HTTP {RequestMethod} {RequestPath} started", context.Request.Method, context.Request.Path.Value ?? string.Empty);
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
                diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
                diagnosticContext.Set("RequestQuery", httpContext.Request.QueryString.Value ?? string.Empty);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
                diagnosticContext.Set("RequestContentType", httpContext.Request.ContentType ?? string.Empty);
                diagnosticContext.Set("RequestContentLength", httpContext.Request.ContentLength ?? 0);
                diagnosticContext.Set("ResponseContentType", httpContext.Response.ContentType ?? string.Empty);
                diagnosticContext.Set("ResponseContentLength", httpContext.Response.ContentLength ?? 0);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
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
