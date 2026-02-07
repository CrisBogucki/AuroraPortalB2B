#!/usr/bin/env sh
set -euo pipefail

if ! command -v git-cliff >/dev/null 2>&1; then
  echo "git-cliff is not installed. Install it first, then rerun." >&2
  exit 1
fi

if ! git diff --quiet || ! git diff --cached --quiet; then
  echo "Working tree is not clean. Commit or stash changes before running this script." >&2
  exit 1
fi

bumped_version="$(git-cliff --bumped-version --unreleased -c .git-cliff.toml 2>/dev/null || true)"
if [ -z "$bumped_version" ]; then
  echo "No unreleased changes to version. Aborting." >&2
  exit 1
fi

normalized_version="${bumped_version#v}"

git-cliff -c .git-cliff.toml -o CHANGELOG.md
if grep -q "^## LATEST$" CHANGELOG.md; then
  sed -i '' "s/^## LATEST$/## v${normalized_version}/" CHANGELOG.md
fi

python3 - "$normalized_version" <<'PY'
import pathlib
import re
import sys

version = sys.argv[1]
csproj = pathlib.Path("src/AuroraPortalB2B.Host/AuroraPortalB2B.Host.csproj")
text = csproj.read_text()

def set_prop(content, name, value):
    pattern = re.compile(rf"<{name}>.*?</{name}>", re.DOTALL)
    replacement = f"<{name}>{value}</{name}>"
    if pattern.search(content):
        return pattern.sub(replacement, content, count=1)
    return re.sub(r"(<PropertyGroup>\s*)", r"\1    " + replacement + "\n", content, count=1)

text = set_prop(text, "Version", version)
text = set_prop(text, "AssemblyVersion", f"{version}.0")
text = set_prop(text, "FileVersion", f"{version}.0")
text = set_prop(text, "InformationalVersion", version)

csproj.write_text(text)
PY

if ! git diff --quiet -- CHANGELOG.md src/AuroraPortalB2B.Host/AuroraPortalB2B.Host.csproj; then
  git add CHANGELOG.md src/AuroraPortalB2B.Host/AuroraPortalB2B.Host.csproj
  git commit -m "chore: update changelog"
fi

tag="v${normalized_version}"
if git rev-parse -q --verify "refs/tags/$tag" >/dev/null; then
  echo "Tag $tag already exists. Aborting." >&2
  exit 1
fi

git tag "$tag"
git push
git push --tags
