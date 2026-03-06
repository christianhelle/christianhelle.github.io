# Squad Decisions

## Active Decisions

### Archive Playwright Locator Hardening
**Decided:** 2026-03-06  
**Owner:** Roy  
**Status:** Completed

Hardened `tests/playwright/BlogArchiveTests.cs` by using `Exact = true` on archive link role locators. The archive crawl was failing due to substring matching collisions when post titles share prefixes (e.g., "Danish Developer Conference 2012" and "Multi-platform Mobile Development"). Exact matching is a test-only fix that prevents future regressions without altering site content.

### Blog Post Series: Zig Projects
**Decided:** 2026-03-05  
**Owner:** Rachael  
**Status:** In progress

Continue documenting Zig projects with blog posts following established style conventions. Argiope web crawler post drafted as second entry in series after Zig CLOC post.

### Projects Page Refresh
**Decided:** 2026-03-06  
**Owner:** Rachael  
**Status:** Completed

Updated projects.md with 4 new non-fork, active repositories showcasing Christian's recent work: Argiope (web crawler in Zig), CLOCZ (line counter in Zig), Azure SDK for Zig, and Otaku (manga reader). All existing projects retained. Selection emphasized Zig ecosystem growth and practical developer value in portfolio presentation.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
