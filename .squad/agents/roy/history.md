# Roy — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Tests: .NET 8 Playwright project at `tests/playwright/`, solution `blog.sln`
- Run tests: `cd tests/playwright && dotnet test`
- Tests require Jekyll dev server at http://127.0.0.1:4000/
- CI: GitHub Actions runs link checker after deploy
- Target framework: net8.0

## Learnings
- Projects page refresh validation can be done quickly with `bundle exec jekyll build`, then a local `bundle exec jekyll serve --incremental` check of `http://127.0.0.1:4000/projects/` for rendered headings and nav links.
- Key validation paths: source content in `projects.md`, generated output in `_site/projects/index.html`, and browser regression coverage in `tests/playwright/BlogArchiveTests.cs`.
- Current test harness detail: `tests/playwright/run.ps1` still calls `bin/Debug/net6.0/playwright.ps1 install` even though `tests/playwright/PlaywrightTests.csproj` targets `net8.0`.
- Current repo blocker observed during validation: the Playwright archive crawl fails at `tests/playwright/BlogArchiveTests.cs:115` because `GetByRole(... Name = "Danish Developer Conference 2012")` now matches two archive links.
- Archive regression coverage lives in `tests/playwright/BlogArchiveTests.cs`, and link-title lookups on `/archives` should use exact accessible-name matching because multiple post titles can share prefixes like `Danish Developer Conference 2012` and `Multi-platform Mobile Development`.
- A fast green-path validation for archive regressions is: copy `_config_dev.yml` to `_config.yml`, run `bundle exec jekyll build`, confirm the dev server is serving `http://127.0.0.1:4000/`, then run `cd tests/playwright && dotnet test --filter "Crawl_Archive"`.
- Share rebrand validation touches `_includes/share.html` and `_includes/social/twitter.svg`, while rendered confirmation is easiest on a representative post such as `_site/2022/10/autofaker.html`.
- Focused regression coverage for the share UI now lives in `tests/playwright/ShareUiTests.cs`, asserting the X label, preserved Twitter intent endpoint, and rendered SVG path on a post page.
- Repo-wide Playwright is still not fully green during share validation because `Crawl_Archive` times out at the `Source Code Download` navigation step in `tests/playwright/BlogArchiveTests.cs:320`, which is unrelated to the X share button change.
- The refined X logo currently renders from `_includes/social/twitter.svg` as a 24x24 monochrome path, and the representative rendered check remains `_site/2022/10/autofaker.html` with `title`/`aria-label` set to `Share on X` while still linking to `twitter.com/intent/tweet`.
- Keep `tests/playwright/ShareUiTests.cs` synchronized with the SVG path in `_includes/social/twitter.svg`; once the expected path string matches the include, `bundle exec jekyll build` and the full `dotnet test .\tests\playwright\PlaywrightTests.csproj` suite are green again.

- Share icon size regression coverage now lives in `tests/playwright/ShareUiTests.cs`, where the post-page check verifies the X, LinkedIn, and Facebook SVGs all render as visible 40x40 icons on `http://127.0.0.1:4000/2022/10/autofaker.html`.
- For static Jekyll page crawls in `tests/playwright/BlogArchiveTests.cs`, waiting for `DOMContentLoaded` is more stable than waiting for the full `load` event because external resources can delay navigation without affecting route validation.
- Key files for share-size validation are `_includes/share.html`, `_includes/social/twitter.svg`, `tests/playwright/ShareUiTests.cs`, and `tests/playwright/BlogArchiveTests.cs`; the regression flow is `bundle exec jekyll build` plus `cd tests/playwright && dotnet test` with the dev server running at `http://127.0.0.1:4000/`.
- For the planned `chlogr` post dated `2025-11-15`, use the November 2025 commit range as the historical baseline: early authored commits start on `2025-11-13`, while the public GitHub repository metadata shows creation on `2026-03-15`, so build-date and public-repo timeline must be distinguished in validation.
- The November 2025 baseline used the `changelog-generator` name, separate `--owner` and `--repo` flags, and a curl-backed HTTP client; the current `chlogr` name, combined `--repo owner/repo` syntax, anonymous no-token fallback, `std.http.Client`, unreleased-changes output, and Snapcraft packaging were introduced later in March 2026.
- Source-backed accuracy risks for `chlogr`: `--since-tag` / `--until-tag` are parsed but not used in `main.zig`, issues are fetched by an unused method rather than included in generated output, release links are formatted with a hardcoded `https://github.com/owner/repo/...` placeholder, and the website URL in `snapcraft.yaml` (`https://christianhelle.com/chlogr`) currently returns 404.
- Final review for the `2025-11-15` chlogr post should compare prose and code samples against commit `4976f54`: the baseline only wires releases plus pull requests through `main.zig`, still depends on `curl` at runtime, and does not implement merge-date grouping or real release-link formatting.
- A green `bundle exec jekyll build --config _config_dev.yml` only clears rendering. Historical approval still requires checking that article claims match the November 2025 code paths, especially `src/changelog_generator.zig`, `src/http_client.zig`, and `src/markdown_formatter.zig`.
- Final approval check for the chlogr post still requires rejecting any claim that later versions fixed release-link formatting or pagination: current `christianhelle/chlogr` main still hardcodes `owner/repo` in `src/markdown_formatter.zig`, and current `README.md` still lists `No Pagination` as a known limitation.
- The rendered chlogr article builds cleanly to `_site\2025\11\building-github-changelog-generator-zig.html`; if approval fails after a green Jekyll build, the blocker is factual wording rather than Markdown/Jekyll rendering.
- November 2025 `getMergedPullRequests` is named as if it fetches merged PRs, but the baseline GitHub API call still requests `pulls?state=closed...` and does not filter `merged_at`, so post prose must avoid describing the 2025 implementation as strictly merged-only without qualification.

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