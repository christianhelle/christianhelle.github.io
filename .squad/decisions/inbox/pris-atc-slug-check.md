# ATC Test Blog Post Slug Analysis

**Decision maker:** Pris (Jekyll Dev)  
**Date:** 2025-03-27  
**Context:** Duplicate blog posts with different filenames and slugs

## Situation

Three versions of the same Atc.Test blog post exist:
- `2025-07-01-atc-test-unit-testing-for-dotnet-with-a-touch-of-class.md` (slug: `dotnet`)
- `2025-07-15-atc-test-unit-testing-for-net-with-a-touch-of-class.md` (slug: `net`)
- `2025-07-22-atc-test-unit-testing-for-net-with-a-touch-of-class.md` (slug: `net`)

## Key Findings

### 1. Canonical Version
The 2025-07-22 version is the most comprehensive (20.7 KB, too large for single view). It contains:
- Extensive redirect_from list covering all slug variants
- More detailed tags (includes "Unit Testing" vs just "Testing")
- Latest content date

### 2. Permalink Structure
Jekyll uses `permalink: /:categories/:year/:month/:title:output_ext` (from _config_prod.yml line 49).
The final URL is: `https://christianhelle.com/2025/07/atc-test-unit-testing-for-{slug}.html`

### 3. Slug Impact
- `dotnet` variant: `/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html`
- `net` variant: `/atc-test-unit-testing-for-net-with-a-touch-of-class.html`

**Both URLs are different** and would be treated as separate resources by search engines and browsers.

### 4. Archives and Tags
Both `archives.html` and `tags.html` are **generated automatically** via Liquid templating:
- Archives iterates `{% for post in site.posts %}` 
- Tags iterates `{% for tag in sorted_tags %}`
- No manual editing needed - Jekyll processes all `.md` files in `_posts/`

### 5. README.md
The README lists:
```
*   [Atc.Test - Unit testing for .NET with A Touch of Class](https://christianhelle.com/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html)
```
This references the `dotnet` slug, **not** the canonical `net` slug from 2025-07-22.

## Recommendations

### If keeping 2025-07-22 as canonical:

1. **README update required** - Change URL from `dotnet` to `net` slug
2. **Delete duplicate posts** - Remove 2025-07-01 and 2025-07-15 versions
3. **No archive/tag edits needed** - These auto-generate from remaining posts
4. **Redirect coverage already complete** - 2025-07-22 has comprehensive redirect_from covering old slugs

### Cleanup Steps
1. Delete `2025-07-01-atc-test-unit-testing-for-dotnet-with-a-touch-of-class.md`
2. Delete `2025-07-15-atc-test-unit-testing-for-net-with-a-touch-of-class.md`
3. Update README.md line 20 to use `/net/` instead of `/dotnet/`

### Validation Commands
```bash
# Verify only one post remains
ls _posts/2025/*atc-test*.md | measure

# Build and check for errors
bundle exec jekyll build

# Serve and manually verify
bundle exec jekyll serve --incremental
# Visit: http://127.0.0.1:4000/2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class.html

# Check redirects work (after deploy)
# curl -I https://christianhelle.com/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class.html
# Should return 301 redirect to /net/ version
```

## Decision

**Status:** Analysis complete - awaiting implementation approval

**Rationale:** The 2025-07-22 version is the most complete and has comprehensive redirects. The `net` vs `dotnet` slug difference creates genuine URL conflicts that need resolution, not just aesthetic cleanup.
