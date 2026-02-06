using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace AuroraPortalB2B.Host.Configuration.HealthChecks.Checks;

public sealed class PartnersDbHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = NormalizeConnectionString(configuration.GetConnectionString("partners"))
            ?? "Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres";

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("database connection ok");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("partners-db connection failed", ex);
        }
    }

    private static string? NormalizeConnectionString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return value;
        }

        var user = string.Empty;
        var password = string.Empty;
        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            user = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
            {
                password = Uri.UnescapeDataString(parts[1]);
            }
        }

        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return string.Join(';', new[]
        {
            $"Host={uri.Host}",
            $"Port={port}",
            $"Database={database}",
            $"Username={user}",
            $"Password={password}",
            "SSL Mode=Require",
            "Trust Server Certificate=true"
        });
    }
}
