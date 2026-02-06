using System.Security.Claims;
using System.Text.Json;
using AuroraPortalB2B.Core.Mediator.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace AuroraPortalB2B.Host.Configuration.Authentication;

public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) || rolesElement.ValueKind != JsonValueKind.Array)
            {
                return Task.FromResult(principal);
            }

            foreach (var roleElement in rolesElement.EnumerateArray())
            {
                var role = roleElement.GetString();
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                if (!identity.HasClaim("roles", role))
                {
                    identity.AddClaim(new Claim("roles", role));
                }
            }

            var permissionClaims = identity
                .FindAll(PermissionNames.ClaimType)
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var roleElement in rolesElement.EnumerateArray())
            {
                var role = roleElement.GetString();
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var permission in PermissionNames.All)
                    {
                        if (permissionClaims.Add(permission))
                        {
                            identity.AddClaim(new Claim(PermissionNames.ClaimType, permission));
                        }
                    }

                    continue;
                }

                if (PermissionNames.All.Contains(role, StringComparer.OrdinalIgnoreCase) && permissionClaims.Add(role))
                {
                    identity.AddClaim(new Claim(PermissionNames.ClaimType, role));
                }
            }
        }
        catch (JsonException)
        {
            return Task.FromResult(principal);
        }

        return Task.FromResult(principal);
    }
}
