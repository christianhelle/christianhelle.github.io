---
name: "share-branding"
description: "Safely refresh social-share branding in Jekyll includes"
domain: "jekyll-ui"
confidence: "high"
source: "repository"
---

## Context

Use this pattern when a social share button needs a branding refresh but its underlying share endpoint should remain stable.

## Patterns

### UI-Only Share Rebrand

- Keep the existing share URL and Liquid parameters intact when the endpoint is still working.
- Update the icon partial in `_includes/social/` and the visible/accessibility label in `_includes/share.html`.
- Prefer minimal HTML changes so layout, CSS hooks, and share behavior remain unchanged.

### Validation

- Run `bundle exec jekyll build` after the include or SVG update to confirm Jekyll still renders the site successfully.
- Inspect a rendered post page in `_site/` (for example `_site/2022/10/autofaker.html`) to verify the share button still points at `twitter.com/intent/tweet` while exposing the updated X label and SVG.
- If the existing Playwright suite does not exercise share buttons, add a focused post-page check under `tests/playwright/` that locks the accessible label and expected share-link endpoint.

## Anti-Patterns

- Changing a working social share endpoint just because the platform branding changed.
- Renaming CSS hooks or Liquid include paths unless the broader component is being refactored.
