using AuroraPortalB2B.Partners.Endpoints.Dtos;
using FluentValidation;

namespace AuroraPortalB2B.Partners.Endpoints.Validators;

public sealed class UpdatePartnerRequestValidator : AbstractValidator<UpdatePartnerRequest>
{
    public UpdatePartnerRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Nip)
            .NotEmpty()
            .Length(10)
            .Matches("^[0-9]+$");

        RuleFor(request => request.Regon)
            .Must(value => string.IsNullOrWhiteSpace(value) || value!.Length is 9 or 14)
            .WithMessage("REGON must be 9 or 14 digits.");

        RuleFor(request => request.Phone)
            .MaximumLength(30);

        RuleFor(request => request.Notes)
            .MaximumLength(1000);

        When(request => request.Address is not null, () =>
        {
            RuleFor(request => request.Address!.CountryCode)
                .NotEmpty()
                .Length(2);
            RuleFor(request => request.Address!.City)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(request => request.Address!.Street)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(request => request.Address!.PostalCode)
                .NotEmpty()
                .MaximumLength(20);
        });
    }
}
