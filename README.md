# AuroraPortalB2B

Backend solution for the Aurora B2B portal.

## Structure
- `src/` application and modules
- `tests/` unit and integration tests
- `deploy/` local Docker compose files
- `docs/` project documentation
- `scripts/` helper scripts

## Build
```sh
dotnet build
```

## Test
```sh
./scripts/test.sh
```

## Changelog
This repo uses `git-cliff` for generating `CHANGELOG.md` from Conventional Commits.
```sh
./scripts/changelog.sh
```

Install `git-cliff`:
```sh
brew install git-cliff
```
or
```sh
cargo install git-cliff
```

## Release (v0.0.1 example)
1. Generate changelog:
```sh
./scripts/changelog.sh
```
2. Commit changelog:
```sh
git add CHANGELOG.md
git commit -m "chore: update changelog"
```
3. Create tag:
```sh
git tag v0.0.1
```
4. Push tag (optional):
```sh
git push --tags
```

## Run (local)
Keycloak runs in a separate compose file but uses the same shared network.

1. Start Keycloak:
```sh
docker compose -f deploy/docker-compose.keycloak.yml up -d
```

2. Build and run API + partners DB:
```sh
docker compose -f deploy/docker-compose.local.yml up -d --build
```

The database schema is initialized from `deploy/partners.sql` on first run. If you already have the volume and need a fresh schema, recreate it:
```sh
docker compose -f deploy/docker-compose.local.yml down -v
docker compose -f deploy/docker-compose.local.yml up -d --build
```

If needed, stop and remove local containers:
```sh
docker compose -f deploy/docker-compose.local.yml down -v
docker compose -f deploy/docker-compose.keycloak.yml down -v
```

Then run the host:
```sh
dotnet run --project src/AuroraPortalB2B.Host
```

Swagger is available at `/swagger` when running in Development.

Soft delete: `DELETE` endpoints do not remove records; they set status to `Inactive`.

The application applies EF Core migrations on startup for the Partners module.

## Migrations (EF Core)
Partners migrations live in `src/modules/partners/AuroraPortalB2B.Partners.Infrastructure`.

Add a migration:
```sh
dotnet ef migrations add <Name> \
  --project src/modules/partners/AuroraPortalB2B.Partners.Infrastructure \
  --output-dir Migrations \
  --no-build
```

Apply migrations to a database:
```sh
PARTNERS_CONNECTION_STRING="Host=localhost;Port=5432;Database=aurora_partners;Username=postgres;Password=postgres" \
dotnet ef database update \
  --project src/modules/partners/AuroraPortalB2B.Partners.Infrastructure \
  --no-build
```

## Authentication (Keycloak)
All API requests require a Bearer token.

Configure Keycloak in:
- `src/AuroraPortalB2B.Host/appsettings.json`
- `src/AuroraPortalB2B.Host/appsettings.Development.json`

Example values:
```
Keycloak:Authority = http://localhost:8080/realms/aurora
Keycloak:Audience = aurora-b2b
```

### Keycloak (local)
Keycloak runs in `deploy/docker-compose.keycloak.yml`:
- Admin UI: `http://localhost:8080`
- Admin user: `admin` / `admin`

#### Realm import (auto)
On startup, Keycloak imports:
- realm: `aurora`
- client: `aurora-b2b` (public, direct access grants enabled)
- user: `test-user` / `test-password`

`test-user` must have required actions cleared and profile fields set (first/last name) to avoid `Account is not fully set up`.

#### Get a token (password grant)
```sh
curl -X POST \\
  "http://localhost:8080/realms/aurora/protocol/openid-connect/token" \\
  -H "Content-Type: application/x-www-form-urlencoded" \\
  -d "client_id=aurora-b2b" \\
  -d "grant_type=password" \\
  -d "username=test-user" \\
  -d "password=test-password"
```

If you still get `Account is not fully set up`, remove volumes and reimport:
```sh
docker compose -f deploy/docker-compose.keycloak.yml down -v
docker compose -f deploy/docker-compose.keycloak.yml up -d
```

If you see `HTTPS required`, reimport the realm (it sets `sslRequired` to `NONE`):
```sh
docker compose -f deploy/docker-compose.keycloak.yml down -v
docker compose -f deploy/docker-compose.keycloak.yml up -d
```

If the Keycloak admin UI still shows **HTTPS required**, the master realm may enforce HTTPS.
Disable it with:
```sh
docker compose -f deploy/docker-compose.keycloak.yml exec -T aurora_portal_b2b_keycloak \
  /opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 --realm master --user admin --password admin

docker compose -f deploy/docker-compose.keycloak.yml exec -T aurora_portal_b2b_keycloak \
  /opt/keycloak/bin/kcadm.sh update realms/master -s sslRequired=NONE
```

Use the `access_token` from the response:
```sh
curl -X GET \\
  "http://localhost:5248/api/v1/partners" \\
  -H "Authorization: Bearer <token>"
```
