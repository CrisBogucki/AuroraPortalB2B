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

public static class PartnersEndpoints
{

    public static IEndpointRouteBuilder MapPartnersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/partners")
            .WithTags("Partners");

        group.MapGet("/", ListPartnersAsync)
            .WithName("Partners_List")
            .WithSummary("List partners")
            .WithDescription("Returns a paged list of partners. Use includeInactive=true to include inactive records.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnersRead.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/{id:guid}", GetPartnerAsync)
            .WithName("Partners_Get")
            .WithSummary("Get partner")
            .WithDescription("Returns partner details by id. Use includeInactive=true to include inactive records.")
            .Produces<PartnerDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnersRead.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPost("/", CreatePartnerAsync)
            .WithName("Partners_Create")
            .WithSummary("Create partner")
            .WithDescription("Creates a new partner.")
            .Produces<CreatePartnerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnersWrite.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPut("/{id:guid}", UpdatePartnerAsync)
            .WithName("Partners_Update")
            .WithSummary("Update partner")
            .WithDescription("Updates partner name, NIP, REGON and address.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnersWrite.Name)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapDelete("/{id:guid}", DeactivatePartnerAsync)
            .WithName("Partners_Deactivate")
            .WithSummary("Deactivate partner")
            .WithDescription("Soft deletes a partner by setting status to Inactive (also deactivates users).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(PermissionPolicies.PartnersWrite.Name)
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

    private static async Task<IResult> ListPartnersAsync(
        int? limit,
        int? offset,
        bool? includeInactive,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validationProblem = ValidatePaging(limit, offset);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var result = await sender.Send(new ListPartnersQuery(limit ?? 50, offset ?? 0, includeInactive ?? false), ct);
        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!);
        }

        var items = result.Value ?? [];
        var response = items.Select(partner => new PartnerListItemDto(
            partner.Id,
            partner.Name,
            partner.Nip.Value,
            partner.Regon?.Value,
            partner.Status.ToString(),
            partner.Phone,
            partner.Notes));

        var partnerListItemDtos = response as PartnerListItemDto[] ?? response.ToArray();
        return Results.Ok(new { items = partnerListItemDtos, total = partnerListItemDtos.Count() });
    }

    private static async Task<IResult> GetPartnerAsync(
        Guid id,
        bool? includeInactive,
        ClaimsPrincipal user,
        [FromServices] IPartnerUserRepository partnerUserRepository,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        if (!user.IsInRole("admin"))
        {
            var currentUser = await ResolvePartnerUserAsync(user, partnerUserRepository, ct);
            if (currentUser is null || currentUser.PartnerId != id)
            {
                return Results.Forbid();
            }

            includeInactive = false;
        }

        var result = await sender.Send(new GetPartnerByIdQuery(id, includeInactive ?? false), ct);
        if (result.IsFailure || result.Value is null)
        {
            return ProblemDetailsFactory.FromError(result.Error!, StatusCodes.Status404NotFound);
        }

        var dto = new PartnerDetailsDto(
            result.Value.Id,
            result.Value.Name,
            result.Value.Nip.Value,
            result.Value.Regon?.Value,
            result.Value.Status.ToString(),
            result.Value.Address is null
                ? null
                : new AddressDto(
                    result.Value.Address.CountryCode,
                    result.Value.Address.City,
                    result.Value.Address.Street,
                    result.Value.Address.PostalCode),
            result.Value.Phone,
            result.Value.Notes);

        return Results.Ok(dto);
    }

    private static async Task<IResult> CreatePartnerAsync(
        CreatePartnerRequest request,
        [FromServices] IValidator<CreatePartnerRequest> validator,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sender.Send(new CreatePartnerCommand(
            request.Name,
            request.Nip,
            request.Regon,
            request.Address?.CountryCode,
            request.Address?.City,
            request.Address?.Street,
            request.Address?.PostalCode,
            request.Phone,
            request.Notes), ct);

        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!);
        }

        return Results.Created($"/api/v1/partners/{result.Value}", new CreatePartnerResponse(result.Value));
    }

    private static async Task<IResult> UpdatePartnerAsync(
        Guid id,
        UpdatePartnerRequest request,
        [FromServices] IValidator<UpdatePartnerRequest> validator,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sender.Send(new UpdatePartnerCommand(
            id,
            request.Name,
            request.Nip,
            request.Regon,
            request.Address?.CountryCode,
            request.Address?.City,
            request.Address?.Street,
            request.Address?.PostalCode,
            request.Phone,
            request.Notes), ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Code == "partners.not_found"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ProblemDetailsFactory.FromError(result.Error!, statusCode);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivatePartnerAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeactivatePartnerCommand(id), ct);
        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!, StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
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
