using AuroraPortalB2B.Partners.Endpoints.Dtos;
using FluentValidation;

namespace AuroraPortalB2B.Partners.Endpoints.Validators;

public sealed class UpdatePartnerUserRequestValidator : AbstractValidator<UpdatePartnerUserRequest>
{
    public UpdatePartnerUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
