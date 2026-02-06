using AuroraPortalB2B.Core.Mediator.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AuroraPortalB2B.Host.Configuration.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddHostAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakSection = configuration.GetSection("Keycloak");
        var authority = keycloakSection["Authority"];
        var metadataAddress = keycloakSection["MetadataAddress"];
        var audience = keycloakSection["Audience"];
        var requireHttpsMetadata = keycloakSection.GetValue("RequireHttpsMetadata", true);
        var validIssuers = keycloakSection.GetSection("ValidIssuers").Get<string[]>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                
                if (!string.IsNullOrWhiteSpace(metadataAddress))
                {
                    options.MetadataAddress = metadataAddress;
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "roles"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(audience))
                        {
                            var azp = context.Principal?.FindFirst("azp")?.Value;
                            if (!string.Equals(azp, audience, StringComparison.OrdinalIgnoreCase))
                            {
                                context.Fail("Invalid token 'azp' (authorized party).");
                            }
                        }

                        return Task.CompletedTask;
                    }
                };

                if (validIssuers is { Length: > 0 })
                {
                    var normalizedAllowed = validIssuers
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.TrimEnd('/'))
                        .ToArray();

                    if (normalizedAllowed.Length > 0)
                    {
                        options.TokenValidationParameters.IssuerValidator = (issuer, _, _) =>
                        {
                            var normalizedIssuer = issuer.TrimEnd('/');
                            if (normalizedAllowed.Contains(normalizedIssuer, StringComparer.OrdinalIgnoreCase))
                            {
                                return issuer;
                            }

                            throw new SecurityTokenInvalidIssuerException($"The issuer '{issuer}' is invalid");
                        };
                    }
                }
                
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PermissionPolicies.PartnersRead.Name, policy => policy.RequireAssertion(context =>
                HasPermission(context, PermissionNames.PartnersRead)));
            options.AddPolicy(PermissionPolicies.PartnersWrite.Name, policy => policy.RequireAssertion(context =>
                HasPermission(context, PermissionNames.PartnersWrite)));
            options.AddPolicy(PermissionPolicies.PartnerUsersRead.Name, policy => policy.RequireAssertion(context =>
                HasPermission(context, PermissionNames.PartnerUsersRead)));
            options.AddPolicy(PermissionPolicies.PartnerUsersWrite.Name, policy => policy.RequireAssertion(context =>
                HasPermission(context, PermissionNames.PartnerUsersWrite)));
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddTransient<IClaimsTransformation, KeycloakRoleClaimsTransformation>();

        return services;
    }

    private static bool HasPermission(AuthorizationHandlerContext context, string permission)
        => context.User.HasClaim(PermissionNames.ClaimType, permission)
           || context.User.IsInRole("admin");
}
