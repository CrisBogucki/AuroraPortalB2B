#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
coverage_dir="$root_dir/coverage"
report_dir="$root_dir/coverage-report"

mkdir -p "$coverage_dir"

dotnet test "$root_dir/AuroraPortalB2B.slnx" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$coverage_dir"

if command -v reportgenerator >/dev/null 2>&1; then
  reportgenerator \
    -reports:"$coverage_dir/**/coverage.cobertura.xml" \
    -targetdir:"$report_dir" \
    -reporttypes:Html

  echo "Coverage report: $report_dir/index.html"
else
  cat <<'EOF'
reportgenerator not found. Install it with:
  dotnet tool install -g dotnet-reportgenerator-globaltool
EOF
fi
