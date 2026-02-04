using AuroraPortalB2B.Partners.App.Common;
using Microsoft.AspNetCore.Http;

namespace AuroraPortalB2B.Partners.Endpoints.DependencyInjection;

public static class ProblemDetailsFactory
{
    public static IResult FromError(Error error, int statusCode = StatusCodes.Status400BadRequest)
        => Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
            });
}
