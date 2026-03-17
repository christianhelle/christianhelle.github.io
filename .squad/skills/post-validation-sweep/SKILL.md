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

- Run `bundle exec jekyll build` from the repo root to confirm the site renders into `_site` without Liquid or Markdown failures.
- Run `dotnet build .\blog.sln` to make sure the Playwright test solution still compiles after the content change.

### Practical Smoke Pass

- If nothing is already serving the site, start `bundle exec jekyll serve --host 127.0.0.1 --port 4000 --incremental`.
- Check the home page, the new post permalink, `/archives/`, and `/tags/` over HTTP.
- On the post permalink, confirm the rendered title, at least a couple of key section headings, and syntax-highlighted code blocks.
- On `/archives/`, confirm the new entry is listed under the correct year.

### Tag Validation

- This repo uses a single `/tags/` index page with one `<h2>` section per tag.
- Do not expect dedicated `/tag/<name>/` routes.
- For a tagged post, verify the relevant headings exist and the post appears in each matching section.

### Optional Playwright Follow-Up

- Full Playwright is optional for content-only validation when the requested sweep only needs build confidence plus route smoke checks.
- If the full suite is run, treat the current share-link environment mismatch and the archive crawl timeout as known baseline noise unless the new work makes either failure worse.

## Anti-Patterns

- Assuming the tag system generates one page per tag.
- Stopping after `_site` generation without checking the live local routes.
- Treating `_config.yml` environment setup as a publishable content change.
