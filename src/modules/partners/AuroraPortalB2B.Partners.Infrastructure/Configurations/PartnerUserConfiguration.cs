using AuroraPortalB2B.Partners.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuroraPortalB2B.Partners.Infrastructure.Configurations;

public sealed class PartnerUserConfiguration : IEntityTypeConfiguration<PartnerUser>
{
    public void Configure(EntityTypeBuilder<PartnerUser> builder)
    {
        builder.ToTable("partner_users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.PartnerId)
            .IsRequired();

        builder.Property(user => user.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(user => user.KeycloakUserId)
            .IsUnique();

        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(user => user.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(user => user.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.OwnsOne(user => user.Email, owned =>
        {
            owned.Property(email => email.Value)
                .HasColumnName("email")
                .HasMaxLength(320)
                .IsRequired();
            owned.HasIndex(email => email.Value);
        });
    }
}
