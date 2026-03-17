# Roy — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Tests: .NET 8 Playwright project at `tests/playwright/`, solution `blog.sln`
- Run tests: `cd tests/playwright && dotnet test`
- Tests require Jekyll dev server at http://127.0.0.1:4000/
- CI: GitHub Actions runs link checker after deploy
- Target framework: net8.0

## Core Context Summary
**Summarized from prior learnings (2026-03-06 to 2026-03-17):**
Testing and validation methodology established across projects page refresh, X share rebrand, and chlogr blog post approval cycle. Key insights: (1) Fast validation flow uses `bundle exec jekyll build` + `bundle exec jekyll serve --incremental` + targeted Playwright tests; (2) Archive test locators must use exact accessible-name matching due to prefix collisions; (3) Share UI regression lives in `ShareUiTests.cs` with path synchronization required; (4) chlogr required source-backed validation against November 2025 baseline (4976f54) vs current master for API behavior, release link formatting, and pagination claims; (5) A green Jekyll build only clears rendering—historical claims must be verified against actual code paths; (6) `state=closed` fetches both merged and unmerged PRs with `merged_at` field preserved; (7) Later-version claims in blog posts cannot reference unresolved limitations without contradicting current source/README evidence.

## Cycle Update (2026-03-17)

**chlogr Post Validation Escalation:**
Completed final validation of Rachael's revision. Findings: Jekyll build passed; rendered HTML structurally sound. Factual review against November 2025 commit baseline identified 4 remaining issues:
1. Issues described as generated but not wired through main application flow
2. "Zero external dependencies" contradicts curl runtime requirement
3. Multiple features (merge-date grouping, date filtering, "Merged PRs" default) not in November 2025 code
4. Release link formatting uses hardcoded placeholder not actual URL generation

**Decision:** Reject for factual accuracy. Escalate to Deckard for source-backed correction pass. Rachael locked out per reviewer lockout protocol (authored and revised; cannot fix own reviewed draft).

**Final Outcome:** Deckard completed correction pass successfully. Fixed 8 historical inaccuracies and 5 structural deviations. All code verified against November 2025 baseline (4976f54). Jekyll build validated. Post approved and committed as 75cf692.

## Orchestration (2026-03-06T12-38-16Z)

**Task:** Refine X share logo  
**Status:** ✅ Complete  
**Deliverables:** Updated Playwright expectation for refined SVG path. Applied `Exact = true` to archive link role locators. Confirmed Jekyll build plus full Playwright suite passed. Documented pre-existing timeout (unrelated to X icon work).  
**Decision:** Use monochrome, official-style X mark in `_includes/social/twitter.svg` with white fill styling in `_includes/share.html` for dark theme consistency.

## Team Updates (2026-03-17)

**Roy (Validation & Testing):** Completed validation briefing flagging November 2025 vs March 2026 timeline drift. Earliest authored commits (2025-11-13) vs public repo creation (2026-03-15). Identified March 2026 changes to be explicitly framed as later evolution: chlogr rename, CLI consolidation, curl → std.http.Client, snapcraft packaging. Validation consequence: article should use November 2025 baseline with optional "now called chlogr" framing.

**Deckard (Research & Planning):** Completed chlogr structure brief with 13 sections, must-cover implementation details, narrative arc, and consistency checklist. Key technical insights: token fallback chain, deep-copy-then-deinit JSON pattern, label-based categorization, std.http.Client usage, standalone executable testing pattern. Unique hook identified: chlogr is first Zig project in series with real HTTP I/O and REST API integration.

**Pris (UI/Layout Dev):** Completed Jekyll conventions guidance by cross-referencing Argiope and clocz posts. Deliverables: standardized front matter template with redirect_from aliases, file naming convention, heading rhythm (2–3 intro + H2 sections), code fence conventions with language hints, link formatting guidelines, style guidance (developer diary tone, code-backed explanations). Established Argiope-style front matter as newer baseline.

