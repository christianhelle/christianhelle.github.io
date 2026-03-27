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
- For source-driven blog fact-checking, trust implementation files over README marketing copy; in `christianhelle/chlogr`, the code currently fetches GitHub Releases and merged pull requests, while the README overstates raw tag/issue support and understates that real API calls already exist.
- Current local validation baseline for content work: `bundle exec jekyll build` and `dotnet build .\blog.sln` are green, but full Playwright still fails on a dev-vs-production URL assertion in `tests/playwright/ShareUiTests.cs` and an archive crawl timeout at the `SqlCeEngineEx - Extending the SqlCeEngine class` link in `tests/playwright/BlogArchiveTests.cs`.
- Final long-form post validation is fastest with `_config.yml` aligned to `_config_dev.yml`, then `bundle exec jekyll build`, `dotnet build .\blog.sln`, and an HTTP smoke pass against `http://127.0.0.1:4000/`, `/archives/`, `/tags/`, and the post permalink.
- Tag validation in this blog uses the shared `/tags/` index rather than per-tag routes; confirm the relevant tag headings (for example `CLI` and `Zig`) and that the new post appears under each section.

## Orchestration (2026-03-17T13:05:00Z)

**Task:** Pre-draft validation brief for chlogr blog post  
**Status:** ✅ Complete  
**Deliverable:** Source-driven validation notes with 8 verified claims and 8 caveats/limitations. Baseline validation: jekyll build ✅, dotnet build ✅, test baseline noise noted.

## Orchestration (2026-03-17T13:14:00Z)

**Task:** Final validation sweep  
**Status:** ✅ Complete  
**Deliverable:** Smoke checks verified: bundle exec jekyll build ✅, dotnet build .\blog.sln ✅, home/permalink/archives/tags checks ✅.

## Orchestration (2026-03-06T12-38-16Z)

**Task:** Refine X share logo  
**Status:** ✅ Complete  
**Deliverables:** Updated Playwright expectation for refined SVG path. Applied `Exact = true` to archive link role locators. Confirmed Jekyll build plus full Playwright suite passed. Documented pre-existing timeout (unrelated to X icon work).  
**Decision:** Use monochrome, official-style X mark in `_includes/social/twitter.svg` with white fill styling in `_includes/share.html` for dark theme consistency.

## Team Updates (2026-03-06)

**Pris (UI/Layout Dev):** Completed share button X rebrand. Replaced Twitter bird SVG with X icon glyph. Updated accessible label and title to "Share on X". Preserved existing `twitter.com/intent/tweet` endpoint and Liquid parameters. Jekyll build passed, visual validation passed on representative post page.

**Roy (Test & Infra):** Added focused Playwright regression coverage in `ShareUiTests.cs` for share UI protection. Hardened archive test infrastructure by applying `Exact = true` to link role locators, preventing substring-match collisions on title prefixes. Pre-existing unrelated timeout remains in `Crawl_Archive` at Source Code Download step (not caused by share rebrand work).

**Rachael (Content Dev):** Completed projects page refresh with 4 new repositories. Strong Zig presence in updates (3 of 4 projects): Argiope web crawler, CLOCZ line counter, and Azure SDK for Zig. Otaku (manga reader) adds portfolio diversity. All existing projects preserved. Selection emphasized non-forks, active repos, creator-maintained, meaningful descriptions. Portfolio positioning emphasizes practical developer value.

## Orchestration (2026-03-18T11:46:00Z)

**Task:** Fact-check Cabazure.Messaging blog post against public API surface  
**Status:** ✅ Complete  
**Deliverable:** Factual accuracy report identifying 3 sections with unverified builder API methods (WithMessageId/WithCorrelationId/WithPartitionKey, WithFilter, WithPollingInterval/WithInitialization). Core abstractions (IMessagePublisher, IMessageProcessor, PublishingOptions, MessageMetadata) and DI setup patterns all verified correct against public API facts.

## Learnings

