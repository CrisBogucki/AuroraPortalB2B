using AuroraPortalB2B.Partners.Endpoints.Dtos;
using FluentValidation;

namespace AuroraPortalB2B.Partners.Endpoints.Validators;

public sealed class CreatePartnerUserRequestValidator : AbstractValidator<CreatePartnerUserRequest>
{
    public CreatePartnerUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Phone)
            .MaximumLength(30);

        RuleFor(request => request.Notes)
            .MaximumLength(1000);
    }
}
