using AuroraPortalB2B.Host.Configuration.Swagger;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace AuroraPortalB2B.Host.Tests.Unit.Swagger;

public sealed class TagOrderingDocumentFilterTests
{
    [Fact]
    public void Apply_ShouldOrderTags_UsingPreferredOrder()
    {
        var document = new OpenApiDocument
        {
            Tags = new List<OpenApiTag>
            {
                new() { Name = "Other" },
                new() { Name = "Partner Users" },
                new() { Name = "Partners" }
            }
        };

        var filter = new TagOrderingDocumentFilter();
        filter.Apply(document, CreateContext());

        document.Tags!.Select(tag => tag.Name).Should().ContainInOrder("Partners", "Partner Users", "Other");
    }

    [Fact]
    public void Apply_ShouldInferTags_WhenMissingFromDocument()
    {
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/partners"] = new OpenApiPathItem
                {
                    Operations =
                    {
                        [OperationType.Get] = new OpenApiOperation
                        {
                            Tags = new List<OpenApiTag>
                            {
                                new() { Name = "Partner Users" },
                                new() { Name = "Partners" },
                                new() { Name = "Other" }
                            }
                        }
                    }
                }
            }
        };

        var filter = new TagOrderingDocumentFilter();
        filter.Apply(document, CreateContext());

        document.Tags!.Select(tag => tag.Name).Should().ContainInOrder("Partners", "Partner Users", "Other");
    }

    private static DocumentFilterContext CreateContext()
    {
        var generator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new JsonSerializerOptions()));

        return new DocumentFilterContext(new List<ApiDescription>(), generator, new SchemaRepository());
    }
}
