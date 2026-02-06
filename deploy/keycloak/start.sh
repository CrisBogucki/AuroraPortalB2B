#!/usr/bin/env sh
set -euo pipefail

HTTP_PORT="${PORT:-8080}"

exec /opt/keycloak/bin/kc.sh start-dev \
  --import-realm \
  --http-port "${HTTP_PORT}" \
  --hostname-strict=false
