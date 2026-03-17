# Roy — chlogr validation brief

## Scope

- Pre-draft, source-driven validation for the planned post `Building a Github Changelog Generator in Zig`
- Reviewed the `christianhelle/chlogr` source files supplied for this round plus the two recent Zig posts used as style references
- The expected draft file `_posts\2026\2026-03-17-building-a-github-changelog-generator-in-zig.md` does not exist locally yet

## Claims clearly supported by source

1. `chlogr` is a Zig CLI with a normal `zig build` entry point and a custom `zig build test` integration-test step defined in `build.zig`.
2. The executable entry point lives in `src/main.zig`, parses CLI arguments, resolves a GitHub token, fetches release and pull request data, generates a changelog model, formats Markdown, and writes an output file.
3. `--repo` is required, while `--token`, `--output`, `--since-tag`, `--until-tag`, `--exclude-labels`, and `--help` are accepted optional flags in `src/cli.zig`.
4. Token lookup is implemented with a clear fallback chain in `src/token_resolver.zig`: explicit `--token`, `GITHUB_TOKEN`, `GH_TOKEN`, then `gh auth token`; if none are available, the app continues anonymously.
5. The GitHub API client uses Zig stdlib HTTP in `src/http_client.zig`, targets `https://api.github.com`, and fetches `/repos/{repo}/releases` plus closed pull requests from `/repos/{repo}/pulls?...`.
6. Changelog categorization is label-driven in `src/changelog_generator.zig`: `feature`/`enhancement` map to `Features`, `bug`/`bugfix` map to `Bug Fixes`, and everything else falls back to `Merged Pull Requests`.
7. The Markdown formatter in `src/markdown_formatter.zig` emits a top-level `# Changelog`, adds an `Unreleased Changes` section when applicable, and lists entries as linked PR numbers plus `@author` handles.
8. The project has integration-style tests in `src/test.zig` backed by mock payloads in `src/test_data.zig`, covering JSON parsing, changelog generation, unreleased sections, exclusion behavior, categorization, and Markdown formatting.

## Caveats and limitations the article should mention

1. The current implementation uses the GitHub Releases API, not raw Git tags. The release model reads `tag_name` from release objects, so a repository with tags but no releases can yield no release sections.
2. Closed issues are modeled and there is a `getClosedIssues()` helper, but the main flow never fetches or formats issues. The article should not claim issue-based changelogs as a shipped feature.
3. `--since-tag` and `--until-tag` are parsed but not used anywhere in `src/main.zig` or `src/changelog_generator.zig`. They read like planned options, not working filters.
4. Release links in the generated Markdown are currently hard-coded to `https://github.com/owner/repo/releases/tag/...` in `src/markdown_formatter.zig`, so the header links are placeholders rather than repository-aware URLs.
5. `--exclude-labels` currently checks whether each label name appears anywhere inside the raw comma-separated string. That is substring matching, not proper CSV parsing.
6. Per-release grouping only checks whether a PR merged before a given release date. There is no lower bound per release window, so older pull requests can appear in multiple release sections.
7. The README is partially out of sync with the code. It still says HTTP integration is pending, and it overstates support for tags and issues, while the code already performs real API calls for releases and pull requests.
8. No workflow or installer files were part of the supplied source pack for this pass, so claims about release automation, packaged installers, or CI distribution should be separately sourced before publication.

## Validation decision

Because the current Playwright suite does not provide dedicated smoke coverage for newly added post routes, new long-form posts should be validated with a local render check on the final permalink plus manual checks on the home page, archives page, and tag pages even when the existing automated suite is also run.

## Local validation baseline observed in this pass

- `bundle exec jekyll build` passes with development configuration
- `dotnet build .\blog.sln` passes
- `dotnet test .\tests\playwright\PlaywrightTests.csproj` is not fully green today for reasons unrelated to the new post:
  - `tests/playwright/ShareUiTests.cs` expects a production canonical URL in the X share link, but the dev server renders `http://localhost:4000/...`
  - `tests/playwright/BlogArchiveTests.cs` times out while locating `SqlCeEngineEx - Extending the SqlCeEngine class`

## Final validation steps once the draft lands

1. Confirm the draft exists at `_posts\2026\2026-03-17-building-a-github-changelog-generator-in-zig.md` with correct front matter, slug, date, and Zig-oriented tags.
2. Copy development config into place: `Copy-Item _config_dev.yml _config.yml -Force`.
3. Run `bundle exec jekyll build`.
4. Start the dev server and open the final permalink locally. Verify headings, section order, code fences, syntax highlighting, and that code samples mirror current `chlogr` source.
5. Check that the post appears correctly on the home page, `/archives`, and relevant tag pages.
6. Verify internal links and all external GitHub links used in the article, especially repository, file, and release references.
7. Run `dotnet build .\blog.sln`.
8. Run `dotnet test .\tests\playwright\PlaywrightTests.csproj`, but treat the two failures noted above as current baseline noise unless they are fixed as part of the same work.
9. Do a final manual smoke pass on the post page because there is no existing Playwright test that explicitly opens newly added post routes.
