using AuroraPortalB2B.Partners.Domain.Aggregates;
using AuroraPortalB2B.Partners.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuroraPortalB2B.Partners.Infrastructure.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(partner => partner.Id);

        builder.Property(partner => partner.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(partner => partner.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(partner => partner.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(partner => partner.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(partner => partner.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(partner => partner.CreatedAtUtc)
            .IsRequired();

        builder.OwnsOne(partner => partner.Nip, owned =>
        {
            owned.Property(nip => nip.Value)
                .HasColumnName("nip")
                .HasMaxLength(10)
                .IsRequired();
            owned.HasIndex(nip => nip.Value).IsUnique();
        });

        builder.HasIndex(partner => partner.TenantId);

        builder.OwnsOne(partner => partner.Regon, owned =>
        {
            owned.ToTable("partner_regons");
            owned.WithOwner().HasForeignKey("PartnerId");
            owned.HasKey("PartnerId");
            owned.Property(regon => regon.Value)
                .HasColumnName("regon")
                .HasMaxLength(14)
                .IsRequired(false);
        });

        builder.OwnsOne(partner => partner.Address, owned =>
        {
            owned.ToTable("partner_addresses");
            owned.WithOwner().HasForeignKey("PartnerId");
            owned.HasKey("PartnerId");
            owned.Property(address => address.CountryCode)
                .HasColumnName("country_code")
                .HasMaxLength(2)
                .IsRequired(false);
            owned.Property(address => address.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired(false);
            owned.Property(address => address.Street)
                .HasColumnName("street")
                .HasMaxLength(200)
                .IsRequired(false);
            owned.Property(address => address.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20)
                .IsRequired(false);
        });

        builder.HasMany(partner => partner.Users)
            .WithOne()
            .HasForeignKey("PartnerId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(partner => partner.Users)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_users");

    }
}
