CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE partners (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    nip character varying(10) NOT NULL,
    regon character varying(14),
    country_code character varying(2),
    city character varying(100),
    street character varying(200),
    postal_code character varying(20),
    "Status" integer NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_partners" PRIMARY KEY ("Id")
);

CREATE TABLE partner_users (
    "Id" uuid NOT NULL,
    "PartnerId" uuid NOT NULL,
    email character varying(320) NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_partner_users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_partner_users_partners_PartnerId" FOREIGN KEY ("PartnerId") REFERENCES partners ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_partner_users_email" ON partner_users (email);

CREATE INDEX "IX_partner_users_PartnerId" ON partner_users ("PartnerId");

CREATE UNIQUE INDEX "IX_partners_nip" ON partners (nip);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260204153857_InitialPartners', '9.0.1');

COMMIT;