- For library blog posts, core interface signatures (IMessagePublisher, IMessageProcessor) and base option classes (PublishingOptions, MessageMetadata) must match public API exactly; these are the contract readers will code against.
- Builder pattern fluent APIs (optional lambda parameters to AddPublisher/AddProcessor) may expose methods not captured in initial GitHub MCP fact gathering; these require source verification or official docs cross-check to confirm.
- Fact-check process for .NET library posts: verify (1) interface method signatures, (2) public properties on option/metadata classes, (3) DI registration method signatures, (4) builder fluent API methods if used in examples.
- Unverified builder methods don't necessarily mean incorrect claims—they may exist but weren't surfaced in the GitHub MCP query; flag as "unverified against provided facts" rather than "wrong" unless contradicted.
- `redirect_from` front matter only generates working alias pages when `jekyll-redirect-from` is enabled in `_config.yml`, `_config_dev.yml`, and `_config_prod.yml`; otherwise the main post builds cleanly while short-link routes still 404.
- For new-post QA, always smoke the canonical permalink plus a few `redirect_from` aliases such as `/slug/`, `/YYYY/slug/`, and `/YYYY/MM/DD/slug/` so redirect regressions are caught before publish.
## Orchestration (2026-03-18T22:27:49Z)

**Task:** Validate "Azure Kusto with Cabazure" blog post  
**Status:** ✅ Complete  
**Deliverable:** Post code examples verified (12/12 accurate), `jekyll-redirect-from` enabled, `bundle exec jekyll build` ✅, full Playwright test suite passing (3/3 tests)

- **Code Accuracy:** All 12 code examples verified against public Cabazure.Kusto GitHub repo and sample apps
- **Infrastructure:** Enabled `jekyll-redirect-from` plugin in all three config files to enable working alias pages
- **Build:** `bundle exec jekyll build` clean and successful
- **Test Suite:** `dotnet test .\tests\playwright\PlaywrightTests.csproj` all 3 tests passed
- **Ready for Merge:** Post approved for publication to master branch

## Learnings

- `jekyll-redirect-from` front matter only generates working alias pages when the plugin is enabled in `_config.yml`, `_config_dev.yml`, and `_config_prod.yml`; without it, canonical post renders cleanly while short-link routes still 404
- The July 2025 Atc.Test draft set currently creates Jekyll route collisions: `_site\2025\07\atc-test-unit-testing-for-net-with-a-touch-of-class.html` resolves to the 2025-07-22 draft while `/archives` lists 2025-07-15, 2025-07-18, 2025-07-20, and 2025-07-22 against that same URL; the `/dotnet/` URL similarly resolves to 2025-07-12 while `/archives` also lists 2025-07-01.
- Current local baseline for this repo: `bundle exec jekyll build` and `dotnet build .\blog.sln --nologo` are green; with a dev server running at `http://127.0.0.1:4000`, `dotnet test .\tests\playwright\PlaywrightTests.csproj --nologo` runs 3 tests with 1 unrelated failure in `tests\playwright\ShareUiTests.cs` because the share intent URL renders `http://localhost:4000` instead of `https://christianhelle.com`.
- `.github\workflows\test-crawler.yml` is stale for current test infrastructure: it still references a `dotnet` folder and `bin/Debug/net6.0/playwright.ps1`, while the active Playwright project lives in `tests\playwright\PlaywrightTests.csproj` and targets `net8.0`.
- For multi-draft blog-post cleanup, verify the winner's slug and redirects against `README.md`, `_site\archives\index.html`, `_site\tags\index.html`, and the generated `_site\YYYY\MM\` outputs before deleting losing variants.
- After deleting or renaming posts, always run `bundle exec jekyll clean` before rebuilding; stale `_site` files can keep removed draft routes alive and hide missing redirect coverage during smoke checks.
- Local Playwright route assertions are more reliable when they match by path and use earlier navigation milestones than absolute host equality, because Jekyll's dev server may flip between `127.0.0.1` and `localhost` during browser navigation.
