using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class UpdatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePartnerCommand, Result>
{
    public async Task<Result> Handle(UpdatePartnerCommand command, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);
        if (partner is null)
        {
            return Result.Fail("partners.not_found", "Partner not found.");
        }

        Nip nip;
        try
        {
            nip = new Nip(command.Nip);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail("validation.nip", ex.Message);
        }

        if (!string.Equals(partner.Nip.Value, nip.Value, StringComparison.Ordinal)
            && await partnerRepository.ExistsByNipAsync(nip, cancellationToken))
        {
            return Result.Fail("partners.exists", "Partner with given NIP already exists.");
        }

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(command.CountryCode)
            || !string.IsNullOrWhiteSpace(command.City)
            || !string.IsNullOrWhiteSpace(command.Street)
            || !string.IsNullOrWhiteSpace(command.PostalCode))
        {
            try
            {
                address = new Address(
                    command.CountryCode ?? string.Empty,
                    command.City ?? string.Empty,
                    command.Street ?? string.Empty,
                    command.PostalCode ?? string.Empty);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail("validation.address", ex.Message);
            }
        }

        Regon? regon = null;
        if (!string.IsNullOrWhiteSpace(command.Regon))
        {
            try
            {
                regon = new Regon(command.Regon);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail("validation.regon", ex.Message);
            }
        }

        try
        {
            partner.Rename(command.Name);
            partner.ChangeNip(nip);
            partner.ChangeRegon(regon);
            partner.ChangeAddress(address);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail("validation.partner", ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
