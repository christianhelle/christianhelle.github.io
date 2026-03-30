# Decision: SEO URL Audit Strategy (Issue #248)

**Date:** January 2026  
**Agent:** Deckard (Lead)  
**Context:** Google Search Console reported "crawled but not indexed" URLs  
**Status:** ✅ Complete

## Decision

Conducted comprehensive URL audit of all 504 generated HTML pages and classified root causes of indexing issues. Primary finding: 356 redirect stub pages (jekyll-redirect-from plugin) are bulk of "crawled but not indexed" — this is **expected behavior**, not a problem.

## Analysis

### Build Process
- Configured Ruby environment: `GEM_HOME=$HOME/gems`, `PATH=$HOME/gems/bin:$PATH`
- Built site: `cp _config_dev.yml _config.yml && bundle exec jekyll build` (60s)
- Analyzed _site/ output structure

### URL Classification
1. **Blog Posts (129):** Valuable content, keep indexed
2. **Redirect Stubs (356):** jekyll-redirect-from generated; already have `noindex` meta tags; expected to be crawled but not indexed
3. **Utility Pages (6):**
   - **Noindex candidates:** archives, tags, privacy (thin content)
   - **Keep indexed:** about, projects, homepage (valuable content)
4. **Error Pages (1):** 404.html — correctly excluded from sitemap

### Root Causes
- jekyll-redirect-from generates 356 redirect stub pages with `<meta http-equiv="refresh">` and `<meta name="robots" content="noindex">`
- No conditional robots meta logic in `_includes/head.html`
- Utility pages (archives, tags, privacy) provide minimal search value but are in sitemap

### Key Insight
**Redirect stubs are functioning as designed.** They have noindex directives and should not be in Google's index. GSC warnings about them being "crawled but not indexed" are expected and can be safely ignored. The real issue is 3 utility pages that need noindex directives.

## Remediation Strategy

Created 6 sub-issues for targeted fixes:
- **#249:** Remove low-value pages from sitemap (add noindex to archives, tags, privacy)
- **#250:** Verify canonical tags on redirect stubs point to target URLs
- **#251:** Fix internal links pointing to redirect stubs instead of canonical URLs
- **#252:** Improve content quality on older blog posts
- **#253:** Strengthen internal linking between related posts
- **#254:** Request re-indexing in Google Search Console after fixes

## Deliverable

**File:** `docs/seo/non-indexed-url-audit-2026-03.md` (339 lines)
- Executive summary with metrics
- Detailed classification table
- Technical analysis of current implementation
- Issues identified with code examples
- Remediation plan with effort/impact ratings
- Expected outcomes and success criteria
- Links to sub-issues #249-#254

## Implementation

**Workflow:**
1. Created feature branch: `feature/issue-248-url-audit`
2. Built site and analyzed output
3. Created comprehensive audit document
4. Committed with co-author trailer
5. Pushed and created PR #256
6. Commented on issue #248 with findings
7. Merged PR and deleted feature branch
8. Switched back to master

**PR:** #256  
**Issue:** #248  
**Status:** Merged to master

## Team Impact

**Decision applies to:**
- Future SEO audits: jekyll-redirect-from behavior is architectural, not a bug
- Content strategy: Focus on improving blog post quality over fixing redirect stubs
- Site structure: Utility pages (archives, tags, privacy) are candidates for noindex
- Monitoring: GSC "crawled but not indexed" warnings for redirect stubs can be ignored

**Key Principle:** Distinguish between expected architectural behavior (redirect stubs with noindex) and actual indexing problems (utility pages without noindex).
