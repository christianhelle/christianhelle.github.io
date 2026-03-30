---
name: "duplicate-post-surface-audit"
description: "Audit duplicate Jekyll posts for canonical slug, listing fallout, and redirect needs"
domain: "jekyll-content-ops"
confidence: "high"
source: "repository"
---

## Context

Use this skill when multiple `_posts` files appear to represent the same article and the team needs a repo-surface answer before deleting variants.

## Patterns

### Start With the Real URL Contract

- Read `_config.yml`, `_config_dev.yml`, and `_config_prod.yml` to confirm the permalink template.
- Remember that Jekyll derives `:title` from the filename slug, not from the front matter `title`.
- If the permalink omits the day, two posts from the same month with the same slug will collide into one rendered URL.

### Separate Source Duplication From Output Duplication

- Inspect the duplicate post front matter first: filename slug, date, tags, and `redirect_from`.
- Then build the site and inspect `_site` plus `sitemap.xml` to see how many rendered URLs actually exist.
- A collision can leave you with one HTML file per slug while `site.posts` still causes archives, tags, and paginated indexes to list every source file.

### Prefer Existing Public Touchpoints

- Check manually curated surfaces like `README.md` before recommending a new canonical slug.
- If one slug is already linked outside the post files, preserving it is usually the safest path for stable links.
- If the best content lives in a differently named draft, prefer moving that content onto the established slug over forcing a public URL change.

### Audit Tags as Part of the Surface

- Compare tag lists across variants; metadata differences can change `/tags/` sections even when title and content are similar.
- Call out when keeping one variant would remove or create an entire tag heading.

### Redirects Need Real Infrastructure

- Do not assume `redirect_from` is active just because it appears in front matter.
- Confirm that the repo actually generates alias pages in `_site`; if not, recommend enabling redirect support or adding a different redirect mechanism before relying on aliases.

## Smoke Checklist

- `bundle exec jekyll build --config _config_prod.yml`
- Confirm only one canonical rendered post URL remains.
- Confirm `/archives/` and `/tags/` show one entry for the article.
- Confirm `sitemap.xml` contains one canonical entry for the article.
- If redirect behavior is part of the cleanup, smoke every losing slug and every promised short alias.

## Anti-Patterns

- Choosing a winner by front matter `title` alone.
- Ignoring README or other curated links when changing a slug.
- Assuming duplicate source files automatically mean duplicate rendered HTML files.
- Assuming archives, tags, and pagination deduplicate colliding posts for you.
