using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraPortalB2B.Partners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPartnerContactColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partners' AND column_name = 'Phone'
    ) THEN
        EXECUTE 'ALTER TABLE partners RENAME COLUMN "Phone" TO phone';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partners' AND column_name = 'Notes'
    ) THEN
        EXECUTE 'ALTER TABLE partners RENAME COLUMN "Notes" TO notes';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partner_users' AND column_name = 'Phone'
    ) THEN
        EXECUTE 'ALTER TABLE partner_users RENAME COLUMN "Phone" TO phone';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partner_users' AND column_name = 'Notes'
    ) THEN
        EXECUTE 'ALTER TABLE partner_users RENAME COLUMN "Notes" TO notes';
    END IF;
END $$;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partners' AND column_name = 'phone'
    ) THEN
        EXECUTE 'ALTER TABLE partners RENAME COLUMN "phone" TO "Phone"';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partners' AND column_name = 'notes'
    ) THEN
        EXECUTE 'ALTER TABLE partners RENAME COLUMN "notes" TO "Notes"';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partner_users' AND column_name = 'phone'
    ) THEN
        EXECUTE 'ALTER TABLE partner_users RENAME COLUMN "phone" TO "Phone"';
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partner_users' AND column_name = 'notes'
    ) THEN
        EXECUTE 'ALTER TABLE partner_users RENAME COLUMN "notes" TO "Notes"';
    END IF;
END $$;
""");
        }
    }
}
