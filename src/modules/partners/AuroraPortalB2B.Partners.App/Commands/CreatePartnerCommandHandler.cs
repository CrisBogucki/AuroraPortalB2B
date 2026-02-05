using AuroraPortalB2B.Core.Mediator;
using AuroraPortalB2B.Partners.App.Common;
using AuroraPortalB2B.Partners.App.Abstractions.Repositories;
using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;

namespace AuroraPortalB2B.Partners.App.Commands;

public sealed class CreatePartnerCommandHandler(
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePartnerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePartnerCommand command, CancellationToken cancellationToken)
    {
        Nip nip;
        try
        {
            nip = new Nip(command.Nip);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Fail("validation.nip", ex.Message);
        }

        if (await partnerRepository.ExistsByNipAsync(nip, cancellationToken))
        {
            return Result<Guid>.Fail("partners.exists", "Partner with given NIP already exists.");
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
                return Result<Guid>.Fail("validation.address", ex.Message);
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
                return Result<Guid>.Fail("validation.regon", ex.Message);
            }
        }

        Partner partner;
        try
        {
            partner = new Partner(
                Guid.NewGuid(),
                command.Name,
                nip,
                regon,
                address);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Fail("validation.partner", ex.Message);
        }

        await partnerRepository.AddAsync(partner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(partner.Id);
    }
}
