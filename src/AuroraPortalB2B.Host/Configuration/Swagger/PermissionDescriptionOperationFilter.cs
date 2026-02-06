using AuroraPortalB2B.Core.Mediator.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuroraPortalB2B.Host.Configuration.Swagger;

public sealed class PermissionDescriptionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var policyNames = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Where(policy => !string.IsNullOrWhiteSpace(policy))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (policyNames.Count == 0)
        {
            return;
        }

        var lines = PermissionPolicies.All
            .Where(policy => policyNames.Contains(policy.Name, StringComparer.OrdinalIgnoreCase))
            .Select(policy => $"- `{policy.Name}`: {policy.Description}")
            .ToList();

        if (lines.Count == 0)
        {
            return;
        }

        var prefix = "Required permissions:";
        var addition = $"{prefix}\n{string.Join("\n", lines)}";

        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? addition
            : $"{operation.Description}\n\n{addition}";
    }
}
