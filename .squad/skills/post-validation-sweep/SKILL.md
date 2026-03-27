---
name: "post-validation-sweep"
description: "Validate a new Jekyll post with build and local smoke checks"
domain: "jekyll-testing"
confidence: "high"
source: "repository"
---

## Context

Use this skill when a new blog post needs a final QA pass and the goal is to verify local rendering without rewriting content.

## Patterns

### Development Configuration First

- Make sure `_config.yml` matches `_config_dev.yml` before local validation so Jekyll builds and serves with the expected development settings.
- Keep this as an environment-prep step, not a content change.

### Build Validation

- If the change deleted or renamed post files, run `bundle exec jekyll clean` before `bundle exec jekyll build`.
- A plain rebuild can leave stale files in `_site`, which makes removed draft routes look healthy even when they would be missing in a clean publish.
- Run `bundle exec jekyll build` from the repo root to confirm the site renders into `_site` without Liquid or Markdown failures.
- Run `dotnet build .\blog.sln` to make sure the Playwright test solution still compiles after the content change.

### Practical Smoke Pass

- If nothing is already serving the site, start `bundle exec jekyll serve --host 127.0.0.1 --port 4000 --incremental`.
- Check the home page, the new post permalink, `/archives/`, and `/tags/` over HTTP.
- On the post permalink, confirm the rendered title, at least a couple of key section headings, and syntax-highlighted code blocks.
- On `/archives/`, confirm the new entry is listed under the correct year.

### Redirect Validation

- If the post uses `redirect_from`, make sure `jekyll-redirect-from` is enabled in `_config.yml`, `_config_dev.yml`, and `_config_prod.yml`; front matter alone is not enough.
- If a canonical slug changes during cleanup, update manually curated surfaces such as `README.md` to the winning permalink; redirects should protect legacy links, not become the new primary target.
- After the build, smoke at least the short-link aliases (`/slug/`, `/YYYY/slug/`, `/YYYY/MM/DD/slug/`) and confirm they return redirect pages instead of 404s.

### Variant Collision Audit

- If multiple draft posts share the same month and slug, Jekyll can silently overwrite the generated page while `/archives/` still lists every dated draft.
- Before selecting a winner, inspect `_site\YYYY\MM\` for the actual generated files and compare `/archives/` entries to the resolved URLs so duplicate-date-to-one-route collisions are caught early.
- When a winner changes slug family (for example `dotnet` vs `net`), include legacy canonical URLs in the redirect smoke pass before deleting losing variants.

### Tag Validation

- This repo uses a single `/tags/` index page with one `<h2>` section per tag.
- Do not expect dedicated `/tag/<name>/` routes.
- For a tagged post, verify the relevant headings exist and the post appears in each matching section.

### Optional Playwright Follow-Up

- Full Playwright is optional for content-only validation when the requested sweep only needs build confidence plus route smoke checks.
- When share links, canonical URLs, or redirects change, run the full suite with the dev server up at `http://127.0.0.1:4000/`; the current 3-test Playwright suite is capable of going fully green.

## Anti-Patterns

- Assuming the tag system generates one page per tag.
- Assuming `redirect_from` works without `jekyll-redirect-from` enabled in the Jekyll config.
- Stopping after `_site` generation without checking the live local routes.
- Treating `_config.yml` environment setup as a publishable content change.
