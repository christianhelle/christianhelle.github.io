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

### March 18, 2026 (Scribe Orchestration): Cabazure.Kusto Session Completed
- **Scope:** Finalized orchestration for "Azure Kusto with Cabazure" blog post planning and team coordination
- **Actions:** Created orchestration logs for all agents (Deckard, Rachael, Pris), session summary, merged inbox decisions into decisions.md, updated agent histories
- **Deliverables:** `.squad/orchestration-log/2026-03-18T22-18-13Z-{deckard,rachael,pris}.md`, `.squad/log/2026-03-18T22-18-13Z-azure-kusto-with-cabazure.md`, consolidated decision entry
- **Key learning:** Orchestration logs serve as real-time activity records; session logs provide brief summaries of team coordination; inbox merging deduplicates decision trail while preserving learnings
- **Attribution:** All agents' work documented with proper timestamps and status tracking for future reference

### March 18, 2026: Cabazure.Kusto Blog Post Brief
- **Scope:** Planned publishing brief for "Azure Kusto with Cabazure" — a public library documentation post by @rickykaare
- **Key decision:** Date post 3 months ago (2025-12-18), matching Christian's established Cabazure post style (Azure Messaging 2025-08-18)
- **Safety boundary:** Explicitly separated public Cabazure.Kusto surface (README, samples) from internal Teal usage to prevent info leak
- **Attribution rule applied:** From decisions.md governance — phrase as "Cabazure.Kusto, created by @rickykaare" not "I built" to respect original author
- **Structure delivered:** 9-section narrative (intro → abstractions → DI → query definition → execution → pagination → patterns → design insights → conclusion)
- **Code tier strategy:** 6–8 examples (Tier 1 essential, Tier 2 optional) all sourced from public repo, ContosoSales sample dataset
- **Commit plan:** 4–6 logical chunks (draft → core → examples → polish → validate) for detailed progress history
- **Handoff notes:** Brief stored at `.squad/decisions/inbox/deckard-kusto-post-brief.md` for Rachael (writer), Roy (validation), Pris (README)
- **Tone ref:** Match Cabazure.Messaging (problem-first, code-heavy) + Argiope (technical depth, algorithms)

### March 17, 2026: Chlogr Blog Post Brief
- **Analysis:** Examined chlogr repository architecture across 8 core modules (CLI, token resolution, API client, changelog generation, markdown formatting, models, testing, build)
- **Pattern Discovered:** Zig zero-dependency philosophy (no external HTTP library; mock data for now) mirrors clocz approach; both posts follow "problem → architecture → algorithm → code → lessons" narrative
- **Decision:** Drafted comprehensive writing brief with 11 major sections, ranked code examples by impact, identified 6 risky claims to avoid, and created 12-commit strategy for incremental narrative delivery
- **Key Insight:** chlogr's token resolver demonstrates Zig ownership patterns beautifully; changelog generator's label categorization and two-phase generation (releases + unreleased) are algorithm showcases; GitHub API integration limitation (mock data) must be front-and-center, not buried
- **Handoff:** Brief stored at `.squad/decisions/inbox/deckard-chlogr-post-brief.md` for Rachael (writer) to follow; 12 focused commits planned for detailed history trail

### January 14, 2025: Chlogr Blog Post Review (APPROVED)
- **Scope:** Full post review against chlogr source, reference posts (clocz/argiope), and Jekyll conventions
- **Code Verification:** All 8 code examples cross-referenced against source files—100% accuracy on main.zig, cli.zig, token_resolver.zig, github_api.zig, changelog_generator.zig, markdown_formatter.zig, build.zig, install.sh
- **Factual Check:** Known limitations accurately stated (HTTP mock testing, fixed pagination, unimplemented date range, no caching); no unsupported claims detected
- **Style Consistency:** Front matter correct; narrative structure matches clocz/argiope pattern (intro → how-it-works → deep dives → usage → limitations → conclusion); tag consistency (Zig, CLI)
- **Completeness:** 634-line post covers 11 major sections across orchestration, CLI, token resolution, API, generation logic, formatting, build, distribution, limitations, and conclusion
- **Commits:** Focused logical chunks observed in git history; detailed revision trail preserved
- **Verdict:** ✅ **APPROVED** — Ready for publication. No material issues. Minor polish opportunities noted (verify GitHub Actions binaries, add caveat for unimplemented flags, confirm platform binary naming)
- **Documentation:** Full review decision written to `.squad/decisions/inbox/deckard-chlogr-review.md`

## Team Updates (2026-03-17)

**Orchestration (2026-03-17T12:45:00Z):** Lead writing brief  
**Orchestration (2026-03-17T13:08:00Z):** Lead review gate  
**Deckard Summary:** Coordinated chlogr blog post from brief through approval. Writing brief established 11-section roadmap with Tier-1 code examples ranked by impact. Review gate verified all 8 code examples against source (100% accuracy), confirmed style consistency with clocz/argiope pattern, approved post for immediate publication with no material blockers.

