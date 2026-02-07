using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraPortalB2B.Partners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "partners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "partner_users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.CreateIndex(
                name: "IX_partners_tenant_id",
                table: "partners",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_partner_users_tenant_id",
                table: "partner_users",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_partners_tenant_id",
                table: "partners");

            migrationBuilder.DropIndex(
                name: "IX_partner_users_tenant_id",
                table: "partner_users");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "partner_users");
        }
    }
}
