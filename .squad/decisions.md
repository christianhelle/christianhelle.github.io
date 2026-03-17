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

### Blog Post Series: Zig Projects
**Decided:** 2026-03-05  
**Owner:** Deckard, Rachael, Roy  
**Status:** Completed

Continue documenting Zig projects with blog posts following established style conventions. Third entry "Building a GitHub Changelog Generator in Zig" (2026-03-17) completed following established style pattern. Comprehensive 634-line post with 11 sections, verified code examples, and detailed limitations documentation. Includes working brief, peer review gate, factual corrections pass, and final validation sweep.

### Projects Page Refresh
**Decided:** 2026-03-06  
**Owner:** Rachael  
**Status:** Completed

Updated projects.md with 4 new non-fork, active repositories showcasing Christian's recent work: Argiope (web crawler in Zig), CLOCZ (line counter in Zig), Azure SDK for Zig, and Otaku (manga reader). All existing projects retained. Selection emphasized Zig ecosystem growth and practical developer value in portfolio presentation.

### Chlogr Blog Post: GitHub Changelog Generator in Zig
**Decided:** 2026-03-17  
**Owner:** Deckard, Rachael, Roy, Pris  
**Status:** Completed

Published comprehensive blog post "Building a GitHub Changelog Generator in Zig" documenting the chlogr project. Post follows established Zig project documentation pattern with working brief, 11 must-have sections (problem statement through lessons learned), verified code examples, testing section, usage guide, and detailed limitations. Multi-pass workflow: writing brief → draft → validation → review gate → factual corrections → README update → final validation. All code examples grounded in actual source. Post commits: 5fdafad (draft), b87af59 (polish), ff8e640 (README). Final validation: jekyll build ✅, dotnet build ✅, smoke checks ✅.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
