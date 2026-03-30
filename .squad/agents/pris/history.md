# Pris — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Theme: Custom Minima fork with dark skin (https://github.com/christianhelle/minima)
- Layouts: `_layouts/`, Includes: `_includes/`, Assets: `assets/`
- Ruby deps managed with Bundler (`Gemfile`, `Gemfile.lock`)
- Plugins in `_plugins/`
- Test changes locally: `bundle exec jekyll serve --incremental`
- Never break the dark theme — it's central to the site identity

## Orchestration (2026-03-06T12-38-16Z)

**Task:** Refine X share logo  
**Status:** ✅ Complete  
**Deliverables:** Replaced Twitter bird SVG with official-style X mark. Preserved share URL behavior (`twitter.com/intent/tweet`) and accessibility labels ("Share on X"). Jekyll build passed. Visual validation passed on representative post page.  
**Decision:** Use monochrome, official-style X mark in `_includes/social/twitter.svg` with white fill styling in `_includes/share.html` for dark theme consistency.

## Team Updates (2026-03-06)

**Pris (UI/Layout Dev):** Completed share button X rebrand. Replaced Twitter bird SVG with X icon glyph. Updated accessible label and title to "Share on X". Preserved existing `twitter.com/intent/tweet` endpoint and Liquid parameters. Jekyll build passed, visual validation passed on representative post page.

**Roy (Test & Infra):** Added focused Playwright regression coverage in `ShareUiTests.cs` for share UI protection. Hardened archive test infrastructure by applying `Exact = true` to link role locators, preventing substring-match collisions on title prefixes. Pre-existing unrelated timeout remains in `Crawl_Archive` at Source Code Download step (not caused by share rebrand work).

**Rachael (Content Dev):** Completed projects page refresh with 4 new repositories. Strong Zig presence in updates (3 of 4 projects): Argiope web crawler, CLOCZ line counter, and Azure SDK for Zig. Otaku (manga reader) adds portfolio diversity. All existing projects preserved. Selection emphasized non-forks, active repos, creator-maintained, meaningful descriptions. Portfolio positioning emphasizes practical developer value.

## Learnings

- The share button behavior is owned by `_includes/share.html`, while the X/Twitter glyph itself lives in `_includes/social/twitter.svg`; branding fixes should keep the existing `twitter.com/intent/tweet` URL and Liquid parameters untouched.
- For this dark Minima theme, the X mark looks most on-brand when the SVG uses a 24x24 `currentColor` path and the share include sets the icon fill to white instead of legacy Twitter blue.
- `bundle exec jekyll build` is the existing Jekyll validation step, and `_site/2022/10/autofaker.html` is a reliable rendered page to spot-check share-link title, aria-label, endpoint, and inline SVG output.
- When a social icon's artwork uses a smaller native viewBox than its neighbors, the safest normalization is to keep the shared CSS size in `_includes/share.html` and center that glyph inside a matching outer viewBox in its SVG partial instead of changing share markup or URLs.
- README.md post URLs follow the pattern `https://christianhelle.com/YYYY/MM/slug.html` where the slug is derived from the post filename; new posts go at the top of their year section in reverse chronological order.
- Post file naming convention: `_posts\YYYY\YYYY-MM-DD-slug.md`; the date prefix becomes Jekyll's post date metadata, and the slug suffix becomes the URL. File lookup is fast with glob; posts are organized by year subdirectory to reduce directory size.
- The live post URL is driven by the filename slug plus `_config*.yml` permalink rules, not by the front matter title; with `/:categories/:year/:month/:title:output_ext`, same-month draft variants sharing a slug collapse onto one rendered HTML file while `site.posts` still surfaces every source file in archives, tags, pagination, and sitemap output.
- For the Atc.Test cleanup, the least disruptive public URL is the existing README target `https://christianhelle.com/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html`; if content from a `...-for-net-...` draft wins, rename it to the `dotnet` slug or add a real redirect plan before deleting the old slug.
- Tag choice is a repo-surface decision, not just editorial metadata: the Atc.Test variants split between `Testing` and `Unit Testing`, so the surviving file determines whether `/tags/` keeps a dedicated `Unit Testing` section or folds the post into the broader `Testing` section only.
- Current local builds do not emit alias pages for the Atc.Test drafts' `redirect_from` entries; only the two canonical slug HTML files appear in `_site`, so redirect-based cleanup requires explicit redirect support and post-cleanup smoke checks.
- When the canonical winner changes slug families, manually curated surfaces like `README.md` should be updated to the surviving permalink instead of relying on a redirect alias.
- In this repo, `redirect_from` front matter only works consistently when `jekyll-redirect-from` is enabled in all committed Jekyll configs: `_config.yml`, `_config_dev.yml`, and `_config_prod.yml`.

