using System.Text.Json;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Endpoints.DependencyInjection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AuroraPortalB2B.Partners.Endpoints.Tests.DependencyInjection;

public sealed class ProblemDetailsFactoryTests
{
    [Fact]
    public async Task FromError_ShouldWriteProblemDetails()
    {
        // arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var services = new ServiceCollection()
            .AddProblemDetails()
            .AddLogging()
            .BuildServiceProvider();
        context.RequestServices = services;

        // act
        var result = ProblemDetailsFactory.FromError(new Error("test.code", "Something went wrong"));
        await result.ExecuteAsync(context);

        // assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        doc.RootElement.GetProperty("code").GetString().Should().Be("test.code");
    }
}
