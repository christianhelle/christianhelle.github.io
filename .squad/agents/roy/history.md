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

## Team Updates (2026-03-06)

**Rachael (Content Dev):** Completed projects page refresh with 4 new repositories. Strong Zig presence in updates (3 of 4 projects): Argiope web crawler, CLOCZ line counter, and Azure SDK for Zig. Otaku (manga reader) adds portfolio diversity. All existing projects preserved. Selection emphasized non-forks, active repos, creator-maintained, meaningful descriptions. Portfolio positioning emphasizes practical developer value.
