# Pris Jekyll analysis for Atc.Test duplicate cleanup

## Decision
- Treat `https://christianhelle.com/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html` as the preferred stable public permalink unless the team intentionally wants to change the published slug.
- If the content winner is a `...-for-net-...` draft, move that content onto the `dotnet` slug instead of keeping the `net` filename as-is.
- Do not plan on the existing `redirect_from` blocks saving the losing variants by themselves; the current repo surface does not generate those alias pages in local builds.

## Why
- `README.md` already links to the `dotnet` URL, so keeping that slug avoids a repo-surface README change and preserves the only manually curated link target found outside the post files.
- The repo permalink pattern ignores the day, so date-only variants in the same month still collide onto the same output URL while archives, tags, pagination, and sitemap continue to list each source file separately.
- The `net` slug is already discoverable in generated surfaces, so if the team chooses it as canonical anyway, redirect handling becomes part of the cleanup rather than an optional nicety.

## Surface Implications
- No manual edits are needed in `archives.html`, `tags.html`, or `index.html`; they are Liquid-driven and will self-heal once only one source post remains.
- `README.md` only needs an edit if the surviving canonical URL changes away from the current `dotnet` slug.
- `/tags/` behavior depends on the winning front matter: `Testing` keeps the post in the broader testing bucket, while `Unit Testing` preserves a dedicated tag section that otherwise disappears.
- `sitemap.xml` is part of the cleanup surface: today it emits six entries for this title across only two URLs, and cleanup should bring that down to one canonical entry.
