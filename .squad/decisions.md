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
**Decided:** 2025-11-15 (planned), 2026-03-17 (orchestrated/approved)  
**Owner:** Deckard (plan), Pris (conventions), Roy (validation), Rachael (draft), Deckard (correction lead)  
**Status:** Approved & published (commit 75cf692)

Comprehensive blog post "Building a GitHub Changelog Generator in Zig" documenting the chlogr project. Follows established Zig series pattern (clocz, Argiope) with 13 sections covering CLI design, token resolution, GitHub API integration, changelog generation, markdown formatting, testing strategy, usage examples, distribution, and design reflections. Post emphasizes unique angles: first project in series with real HTTP I/O, REST API integration, and external process spawning. Front matter includes `redirect_from` aliases per Argiope convention. Initial draft reviewed by Pris (approved structurally) and Roy (rejected for factual accuracy). Deckard performed source-backed correction pass, fixing 8 historical inaccuracies and 5 structural deviations. All code examples verified against November 2025 baseline (commit 4976f54). Jekyll build validated. File: `_posts/2025/2025-11-15-building-github-changelog-generator-zig.md`.

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
**Status:** Completed

For blog post dated 2025-11-15, use November 2025 commit history (earliest authored 2025-11-13) as baseline for technical claims and code snippets. Repository creation metadata (2026-03-15) differs from historical commit dates; treat March 2026 changes (rename to chlogr, CLI consolidation, std.http.Client migration, snapcraft packaging, unreleased changes feature) as later evolution. Article should frame current features explicitly if using modern naming for reader familiarity.

### chlogr Post Final Structural Approval
**Decided:** 2026-03-17  
**Owner:** Pris  
**Status:** Approved

Final Jekyll/theme review of `_posts/2025/2025-11-15-building-github-changelog-generator-zig.md` for series fit, heading rhythm, front matter, and overall publish readiness. Front matter is publish-ready and aligned with the established Zig series shape: `layout`, `title`, `date`, `author`, `tags`, and `redirect_from` are present and valid. Article follows the same narrative rhythm as clocz and Argiope posts: intro paragraphs, "How it works" section, implementation-focused H2 sections, "Usage", "Distribution", and "Conclusion". Post maintains series' code-backed explanation style throughout. Local Jekyll validation succeeded with development config. Post is structurally ready to publish without Jekyll, layout, or styling changes.

### chlogr Post Final Factual Approval
**Decided:** 2026-03-17  
**Owner:** Deckard (correction lead)  
**Status:** Approved

Correction pass on rejected post after initial review cycle identified structural and factual issues. Took over correction responsibility after both reviewers rejected and Rachael was locked out per protocol. Verified every code example line-by-line against November 2025 baseline (commit 4976f54). Addressed all 8 historical inaccuracies: removed issues from claims, fixed dependency story (curl runtime requirement documented), corrected fallback category from "Merged Pull Requests" to "Other", removed fabricated date-filtering logic, fixed struct field names, removed invented HttpResponse wrapper, documented hardcoded release link placeholder, fixed test scenario list. Addressed all 5 structural deviations: added "How it works" section after intro, added "Distribution" section before conclusion, folded standalone "Motivation" into intro, removed non-standard "Key Design Decisions"/"Lessons Learned"/"Limitations" sections, expanded conclusion to four comprehensive paragraphs. Jekyll build verified. All code blocks match source repository. Committed as 75cf692.

### chlogr Post: Final Wording Corrections
**Decided:** 2026-03-17  
**Owner:** Scribe  
**Status:** Completed

Minimal wording-only correction pass to align post prose with verified code behavior. Three factual overstatements corrected: (1) opening now says "closed pull requests" instead of "merged PRs" (API fetches `state=closed`); (2) API section clarified that `state=closed` returns both merged and unmerged PRs while preserving `merged_at` field; (3) removed false claim that formatter was updated in later versions to emit correct release links (still hardcoded in current master); (4) softened vague "later versions addressed limitations" language. Verified against November 2025 baseline (commit 4976f54) and current master branch: confirmed `state=closed` query, hardcoded release link placeholder, and no pagination implementation. No architectural changes. Committed as 7283b68.

### chlogr Post Final Signoff Recheck
**Decided:** 2026-03-17  
**Owner:** Roy  
**Status:** Escalated then Approved

Initial final signoff review (commit 7283b68) identified remaining factual blockers: API integration summary still used "merged pull requests" language despite `state=closed` baseline, and conclusion still claimed later versions addressed unresolved limitations. Escalated to Scribe for tightly scoped wording-only correction pass. Upon Scribe's completion (commit 51436b8), rechecked blockers and approved: API integration summary now correctly says "closed pull requests" matching baseline, unsupported conclusion wording removed, remaining future-state claims source-backed in current repository (`src/http_client.zig` uses `std.http.Client`, `src/changelog_generator.zig` adds unreleased/date filtering), and persistent limitations accurately documented (hardcoded release links, no pagination). Jekyll build clean. Final decision: **APPROVE**.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
