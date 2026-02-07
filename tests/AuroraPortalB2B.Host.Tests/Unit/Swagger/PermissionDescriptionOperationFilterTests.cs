using AuroraPortalB2B.Core.Mediator.Authorization;
using AuroraPortalB2B.Host.Configuration.Swagger;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json;

namespace AuroraPortalB2B.Host.Tests.Unit.Swagger;

public sealed class PermissionDescriptionOperationFilterTests
{
    [Fact]
    public void Apply_ShouldAppendPermissionDescriptions_WhenPoliciesPresent()
    {
        var operation = new OpenApiOperation { Description = "Existing description." };
        var context = CreateContext(PermissionPolicies.PartnersRead.Name);

        var filter = new PermissionDescriptionOperationFilter();
        filter.Apply(operation, context);

        operation.Description.Should().Contain("Existing description.");
        operation.Description.Should().Contain("Required permissions:");
        operation.Description.Should().Contain(PermissionPolicies.PartnersRead.Description);
    }

    [Fact]
    public void Apply_ShouldLeaveDescriptionUnchanged_WhenNoMatchingPolicies()
    {
        var operation = new OpenApiOperation { Description = "Existing description." };
        var context = CreateContext("UnknownPolicy");

        var filter = new PermissionDescriptionOperationFilter();
        filter.Apply(operation, context);

        operation.Description.Should().Be("Existing description.");
    }

    private static OperationFilterContext CreateContext(string policyName)
    {
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = new List<object>
                {
                    new AuthorizeAttribute { Policy = policyName }
                }
            }
        };

        var generator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new JsonSerializerOptions()));

        return new OperationFilterContext(
            apiDescription,
            generator,
            new SchemaRepository(),
            GetDummyMethod());
    }

    private static MethodInfo GetDummyMethod()
        => typeof(DummyController).GetMethod(nameof(DummyController.Get))!;

    private sealed class DummyController
    {
        public void Get() { }
    }
}
