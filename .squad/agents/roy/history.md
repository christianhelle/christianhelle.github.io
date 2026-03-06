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

## Team Updates (2026-03-06)

**Pris (UI/Layout Dev):** Completed share button X rebrand. Replaced Twitter bird SVG with X icon glyph. Updated accessible label and title to "Share on X". Preserved existing `twitter.com/intent/tweet` endpoint and Liquid parameters. Jekyll build passed, visual validation passed on representative post page.

**Roy (Test & Infra):** Added focused Playwright regression coverage in `ShareUiTests.cs` for share UI protection. Hardened archive test infrastructure by applying `Exact = true` to link role locators, preventing substring-match collisions on title prefixes. Pre-existing unrelated timeout remains in `Crawl_Archive` at Source Code Download step (not caused by share rebrand work).

**Rachael (Content Dev):** Completed projects page refresh with 4 new repositories. Strong Zig presence in updates (3 of 4 projects): Argiope web crawler, CLOCZ line counter, and Azure SDK for Zig. Otaku (manga reader) adds portfolio diversity. All existing projects preserved. Selection emphasized non-forks, active repos, creator-maintained, meaningful descriptions. Portfolio positioning emphasizes practical developer value.
