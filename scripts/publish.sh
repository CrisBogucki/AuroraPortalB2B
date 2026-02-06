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

if ! git diff --quiet -- CHANGELOG.md; then
  git add CHANGELOG.md
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
