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

### Official-Style X Mark

- When refining the X share icon, use an official-style 24x24 monochrome path in `_includes/social/twitter.svg` instead of a generic crossed-lines glyph.
- Set the SVG `fill` to `currentColor`, then style the rendered icon from `_includes/share.html` so the dark theme can keep the X mark white without hardcoding presentation into the SVG.
- Preserve the existing `.twitter` CSS hook unless the whole share component is being renamed.
- If neighboring share icons render inside a larger common box, wrap the X path in a matching outer viewBox (for example 40x40) and center it there rather than changing the common CSS icon size.

### Validation

- Run `bundle exec jekyll build` after the include or SVG update to confirm Jekyll still renders the site successfully.
- Inspect a rendered post page in `_site/` (for example `_site/2022/10/autofaker.html`) to verify the share button still points at `twitter.com/intent/tweet` while exposing the updated X label and SVG.
- If the existing Playwright suite does not exercise share buttons, add a focused post-page check under `tests/playwright/` that locks the accessible label and expected share-link endpoint.
- If the X glyph path changes, update the expected SVG path in `tests/playwright/ShareUiTests.cs` to mirror `_includes/social/twitter.svg`; otherwise the share regression test will fail even when the rendered markup is correct.
- Add a rendered-size regression in `tests/playwright/ShareUiTests.cs` that compares the X, LinkedIn, and Facebook SVG bounding boxes on a representative post page so shared 40x40 icon sizing stays locked in.

## Anti-Patterns

- Changing a working social share endpoint just because the platform branding changed.
- Renaming CSS hooks or Liquid include paths unless the broader component is being refactored.

