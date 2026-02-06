using Asp.Versioning;
using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Core.Mediator.Authorization;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.Endpoints.DependencyInjection;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuroraPortalB2B.Partners.Endpoints;

public static class PartnerUsersEndpoints
{

    public static IEndpointRouteBuilder MapPartnerUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/partners/{id:guid}/users")
            .WithTags("Partner Users");

        group.MapPost("/", CreatePartnerUserAsync)
            .WithName("Partners_CreateUser")
            .WithSummary("Create partner user")
            .WithDescription("Creates a user assigned to the partner.")
            .Produces<CreatePartnerUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnerUsersWrite.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPut("/{userId:guid}", UpdatePartnerUserAsync)
            .WithName("Partners_Users_Update")
            .WithSummary("Update partner user")
            .WithDescription("Updates partner user email and name.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnerUsersWrite.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapDelete("/{userId:guid}", DeactivatePartnerUserAsync)
            .WithName("Partners_Users_Deactivate")
            .WithSummary("Deactivate partner user")
            .WithDescription("Soft deletes a partner user by setting status to Inactive.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnerUsersWrite.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/", ListPartnerUsersAsync)
            .WithName("Partners_Users_List")
            .WithSummary("List partner users")
            .WithDescription("Returns a paged list of users for the partner. Use includeInactive=true to include inactive records.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionPolicies.PartnerUsersRead.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        return endpoints;
    }

    private static IResult? ValidatePaging(int? limit, int? offset)
    {
        var errors = new Dictionary<string, string[]>();

        if (limit is < 1 or > 200)
        {
            errors["limit"] = ["Limit must be between 1 and 200."];
        }

        if (offset is < 0)
        {
            errors["offset"] = ["Offset must be 0 or greater."];
        }

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static async Task<IResult> CreatePartnerUserAsync(
        Guid id,
        CreatePartnerUserRequest request,
        [FromServices] IValidator<CreatePartnerUserRequest> validator,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sender.Send(new CreatePartnerUserCommand(
            id,
            request.KeycloakUserId,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Notes), ct);

        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!);
        }

        return Results.Created($"/api/v1/partners/{id}/users/{result.Value}", new CreatePartnerUserResponse(result.Value));
    }

    private static async Task<IResult> UpdatePartnerUserAsync(
        Guid id,
        Guid userId,
        UpdatePartnerUserRequest request,
        [FromServices] IValidator<UpdatePartnerUserRequest> validator,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sender.Send(new UpdatePartnerUserCommand(
            id,
            userId,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Notes), ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Code == "partners.user_not_found"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ProblemDetailsFactory.FromError(result.Error!, statusCode);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivatePartnerUserAsync(
        Guid id,
        Guid userId,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeactivatePartnerUserCommand(id, userId), ct);
        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!, StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ListPartnerUsersAsync(
        Guid id,
        int? limit,
        int? offset,
        bool? includeInactive,
        ClaimsPrincipal user,
        [FromServices] IPartnerUserRepository partnerUserRepository,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validationProblem = ValidatePaging(limit, offset);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        if (!user.IsInRole("admin"))
        {
            var currentUser = await ResolvePartnerUserAsync(user, partnerUserRepository, ct);
            if (currentUser is null || currentUser.PartnerId != id)
            {
                return Results.Forbid();
            }

            includeInactive = false;
        }

        var result = await sender.Send(new ListPartnerUsersQuery(id, limit ?? 50, offset ?? 0, includeInactive ?? false), ct);
        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!);
        }

        var items = result.Value ?? [];
        var response = items.Select(user => new PartnerUserDto(
            user.Id,
            user.PartnerId,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Status.ToString(),
            user.Phone,
            user.Notes));

        var partnerUserDtos = response as PartnerUserDto[] ?? response.ToArray();
        return Results.Ok(new { items = partnerUserDtos, total = partnerUserDtos.Length });
    }

    private static async Task<PartnerUser?> ResolvePartnerUserAsync(
        ClaimsPrincipal user,
        IPartnerUserRepository partnerUserRepository,
        CancellationToken ct)
    {
        var keycloakUserId = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return null;
        }

        return await partnerUserRepository.GetByKeycloakUserIdAsync(keycloakUserId, ct);
    }
}
