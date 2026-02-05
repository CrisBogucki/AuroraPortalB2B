using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraPortalB2B.Partners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "partners",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "partners",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "partner_users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "partner_users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notes",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "partner_users");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "partner_users");
        }
    }
}
