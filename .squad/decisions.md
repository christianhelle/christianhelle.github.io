# Squad Decisions

## Active Decisions

### Share Button X Branding
**Decided:** 2026-03-06  
**Owner:** Pris, Roy  
**Status:** Completed

Modernized the blog post share button UI from Twitter bird to X icon. Updated SVG glyph and accessible label/title to "Share on X" while preserving the existing `twitter.com/intent/tweet` endpoint and all Liquid parameters. This is a pure UI rebrand with no functional change to the share flow. Added focused Playwright regression test coverage in `ShareUiTests.cs` to prevent future share UI regressions.

### Archive Playwright Locator Hardening
**Decided:** 2026-03-06  
**Owner:** Roy  
**Status:** Completed

Hardened `tests/playwright/BlogArchiveTests.cs` by using `Exact = true` on archive link role locators. The archive crawl was failing due to substring matching collisions when post titles share prefixes (e.g., "Danish Developer Conference 2012" and "Multi-platform Mobile Development"). Exact matching is a test-only fix that prevents future regressions without altering site content.

### Blog Post Series: Zig Projects — chlogr Article
**Decided:** 2025-11-15 (planned), 2026-03-17 (orchestrated)  
**Owner:** Deckard (plan), Pris (conventions), Roy (validation), Rachael (draft)  
**Status:** Draft complete (commit f110feb)

Comprehensive blog post "Building a GitHub Changelog Generator in Zig" documenting the chlogr project. Follows established Zig series pattern (clocz, Argiope) with 13 sections covering CLI design, token resolution, GitHub API integration, changelog generation, markdown formatting, testing strategy, usage examples, distribution, and design reflections. Post emphasizes unique angles: first project in series with real HTTP I/O, REST API integration, and external process spawning. Front matter includes `redirect_from` aliases per Argiope convention. File: `_posts/2025/2025-11-15-building-github-changelog-generator-zig.md`.

### Projects Page Refresh
**Decided:** 2026-03-06  
**Owner:** Rachael  
**Status:** Completed

Updated projects.md with 4 new non-fork, active repositories showcasing Christian's recent work: Argiope (web crawler in Zig), CLOCZ (line counter in Zig), Azure SDK for Zig, and Otaku (manga reader). All existing projects retained. Selection emphasized Zig ecosystem growth and practical developer value in portfolio presentation.

### chlogr Post Structure Brief
**Decided:** 2025-11-15 (research), 2026-03-17 (formalized)  
**Owner:** Deckard  
**Status:** Completed

13-section structural plan for chlogr blog post with must-cover implementation details: std.json.parseFromSlice with ignore_unknown_fields, deep-copy-then-deinit pattern, token fallback chain (flag → env → gh CLI), std.http.Client with headers, label-based categorization, unreleased changes detection, std.fmt.allocPrint for formatting, build.zig test pattern. Narrative arc: CLI → Models → Token Resolution → HTTP → API → Generation → Formatting → Testing → Usage → Distribution. Consistency checklist provided for content development.

### chlogr Post Jekyll Conventions
**Decided:** 2026-03-17  
**Owner:** Pris  
**Status:** Completed

Standardized Jekyll conventions extracted from Argiope and clocz reference posts. Front matter template: layout, title, date, author, tags, redirect_from. File naming: `_posts/2025/2025-11-15-building-github-changelog-generator-zig.md`. Heading rhythm: 2–3 intro paragraphs, mostly H2 sections, H3 only for genuine subsections. Code fences with language hints (zig, bash, powershell, yaml). Link formatting: inline Markdown with explicit repo callout. Style: developer diary tone, code-backed explanations throughout, no unnecessary front matter fields.

### chlogr Post Timeline Validation
**Decided:** 2026-03-17  
**Owner:** Roy  
**Status:** Proposed

For blog post dated 2025-11-15, use November 2025 commit history (earliest authored 2025-11-13) as baseline for technical claims and code snippets. Repository creation metadata (2026-03-15) differs from historical commit dates; treat March 2026 changes (rename to chlogr, CLI consolidation, std.http.Client migration, snapcraft packaging, unreleased changes feature) as later evolution. Article should frame current features explicitly if using modern naming for reader familiarity.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
