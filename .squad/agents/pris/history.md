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

### 2026-03-17: chlogr Post Jekyll Conventions

Applied cross-reference methodology to Argiope (newer pattern) and clocz (baseline) to extract standardized conventions: front matter template with layout/title/date/author/tags/redirect_from, file naming (`_posts/2025/2025-11-15-building-github-changelog-generator-zig.md`), heading rhythm (2–3 intro paras + H2 sections), code fence language hints, link formatting (inline + explicit repo callout), and style guidance. No separate `description:` front matter field; opening paragraphs act as excerpt. Provided comprehensive pitfall warnings.

## Team Updates (2026-03-17 Initial)

**Pris (UI/Layout Dev):** Completed Jekyll conventions guidance by cross-referencing Argiope and clocz posts. Deliverables: standardized front matter template with redirect_from aliases, file naming convention, heading rhythm (2–3 intro + H2 sections), code fence conventions with language hints, link formatting guidelines, style guidance (developer diary tone, code-backed explanations). Established Argiope-style front matter as newer baseline.

**Deckard (Research & Planning):** Completed chlogr structure brief with 13 sections, must-cover implementation details, narrative arc, and consistency checklist. Key technical insights: token fallback chain, deep-copy-then-deinit JSON pattern, label-based categorization, std.http.Client usage, standalone executable testing pattern. Unique hook identified: chlogr is first Zig project in series with real HTTP I/O and REST API integration.

**Roy (Validation & Testing):** Completed validation briefing flagging November 2025 vs March 2026 timeline drift. Earliest authored commits (2025-11-13) vs public repo creation (2026-03-15). Identified March 2026 changes to be explicitly framed as later evolution: chlogr rename, CLI consolidation, curl → std.http.Client, snapcraft packaging. Validation consequence: article should use November 2025 baseline with optional "now called chlogr" framing.

**Rachael (Content Dev):** Completed first draft of blog post with 467 lines, 13 sections, and comprehensive code examples. Post committed to f110feb with detailed message. Ready for team review and publication workflow.


### 2026-03-17: chlogr Final Approval Review

- Final structural approval for a Zig deep-dive post should verify the full narrative arc, not just heading counts: intro, `How it works`, implementation H2s, `Usage`, `Distribution`, and `Conclusion` is the series rhythm established by `clocz` and `Argiope`.
- Regex checks for headings are useful, but fenced code samples can legitimately contain `##` and `###` lines that are not real article headings. Always confirm suspicious matches against the rendered structure before rejecting a post.
- For this blog, publish readiness on the Jekyll side still reduces to sane front matter plus a clean `bundle exec jekyll build` with development config.

## Team Updates (2026-03-17 Final Approval)

**Pris (UI/Layout Dev - Structural Approval):** Final Jekyll/theme review approved. Front matter is publish-ready and aligned with established Zig series shape. Article follows same narrative rhythm as clocz and Argiope posts: intro paragraphs, "How it works" section, implementation-focused H2 sections, "Usage", "Distribution", "Conclusion". Post keeps series' code-backed explanation style. Apparent extra heading matches come from sample generated changelog inside fenced code, not actual article outline. Local Jekyll validation succeeded with development config. Post is structurally ready to publish without Jekyll/layout/styling changes.

**Deckard (Research & Planning - Correction Lead):** Completed correction pass after initial rejection. Verified all code examples line-by-line against November 2025 baseline (4976f54). Fixed 8 historical inaccuracies and 5 structural deviations. Added "How it works" and "Distribution" sections, folded standalone sections into intro/conclusion, corrected struct field names and removed fabricated HttpResponse wrapper. Jekyll build verified. Committed as 75cf692.

**Roy (Validation & Testing - Final Check):** Jekyll build and render passed. Identified remaining factual blockers: later-version release-link fix still false (current repo still hardcodes placeholder), later-version pagination fix still false (README still lists as known limitation), November 2025 implementation over-described as merged-only. Escalated to Deckard for source-backed correction pass. Rachael locked out per reviewer lockout protocol.
