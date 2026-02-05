using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraPortalB2B.Partners.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparatePartnerOwnedTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partner_addresses",
                columns: table => new
                {
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_addresses", x => x.PartnerId);
                    table.ForeignKey(
                        name: "FK_partner_addresses_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partner_regons",
                columns: table => new
                {
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    regon = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_regons", x => x.PartnerId);
                    table.ForeignKey(
                        name: "FK_partner_regons_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO partner_addresses ("PartnerId", "country_code", "city", "street", "postal_code")
                SELECT "Id", "country_code", "city", "street", "postal_code"
                FROM partners
                WHERE "country_code" IS NOT NULL
                   OR "city" IS NOT NULL
                   OR "street" IS NOT NULL
                   OR "postal_code" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                INSERT INTO partner_regons ("PartnerId", "regon")
                SELECT "Id", "regon"
                FROM partners
                WHERE "regon" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "city",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "regon",
                table: "partners");

            migrationBuilder.DropColumn(
                name: "street",
                table: "partners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "partners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "partners",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                table: "partners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "regon",
                table: "partners",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "partners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE partners
                SET "country_code" = a."country_code",
                    "city" = a."city",
                    "street" = a."street",
                    "postal_code" = a."postal_code"
                FROM partner_addresses a
                WHERE partners."Id" = a."PartnerId";
                """);

            migrationBuilder.Sql("""
                UPDATE partners
                SET "regon" = r."regon"
                FROM partner_regons r
                WHERE partners."Id" = r."PartnerId";
                """);

            migrationBuilder.DropTable(
                name: "partner_addresses");

            migrationBuilder.DropTable(
                name: "partner_regons");
        }
    }
}