### March 18, 2026: Cabazure.Kusto Blog Post Review (APPROVED)
- **Scope:** Full post review of `_posts/2025/2025-12-18-azure-kusto-with-cabazure.md` against public source, reference posts, and Jekyll conventions
- **Code Verification:** All ~10 code examples cross-referenced against public Cabazure.Kusto repo (SampleApi/Program.cs, Queries/, Contracts/, IKustoProcessor.cs, PagedResult.cs)—100% accuracy
- **Attribution Check:** Correctly credits @rickykaare as library author; no "I built/created" claims about the library; complies with Third-Party Library Documentation Attribution governance
- **Public-Only Compliance:** No Teal internals referenced; all examples from public GitHub repo and ContosoSales sample database
- **Style Consistency:** Matches Cabazure.Messaging post pattern (problem-first → "That's why I use [Library]—written by @rickykaare" → code-heavy sections → conclusion)
- **Front Matter:** Layout, title, date (2025-12-18), author, tags (Azure, Kusto, .NET), redirect_from patterns—all correct
- **README Fix:** Kusto entry (2025-12) was listed after HTTP File Runner (2025-10); fixed to correct reverse-chronological order
- **Validation:** `bundle exec jekyll build` ✅ clean
- **Verdict:** ✅ APPROVED — No material issues. README ordering fix applied directly.
- **Documentation:** Review decision written to `.squad/decisions/inbox/deckard-kusto-gate.md`

**Team Context:**
- Rachael (13:02 UTC): Drafted comprehensive 634-line post with verified code examples from source. Commits 5fdafad (draft), b87af59 (polish).
- Roy (13:05, 13:14 UTC): Pre-draft validation identified 8 verified claims and 8 caveats. Final sweep: `bundle exec jekyll build` ✅, `dotnet build` ✅, smoke checks ✅.
- Pris (13:12 UTC): Updated README.md with new post entry (commit ff8e640).
- Deckard (13:08 UTC): Review gate APPROVED; all quality gates met.

### March 18, 2026 (Late): Cabazure.Kusto Publication Complete
- **Scope:** Final validation and publication of "Azure Kusto with Cabazure" blog post
- **Roy's Final Validation:** Code examples verified (12/12 accurate), `jekyll-redirect-from` enabled in all config files, `bundle exec jekyll build` ✅ clean, full Playwright test suite passing (3/3 tests)
- **Status:** ✅ Ready for publication to master branch
- **Decision:** Post approved for immediate merge; all quality gates satisfied
- **Key Learning:** Redirect aliases require `jekyll-redirect-from` plugin enabled in all config files, not just front matter

### March 19, 2026: Multi-Model Blog Post Evaluation Strategy — Atc.Test Article Selection
- **Scope:** Analyzed six blog post variants (July 2025 dated) for "Atc.Test - Unit testing for .NET with A Touch of Class" to determine which model/version best aligns with Christian's blog voice and publication standards
- **Variants Evaluated:**
  - 2025-07-01: Free/early model; 416 lines; focused, crisp voice; 95.5/100 score
  - 2025-07-12: Mid-tier model; 833 lines; exhaustive, tutorial tone; 83/100 score
  - 2025-07-15: Premium model; 391 lines; structured, generic voice; 86.5/100 score
  - 2025-07-18: Premium model; 505 lines; comprehensive, formal tone; 80/100 score
  - 2025-07-20: Premium model; 142 lines; REJECTED (insufficient content, 42/100)
  - 2025-07-22: Free/early model; 375 lines; balanced, pragmatic voice; **94/100 score (WINNER)**
- **Selection Rubric:** Accuracy (30%), Completeness (25%), Voice Alignment (20%), Maintainability (15%), Signal-to-Noise (10%)
- **Decision:** Keep 2025-07-22; delete 07-01, 07-12, 07-15, 07-18, 07-20
- **Rationale:** 2025-07-22 leads on voice alignment (problem-first framing: "ceremony was not"), explicit use-case guidance, xUnit v3 justification depth, and exact-type reuse pattern pedagogy. Matches Christian's established tone (Cabazure.Messaging, Chlogr) better than alternatives
- **Execution:** Sequential three-phase approach: (1) Code accuracy + redirect audit (Deckard), (2) Build + Playwright validation (Roy) + README update (Pris), (3) Deletion + commit + final validation
- **Risks Identified:** Slug migration (/dotnet/ → /net/), jekyll-redirect-from plugin verification (critical), archive/tag page crawling (low), external backlink breakage (accepted SEO refresh)
- **Acceptance Criteria:** Code verification ✅, build clean ✅, Playwright tests pass ✅, README updated, plugin enabled, redirects verified
- **Documentation:** Full strategy written to `.squad/decisions/deckard-atc-post-selection-strategy.md`
- **Key Learning:** Multi-variant evaluation requires holistic signal-to-noise assessment; parallelizing variant evaluation is counterproductive (duplicates work, loses context). Sequential cross-check better. Problem-first framing is distinguishing feature of Christian's best posts; voice alignment matters as much as completeness for blog audience.