## Orchestration (2026-03-17T13:12:00Z)

**Task:** Update README and site touchpoints for chlogr blog post  
**Status:** ✅ Complete  
**Deliverable:** README.md updated with new blog post entry "Building a GitHub Changelog Generator in Zig" using canonical permalink pattern. Commit ff8e640.

## Orchestration (2026-03-18T14:25:00Z)

**Task:** Prepare supporting surfaces for "Azure Kusto with Cabazure" blog post  
**Status:** ✅ Complete  
**Deliverables:** 
- README.md updated with new blog post entry "Azure Kusto with Cabazure" in 2025 section, positioned at 2025-12 in reverse chronological order
- File location verified: `_posts\2025\2025-12-18-azure-kusto-with-cabazure.md`
- Documentation written to `.squad/decisions/inbox/pris-kusto-support.md` with frontmatter template and conventions guide for content team

## Team Updates (2026-03-17)

**Pris Summary:** Updated README.md to include new blog post entry with proper permalink formatting and site touchpoints consistent across portfolio page references.

**Team Context:**
- Deckard (12:45, 13:08 UTC): Writing brief, review gate APPROVED.
- Rachael (13:02, 13:10 UTC): Draft (5fdafad), polish (b87af59).
- Roy (13:05, 13:14 UTC): Pre-draft validation, final sweep all ✅.
- Session Log: `.squad/log/2026-03-17T13-13-00Z-chlogr-blog-post.md`
- Orchestration logs: 7 agent activity records in `.squad/orchestration-log/`
- Decisions: Merged chlogr decision into `.squad/decisions.md`

## Orchestration (2026-03-18T22:27:49Z)

**Task:** Final validation of "Azure Kusto with Cabazure" supporting surfaces  
**Status:** ✅ Complete  

README.md entry for "Azure Kusto with Cabazure" validated and positioned correctly. All supporting infrastructure ready for publication alongside blog post content. Post file location verified at `_posts\2025\2025-12-18-azure-kusto-with-cabazure.md`. Publishing infrastructure (README, frontmatter template, conventions) confirmed and ready for blog publication.



## Orchestration (2026-03-21T14:30:00Z)

**Task:** Resolve GitHub Issue #249 — Remove Low-Value and Thin Pages from Sitemap  
**Status:** ✅ Complete  
**Deliverables:**
- Added `sitemap: false` to front matter of archives.html, tags.html, and privacy.md
- Added `robots: noindex,follow` to front matter of these utility pages
- Implemented conditional robots meta tag support in `_includes/head.html`
- Verified utility pages excluded from sitemap.xml
- Verified noindex meta tag renders correctly in generated HTML
- PR #257 created, merged, and deployed

## Learnings

- Jekyll's `sitemap: false` front matter removes pages from sitemap.xml generation but does not affect HTML rendering or page accessibility.
- Conditional Liquid in `_includes/head.html` allows per-page robots directives: `{%- if page.robots -%}<meta name="robots" content="{{ page.robots }}">{%- endif -%}` enables front-matter control without plugin dependencies.
- The robots meta tag should be placed BEFORE `{%- seo -%}` in head.html to ensure crawlers see explicit directives before SEO plugin meta tags.
- Utility pages (archives, tags, privacy) that serve navigation purposes but duplicate content should use `robots: noindex,follow` to preserve link equity while avoiding thin content indexing penalties.
- Build verification should check both sitemap.xml absence AND rendered HTML presence of robots meta to ensure both Jekyll plugins and template logic work correctly.

