using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuroraPortalB2B.Host.Configuration.Swagger;

public sealed class TagOrderingDocumentFilter : IDocumentFilter
{
    private static readonly string[] PreferredOrder = ["Partners", "Partner Users"];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var tags = swaggerDoc.Tags?.ToList() ?? [];
        if (tags.Count == 0)
        {
            foreach (var tag in from path in swaggerDoc.Paths.Values from operation in path.Operations.Values from tag in operation.Tags where !string.IsNullOrWhiteSpace(tag.Name) where tags.All(existing => !string.Equals(existing.Name, tag.Name, StringComparison.OrdinalIgnoreCase)) select tag)
            {
                tags.Add(new OpenApiTag { Name = tag.Name });
            }
        }

        var priority = PreferredOrder
            .Select((name, index) => new { name, index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        swaggerDoc.Tags = tags
            .OrderBy(tag => priority.TryGetValue(tag.Name ?? string.Empty, out var idx) ? idx : int.MaxValue)
            .ThenBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
