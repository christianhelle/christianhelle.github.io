# Roy validation gate for Atc.Test draft cleanup

## Decision
- Treat `/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html` as the safest canonical winner unless implementation intentionally changes the public slug.
- Require this local validation order: copy `_config_dev.yml` to `_config.yml`, run `bundle exec jekyll build`, run `dotnet build .\blog.sln`, serve locally, then smoke the home page, winner permalink, `/archives/`, `/tags/`, and the losing-slug aliases.
- Run `dotnet test .\tests\playwright\PlaywrightTests.csproj` when permalink, canonical URL, or redirect behavior changes.
- Do not require a new permanent automated test for the cleanup if the targeted redirect/permalink smoke checks pass.

## Why
- The repo material already points readers to the dotnet slug in the listed README content.
- `BlogArchiveTests.cs` does not currently exercise the Atc.Test route or its redirect aliases.
- The post drafts make package-behavior claims that can only be fully verified against external Atc.Test source/NuGet metadata, not by local Jekyll rendering alone.
