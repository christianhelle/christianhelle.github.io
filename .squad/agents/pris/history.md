# Pris — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Theme: Custom Minima fork with dark skin (https://github.com/christianhelle/minima)
- Layouts: `_layouts/`, Includes: `_includes/`, Assets: `assets/`
- Ruby deps managed with Bundler (`Gemfile`, `Gemfile.lock`)
- Plugins in `_plugins/`
- Test changes locally: `bundle exec jekyll serve --incremental`
- Never break the dark theme — it's central to the site identity

## Orchestration (2026-03-06T12-38-16Z)

**Task:** Refine X share logo  
**Status:** ✅ Complete  
**Deliverables:** Replaced Twitter bird SVG with official-style X mark. Preserved share URL behavior (`twitter.com/intent/tweet`) and accessibility labels ("Share on X"). Jekyll build passed. Visual validation passed on representative post page.  
**Decision:** Use monochrome, official-style X mark in `_includes/social/twitter.svg` with white fill styling in `_includes/share.html` for dark theme consistency.

## Team Updates (2026-03-06)

**Pris (UI/Layout Dev):** Completed share button X rebrand. Replaced Twitter bird SVG with X icon glyph. Updated accessible label and title to "Share on X". Preserved existing `twitter.com/intent/tweet` endpoint and Liquid parameters. Jekyll build passed, visual validation passed on representative post page.

**Roy (Test & Infra):** Added focused Playwright regression coverage in `ShareUiTests.cs` for share UI protection. Hardened archive test infrastructure by applying `Exact = true` to link role locators, preventing substring-match collisions on title prefixes. Pre-existing unrelated timeout remains in `Crawl_Archive` at Source Code Download step (not caused by share rebrand work).

**Rachael (Content Dev):** Completed projects page refresh with 4 new repositories. Strong Zig presence in updates (3 of 4 projects): Argiope web crawler, CLOCZ line counter, and Azure SDK for Zig. Otaku (manga reader) adds portfolio diversity. All existing projects preserved. Selection emphasized non-forks, active repos, creator-maintained, meaningful descriptions. Portfolio positioning emphasizes practical developer value.

## Learnings

- The share button behavior is owned by `_includes/share.html`, while the X/Twitter glyph itself lives in `_includes/social/twitter.svg`; branding fixes should keep the existing `twitter.com/intent/tweet` URL and Liquid parameters untouched.
- For this dark Minima theme, the X mark looks most on-brand when the SVG uses a 24x24 `currentColor` path and the share include sets the icon fill to white instead of legacy Twitter blue.
- `bundle exec jekyll build` is the existing Jekyll validation step, and `_site/2022/10/autofaker.html` is a reliable rendered page to spot-check share-link title, aria-label, endpoint, and inline SVG output.
