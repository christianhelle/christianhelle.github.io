# Roy — Atc.Test verification guardrails

## Context

The six July 2025 Atc.Test drafts do not behave like six independent pages at build time. Jekyll currently collapses them into two generated URLs under `_site\2025\07\` (`...-dotnet-...` and `...-net-...`) while `/archives` still lists all six publication dates.

## Decision

Treat **route-collision audit + redirect preservation** as a required gate before deleting any losing variant.

## Required checks

1. Decide the canonical surviving slug first (`dotnet` vs `net`).
2. Rebuild and confirm `_site\2025\07\` contains exactly the intended canonical HTML output plus any redirect pages.
3. Recheck `README.md`, `/archives`, `/tags`, feed/canonical metadata, and any `redirect_from` aliases against the surviving slug.
4. Do not rely on current Playwright coverage alone; the suite does not detect duplicate dated entries collapsing to one URL.

## Why this matters

Without this gate, deleting drafts can silently change which content is served at an existing URL, strand README links on the wrong slug family, or remove legacy aliases that readers and search engines may already follow.
