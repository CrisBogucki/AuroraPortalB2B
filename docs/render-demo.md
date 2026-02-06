# Render Demo Deployment (API + Keycloak + Postgres)

This is a **demo-only** setup using Render Free plan. Services will sleep on inactivity.

## Files
- `render.yaml` defines the blueprint (API, Keycloak, and 2 Postgres DBs).
- `deploy/Dockerfile.keycloak` builds a Keycloak container with realm import.
- `deploy/keycloak/realm-aurora.json` is imported at startup.

## Deploy
1. Push this repo to GitHub.
2. In Render dashboard:
   - Create a new **Blueprint**.
   - Select this repo and deploy the `render.yaml`.
3. Wait for all services to become **Healthy**.

## URLs
- API: `https://aurora-b2b-api.onrender.com`
- Keycloak: `https://aurora-b2b-keycloak.onrender.com`

If Render changes the service URL (name already taken), update these values in `render.yaml`:
- `Keycloak__Authority`
- `Keycloak__MetadataAddress`
- `Keycloak__ValidIssuers__0`
- `KC_HOSTNAME_URL`

## Keycloak
Default admin (demo):
- user: `admin`
- pass: `admin`

Realm import:
- realm: `aurora`
- client: `aurora-b2b`

## API config
The API uses:
- `Keycloak__Authority` = `https://aurora-b2b-keycloak.onrender.com/realms/aurora`
- `Keycloak__MetadataAddress` = `https://aurora-b2b-keycloak.onrender.com/realms/aurora/.well-known/openid-configuration`

## Notes (Free Plan)
- Render Free services sleep after inactivity.
- Cold start can take ~30–60s.
