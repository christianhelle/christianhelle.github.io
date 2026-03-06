---
name: "project-conventions"
description: "Core conventions and patterns for this codebase"
domain: "project-conventions"
confidence: "medium"
source: "template"
---

## Context

> **This is a starter template.** Replace the placeholder patterns below with your actual project conventions. Skills train agents on codebase-specific practices — accurate documentation here improves agent output quality.

## Patterns

### [Pattern Name]

Describe a key convention or practice used in this codebase. Be specific about what to do and why.

### Error Handling

<!-- Example: How does your project handle errors? -->
<!-- - Use try/catch with specific error types? -->
<!-- - Log to a specific service? -->
<!-- - Return error objects vs throwing? -->

### Testing

- Browser regression coverage for the blog archive lives in `tests/playwright/BlogArchiveTests.cs` and runs with `cd tests/playwright && dotnet test`.
- Playwright archive link locators should use `GetByRole(..., new() { NameString = "...", Exact = true })` because archive titles can share prefixes and default accessible-name matching is substring-based.
- When validating archive regressions locally, use development config (`_config_dev.yml` copied to `_config.yml`), make sure the Jekyll server is serving `http://127.0.0.1:4000/`, then run `dotnet test --filter "Crawl_Archive"` for the focused regression check.

### Code Style

<!-- Example: Linting, formatting, naming conventions -->
<!-- - Linter: ESLint config? -->
<!-- - Formatter: Prettier? -->
<!-- - Naming: camelCase, snake_case, etc.? -->

### File Structure

<!-- Example: How is the project organized? -->
<!-- - src/ — Source code -->
<!-- - test/ — Tests -->
<!-- - docs/ — Documentation -->

## Examples

```csharp
await page.GetByRole(AriaRole.Link, new() { NameString = "Danish Developer Conference 2012", Exact = true }).ClickAsync();
await page.WaitForURLAsync($"{baseUrl}/2012/02/danish-developer-conference-2012.html");
```

## Anti-Patterns

<!-- List things to avoid in this codebase -->
- **Partial Playwright archive link matches** — Do not rely on default `GetByRole` name matching for archive titles, because substring collisions can make the crawl flaky as new posts are added.
