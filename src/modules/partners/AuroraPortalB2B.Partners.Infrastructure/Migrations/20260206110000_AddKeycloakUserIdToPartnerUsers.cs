using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraPortalB2B.Partners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKeycloakUserIdToPartnerUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'partner_users' AND column_name = 'keycloak_user_id'
    ) THEN
        ALTER TABLE partner_users
            ADD COLUMN keycloak_user_id character varying(200);
    END IF;
END $$;
""");

            migrationBuilder.Sql("""
UPDATE partner_users
SET keycloak_user_id = "Id"::text
WHERE keycloak_user_id IS NULL OR keycloak_user_id = '';
""");

            migrationBuilder.Sql("""
ALTER TABLE partner_users
    ALTER COLUMN keycloak_user_id SET NOT NULL;
""");

            migrationBuilder.Sql("""
CREATE UNIQUE INDEX IF NOT EXISTS "IX_partner_users_keycloak_user_id"
    ON partner_users (keycloak_user_id);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DROP INDEX IF EXISTS "IX_partner_users_keycloak_user_id";
""");

            migrationBuilder.Sql("""
ALTER TABLE partner_users
    DROP COLUMN IF EXISTS keycloak_user_id;
""");
        }
    }
}
