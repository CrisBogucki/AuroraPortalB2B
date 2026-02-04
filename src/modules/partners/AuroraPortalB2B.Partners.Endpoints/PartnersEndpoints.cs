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
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/{id:guid}", GetPartnerAsync)
            .WithName("Partners_Get")
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPost("/", CreatePartnerAsync)
            .WithName("Partners_Create")
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapPost("/{id:guid}/users", CreatePartnerUserAsync)
            .WithName("Partners_CreateUser")
            .MapToApiVersion(new ApiVersion(1, 0));

        group.MapGet("/{id:guid}/users", ListPartnerUsersAsync)
            .WithName("Partners_Users_List")
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
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validationProblem = ValidatePaging(limit, offset);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var result = await sender.Send(new ListPartnersQuery(limit ?? 50, offset ?? 0), ct);
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
            partner.Status.ToString()));

        var partnerListItemDtos = response as PartnerListItemDto[] ?? response.ToArray();
        return Results.Ok(new { items = partnerListItemDtos, total = partnerListItemDtos.Count() });
    }

    private static async Task<IResult> GetPartnerAsync(
        Guid id,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPartnerByIdQuery(id), ct);
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
                    result.Value.Address.PostalCode));

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
            request.Address?.PostalCode), ct);

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
            request.LastName), ct);

        if (result.IsFailure)
        {
            return ProblemDetailsFactory.FromError(result.Error!);
        }

        return Results.Created($"/api/v1/partners/{id}/users/{result.Value}", new CreatePartnerUserResponse(result.Value));
    }

    private static async Task<IResult> ListPartnerUsersAsync(
        Guid id,
        int? limit,
        int? offset,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var validationProblem = ValidatePaging(limit, offset);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var result = await sender.Send(new ListPartnerUsersQuery(id, limit ?? 50, offset ?? 0), ct);
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
            user.Status.ToString()));

        var partnerUserDtos = response as PartnerUserDto[] ?? response.ToArray();
        return Results.Ok(new { items = partnerUserDtos, total = partnerUserDtos.Length });
    }
}
