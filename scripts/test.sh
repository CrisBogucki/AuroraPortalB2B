#!/usr/bin/env sh
set -euo pipefail

ROOT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"

cd "$ROOT_DIR"

dotnet test -m:1 --settings test.runsettings
