using Asp.Versioning;
using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Commands;
using AuroraPortalB2B.Partners.App.Queries;
using AuroraPortalB2B.Partners.Endpoints.DependencyInjection;
using AuroraPortalB2B.Partners.Endpoints.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

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
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/{id:guid}", GetPartnerAsync)
            .WithName("Partners_Get")
            .WithSummary("Get partner")
            .WithDescription("Returns partner details by id. Use includeInactive=true to include inactive records.")
            .Produces<PartnerDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPost("/", CreatePartnerAsync)
            .WithName("Partners_Create")
            .WithSummary("Create partner")
            .WithDescription("Creates a new partner.")
            .Produces<CreatePartnerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPut("/{id:guid}", UpdatePartnerAsync)
            .WithName("Partners_Update")
            .WithSummary("Update partner")
            .WithDescription("Updates partner name, NIP, REGON and address.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapDelete("/{id:guid}", DeactivatePartnerAsync)
            .WithName("Partners_Deactivate")
            .WithSummary("Deactivate partner")
            .WithDescription("Soft deletes a partner by setting status to Inactive (also deactivates users).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPost("/{id:guid}/users", CreatePartnerUserAsync)
            .WithName("Partners_CreateUser")
            .WithSummary("Create partner user")
            .WithDescription("Creates a user assigned to the partner.")
            .Produces<CreatePartnerUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPut("/{id:guid}/users/{userId:guid}", UpdatePartnerUserAsync)
            .WithName("Partners_Users_Update")
            .WithSummary("Update partner user")
            .WithDescription("Updates partner user email and name.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapDelete("/{id:guid}/users/{userId:guid}", DeactivatePartnerUserAsync)
            .WithName("Partners_Users_Deactivate")
            .WithSummary("Deactivate partner user")
            .WithDescription("Soft deletes a partner user by setting status to Inactive.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/{id:guid}/users", ListPartnerUsersAsync)
            .WithName("Partners_Users_List")
            .WithSummary("List partner users")
            .WithDescription("Returns a paged list of users for the partner. Use includeInactive=true to include inactive records.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
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
        [FromServices] ISender sender,
        CancellationToken ct)
    {
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
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validationProblem = ValidatePaging(limit, offset);
        if (validationProblem is not null)
        {
            return validationProblem;
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
}
