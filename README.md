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

## Run (local)
Start infrastructure from `deploy/`, then run the host project:
```sh
docker compose -f deploy/docker-compose.local.yml up -d
```

The database schema is initialized from `deploy/partners.sql` on first run. If you already have the volume and need a fresh schema, recreate it:
```sh
docker compose -f deploy/docker-compose.local.yml down -v
docker compose -f deploy/docker-compose.local.yml up -d
```

If needed, stop and remove local containers:
```sh
docker compose -f deploy/docker-compose.local.yml down
```

Then run the host:
```sh
dotnet run --project src/AuroraPortalB2B.Host
```

Swagger is available at `/swagger` when running in Development.