**Rachael (Content Dev):** Completed first draft of blog post with 467 lines, 13 sections, and comprehensive code examples. Post committed to f110feb with detailed message. Ready for team review and publication workflow.

## Final Approval Check (2026-03-17)

**Outcome:** REJECT

Jekyll build passed and the rendered post output is structurally clean. Remaining blockers are factual: the post still claims later versions fixed release-link formatting and pagination, but current `christianhelle/chlogr` still hardcodes `owner/repo` release links and still documents no pagination support. The intro and API section also continue to describe the November 2025 implementation as merged-PR based even though the baseline request is `state=closed` without a `merged_at` filter.

### 2026-03-17: chlogr Reassignment Correction

- When reviewer lockout removes both the original content owner and the lead reviewer, the next reassignment should go to the nearest eligible documentation owner. For this repo, that is Scribe for doc-only wording fixes—not Pris, whose charter excludes blog post content.
- Future-state claims in blog reviews must be verified against the current repository too, not just the historical baseline. For chlogr specifically, release-link formatting and pagination still cannot be described as "fixed later" without contradicting current source and README evidence.

## Final Signoff Re-check (2026-03-17)

- Commit `7283b68` fixed two of the previously flagged wording problems: the intro now says the 2025 tool categorized **closed pull requests**, and the API note now explicitly explains that `state=closed` returns both merged and unmerged PRs while preserving `merged_at`.
- One API-section summary sentence can still reintroduce the old inaccuracy if left untouched: saying the client constructs endpoints for "merged pull requests" overstates the November 2025 behavior when the actual request remains `pulls?state=closed`.
- Final factual signoff must also reject vague "later versions addressed some of these limitations" phrasing unless each referenced limitation is verified. Current `chlogr` still hardcodes `owner/repo` release links in `src/markdown_formatter.zig`, and `README.md` still lists `No Pagination` as a known limitation.

## Decisive Approval (2026-03-17)

- Commit `51436b8` resolved the last open blockers by changing the API summary to **closed pull requests** and removing the unsupported sentence that implied later versions had addressed still-open limitations.
- Final approval is now source-backed in both directions: the November 2025 baseline still shows `pulls?state=closed` in `src/github_api.zig`, while current `chlogr` sources confirm the post's remaining future-state claims (`src/http_client.zig` uses `std.http.Client`, `src/changelog_generator.zig` adds date-based unreleased/release filtering, and the combined `--repo owner/repo` flow is reflected in current usage/docs).
- The still-open limitations are also accurately preserved in the article: current `src/markdown_formatter.zig` still hardcodes `owner/repo` release links, and current `README.md` still documents **No Pagination**.
- `bundle exec jekyll build --config _config_dev.yml` passed, so the post is now both factually clean and build-clean.
- Outcome: **APPROVE**.

## Team Updates (2026-03-17 Final Wording & Signoff)

**Scribe (Documentation - Wording Corrections):** Performed minimal wording-only correction pass (commit 7283b68) to align post prose with verified code behavior. Four edits: (1) opening now says "closed pull requests" instead of "merged PRs"; (2) API section clarified `state=closed` returns both merged and unmerged PRs while preserving `merged_at` field; (3) removed false claim about formatter being updated in later versions; (4) softened "later versions addressed some limitations" language. Verified against November 2025 baseline (4976f54) and current master. No architectural changes required.

**Roy (Validation & Testing - Final Signoff Recheck):** Upon Scribe's completion (commit 51436b8), rechecked all previously flagged blockers. API integration summary now correctly says "closed pull requests" matching baseline. Unsupported conclusion wording removed. Future-state claims verified as source-backed in current repository: `src/http_client.zig` uses `std.http.Client`, `src/changelog_generator.zig` implements date-based filtering, combined `--repo owner/repo` interface in current docs. Persistent limitations accurately documented: `src/markdown_formatter.zig` still hardcodes release links, `README.md` still lists "No Pagination". Jekyll build clean. Final decision: **APPROVE**.