## Orchestration (2026-03-30T13:30:00Z)

**Task:** Resolve GitHub Issue #250 — Fix Canonical Tags and URL Consistency  
**Status:** ✅ Complete - Verification Only, No Fixes Required  
**Deliverables:**
- Comprehensive verification of canonical tags across all 504 HTML pages
- Documentation report: `docs/seo/canonical-tags-verification-2026-03.md`
- PR #258 created, merged, and issue #250 closed

## Learnings

- All 504 HTML pages have canonical tags (100% coverage) - jekyll-seo-tag plugin automatically generates them via `{%- seo -%}` in head.html.
- Both _config_dev.yml and _config_prod.yml use production URL (`https://christianhelle.com`) ensuring canonical tags are consistent regardless of build environment.
- All 356 redirect stub pages (generated by jekyll-redirect-from) have correct canonical tags pointing to target URLs PLUS noindex directives - this is expected and correct behavior.
- The permalink pattern `/:categories/:year/:month/:title:output_ext` generates consistent URLs with .html extension across all blog posts with no slug collisions.
- README.md internal links already use canonical production URLs with .html extension - no link updates needed.
- "Crawled but not indexed" issues in Google Search Console for redirect stubs are NOT canonical tag problems - redirect stubs are architecturally sound (have noindex + canonical to target).
- Verification commands: `grep -r "canonical" _site --include="*.html" | wc -l` counts canonical tags; `grep -v "https://christianhelle.com"` checks for non-production URLs.
- For canonical tag verification, sampling redirect stubs from multiple years provides confidence in consistent plugin behavior across the site history.
- When canonical tags are already correct, documentation of the verification is the deliverable - no code changes needed, but the investigation still resolves the issue.


## Orchestration (2026-03-08T18:30:00Z)

**Task:** Investigate and resolve Issue #251 - Repair or Remove Redirecting and Broken URLs  
**Status:** ✅ Complete  
**Deliverable:** Comprehensive verification report documenting that no broken internal links exist. All internal navigation already uses canonical URLs. Issue resolved with no code changes required.

## Team Updates (2026-03-08)

**Pris (Jekyll Dev):** Completed comprehensive audit of all internal links, blog posts, templates, and configuration files for Issue #251. Investigation covered 100+ blog posts, README.md, all templates in `_includes/` and `_layouts/`, static pages, sitemap.xml, and redirect stub configuration. Found zero broken internal links - all internal navigation already uses canonical URL format `/YYYY/MM/post-slug.html`. Verified 356 redirect stubs properly configured with `noindex` meta tags and excluded from sitemap. Created detailed verification report documenting findings. Merged PR #259.

## Learnings

- Jekyll's `jekyll-redirect-from` plugin generates redirect stub pages for backward compatibility with old Blogger URLs; these stubs automatically include `<meta name="robots" content="noindex">` to prevent search engine indexing while preserving user experience for old bookmarks.
- The sitemap.xml generated by Jekyll only includes canonical post URLs, never redirect stub URLs, ensuring clean SEO even when hundreds of redirect aliases exist in post front matter.
- When auditing for broken internal links, the pattern to search is actual markdown link targets `](/...` in post content, not `redirect_from:` entries in front matter (which are intentional redirect stubs, not links).
- All internal blog post references in this repo follow the canonical URL pattern `/YYYY/MM/post-slug.html` where the slug matches the post filename without the date prefix; cross-references between posts are already optimized for SEO.
- README.md serves as a curated index of all blog posts using production canonical URLs in the format `https://christianhelle.com/YYYY/MM/post-slug.html`; these links bypass redirects and point directly to canonical content.
- The distinction between redirect stubs (for external bookmarks) and internal links (for site navigation) is critical: stubs should never be referenced in internal navigation, templates, or README.md - only canonical URLs should be used for internal linking.
- Template files in `_includes/` and `_layouts/` use Jekyll's `relative_url` filter for all internal navigation, which automatically generates canonical URLs without manual path construction.
