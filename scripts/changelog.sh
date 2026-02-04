#!/usr/bin/env sh
set -euo pipefail

if ! command -v git-cliff >/dev/null 2>&1; then
  echo "git-cliff is not installed. Install it first, then rerun." >&2
  exit 1
fi

git-cliff -c .git-cliff.toml -o CHANGELOG.md
