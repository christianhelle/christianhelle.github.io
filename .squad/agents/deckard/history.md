# Deckard — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Stack: Jekyll, Ruby 3.2+, Bundler, .NET 8 Playwright tests, custom Minima dark fork
- Posts live in `_posts/YYYY/YYYY-MM-DD-title.md`
- Dev server: `bundle exec jekyll serve --incremental` → port 4000
- Configs: `_config_dev.yml` (dev) / `_config_prod.yml` (prod)
- Deployment: push to `master` → GitHub Actions → GitHub Pages

## Learnings

### 2026-03-17: chlogr Post Structure Brief

Produced 13-section structural plan for chlogr blog post with comprehensive must-cover implementation details list. Plan established narrative arc (CLI → Models → Token → HTTP → API → Generation → Formatting → Testing → Usage → Distribution), extracted reference post pattern from clocz and Argiope, identified unique angles (first HTTP I/O project, REST API, process spawning), and created consistency checklist for Rachael.

## Team Updates (2026-03-17)

**Deckard (Research & Planning):** Completed chlogr structure brief with 13 sections, must-cover implementation details, narrative arc, and consistency checklist. Key technical insights: token fallback chain, deep-copy-then-deinit JSON pattern, label-based categorization, std.http.Client usage, standalone executable testing pattern. Unique hook identified: chlogr is first Zig project in series with real HTTP I/O and REST API integration.

**Pris (UI/Layout Dev):** Completed Jekyll conventions guidance by cross-referencing Argiope and clocz posts. Deliverables: standardized front matter template with redirect_from aliases, file naming convention, heading rhythm (2–3 intro + H2 sections), code fence conventions with language hints, link formatting guidelines, style guidance (developer diary tone, code-backed explanations). Established Argiope-style front matter as newer baseline.

**Roy (Validation & Testing):** Completed validation briefing flagging November 2025 vs March 2026 timeline drift. Earliest authored commits (2025-11-13) vs public repo creation (2026-03-15). Identified March 2026 changes to be explicitly framed as later evolution: chlogr rename, CLI consolidation, curl → std.http.Client, snapcraft packaging. Validation consequence: article should use November 2025 baseline with optional "now called chlogr" framing.

**Rachael (Content Dev):** Completed first draft of blog post with 467 lines, 13 sections, and comprehensive code examples. Post committed to f110feb with detailed message. Ready for team review and publication workflow.

### 2026-03-17: chlogr Post Final Review — REJECTED

Performed final review gate on the revised blog post. Historical accuracy verified against November 2025 commit history (all 4 key claims confirmed: "changelog-generator" naming, separate --owner/--repo flags, curl-based HTTP, required token with no anonymous fallback). Technical depth, tone, Jekyll conventions, and front matter all passed.

**Rejected for structural deviations from reference posts:**
1. Missing "How it works" overview section (present in both Argiope and clocz)
2. Missing "Distribution" section (present in both reference posts — signature Zig series element)
3. Four non-standard standalone sections (Motivation, Key Design Decisions, Lessons Learned, Limitations) that reference posts embed in intro/conclusion
4. Thin conclusion compared to reference post pattern

Assigned **Pris** for structural revision (Rachael locked out per reviewer rules). Detailed revision instructions written to decision inbox. Code examples and historical claims verified correct — no technical changes needed, only section reorganization to match established Zig series structure.

**Key learning:** When reviewing for style match, section structure alignment matters as much as tone and voice. The reference posts have a consistent architectural pattern (intro → How it works → implementation → Usage → Distribution → Conclusion) that defines the series identity.

### 2026-03-17: chlogr Post Correction Pass — Completed

Took over the correction pass after both reviewers rejected and Rachael was locked out. Verified every code example line-by-line against the November 2025 baseline (commit `4976f54`). Addressed all structural deviations (added How it works and Distribution, folded four standalone sections into intro/conclusion) and all historical inaccuracies (issues not in shipped flow, curl runtime dependency, "Other" fallback, no date filtering, wrong struct fields, hardcoded release links). Found additional inaccuracies not flagged by reviewers: `ResolvedToken` used wrong field names, `HttpClient` was wrapped in a fabricated `HttpResponse` struct, and the `generate()` function included invented date-filtering logic. Jekyll build verified. Committed as `75cf692`.

**Key learning:** When correcting a source-backed blog post, always verify code examples against the actual commit diff—not just behavioral claims. Draft authors may "improve" code examples to tell a cleaner story, introducing subtle inaccuracies that reviewers focused on behavioral claims will miss. The Lead's correction pass should diff every code block against `git show <commit> -- <file>`.

## Cycle Update (2026-03-17)

**Review Cycle Complete — Structural Reassignment:**
Post escalated from Roy's factual accuracy concerns to Deckard's final review. Deckard verified all code and historical claims correct against November 2025 baseline. However, post structure diverges from established Zig series pattern (Argiope/clocz reference architecture). 

**Reassigned to Pris** for structural reorganization (missing "How it works" and "Distribution" sections, four standalone sections needing to fold into intro/conclusion, thinner conclusion). Rachael locked out per reviewer lockout rules — cannot author fix to own reviewed revision. Detailed section-by-section reorganization instructions provided to Pris in decision brief.
