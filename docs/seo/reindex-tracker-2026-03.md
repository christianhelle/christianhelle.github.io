# Re-indexing Tracker - March 2026

**Purpose:** Track all pages that were improved during the SEO audit remediation (issues #248-#253) and monitor their re-indexing progress in Google Search Console.

**Date Created:** March 2026

---

## How to Use Google Search Console for Re-indexing

### URL Inspection Tool Steps
1. Go to [Google Search Console](https://search.google.com/search-console)
2. Select your property (christianhelle.com)
3. Click the search bar at the top
4. Enter the full URL (e.g., `https://christianhelle.com/about/`)
5. Review the URL inspection results
6. Click **"Request Indexing"** button if available
7. Google will crawl and process the URL within 24-48 hours

### Important Notes
- **Noindexed Pages (from #249):** Do NOT submit `/archives/`, `/tags/`, or `/privacy/` for indexing. These pages intentionally use noindex meta tag.
- **Typical Timeline:** Re-indexing typically takes 2-6 weeks after submission. Initial crawl happens within 48 hours, but indexing and ranking updates take longer.
- **Batch Submissions:** You can submit multiple URLs at once using the bulk indexing feature in Search Console.
- **Monitor Progress:** Check back weekly to monitor which URLs have been indexed and their coverage status.

---

## Pages Added with Noindex Meta Tag (Issue #249)

**Issue:** #249 - Remove Utility Pages from Organic Search  
**Status:** ✅ Completed - These should NOT be submitted for indexing

| URL | Change Type | Issue | Deployed Date | Requested Indexing | Indexed? | Notes |
|-----|-------------|-------|---------------|-------------------|----------|-------|
| `/archives/` | Added noindex meta tag, removed from sitemap | #249 | 2026-03 | N/A | N/A | Utility page - intentionally removed from search index |
| `/tags/` | Added noindex meta tag, removed from sitemap | #249 | 2026-03 | N/A | N/A | Utility page - intentionally removed from search index |
| `/privacy/` | Added noindex meta tag, removed from sitemap | #249 | 2026-03 | N/A | N/A | Utility page - intentionally removed from search index |

---

## Pages with Improved Content & SEO Metadata (Issue #252)

**Issue:** #252 - Improve Content Quality and SEO Metadata  
**Status:** ✅ Completed - These pages have SEO descriptions and improved metadata

### Standalone Pages

| URL | Change Type | Issue | Deployed Date | Requested Indexing | Indexed? | Notes |
|-----|-------------|-------|---------------|-------------------|----------|-------|
| `/about/` | Added SEO description and improved metadata | #252 | 2026-03 | [ ] | [ ] | Content improvements for author page |
| `/projects/` | Added SEO description and improved metadata | #252 | 2026-03 | [ ] | [ ] | Content improvements for project portfolio |

### Blog Posts with Improved Description and Tags

| URL | Change Type | Issue | Deployed Date | Requested Indexing | Indexed? | Notes |
|-----|-------------|-------|---------------|-------------------|----------|-------|
| `/2007/06/04/my-first-ever-blog-post/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2007/06/29/wisp-lite-in-managed-code/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2007/10/05/official-windows-mobile-60-upgrade/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2007/10/24/microsoft-dynamics-convergence-2007/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2007/11/20/visual-studio-2008-released/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2007/11/27/microsoft-development-center-copenhagen/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2008/01/01/cepa-mobility-enabling-disabled/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2008/04/10/chris-puzzle-game/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |
| `/2008/10/30/source-code-download/` | Improved SEO description and front matter tags | #252 | 2026-03 | [ ] | [ ] | Historic post - enhanced metadata |

---

## Pages with Strengthened Internal Linking (Issue #253)

**Issue:** #253 - Strengthen Internal Linking to Under-Indexed Pages  
**Status:** ✅ Completed - These posts received 3-6 new contextual internal links each

| URL | Change Type | Issue | Deployed Date | Requested Indexing | Indexed? | Notes |
|-----|-------------|-------|---------------|-------------------|----------|-------|
| `/2023/11/19/generate-http-files/` | Added 6 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Central hub - enhanced with links to cURL, .env, IDE tests, HttpTestGen, integration testing |
| `/2023/03/08/refitter/` | Added 2 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to MSBuild integration and Alba testing |
| `/2025/09/18/http-test-gen/` | Added 5 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to HTTP File Generator, integration testing, Alba, Atc.Test |
| `/2025/01/07/alba-testing/` | Added 3 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Atc.Test, HttpTestGen, integration testing |
| `/2026/02/22/rest-api-client-code-generator/` | Added 2 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Kiota and Refitter |
| `/2026/01/26/integration-testing-httprunner/` | Added 3 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to HTTP File Generator, HttpTestGen, Alba |
| `/2024/02/28/cosmos-cqrs-event-store/` | Added 2 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Cabazure Messaging and Cabazure Kusto |
| `/2025/06/21/http-file-runner-zig/` | Added 4 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Rust rewrite, chlogr, clocz, argiope |
| `/2025/10/27/http-file-runner-rust/` | Added 4 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Zig tools and other Rust projects |
| `/2025/11/25/chlogr/` | Added 5 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to HTTP File Runner, clocz, argiope, ZigFaker, Changelog Actions |
| `/2026/02/10/clocz/` | Added 4 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to HTTP File Runner, chlogr, argiope, ZigFaker |
| `/2026/03/17/agentic-engineering/` | Added 4 outbound contextual internal links | #253 | 2026-03 | [ ] | [ ] | Enhanced with links to Refitter, HTTP File Generator, Zig, Rust, and projects page |

---

## Re-indexing Status Summary

### Pages to Submit for Indexing

**Standalone Pages (2):**
- `/about/`
- `/projects/`

**Blog Posts with Improved Metadata (9):**
- `/2007/06/04/my-first-ever-blog-post/`
- `/2007/06/29/wisp-lite-in-managed-code/`
- `/2007/10/05/official-windows-mobile-60-upgrade/`
- `/2007/10/24/microsoft-dynamics-convergence-2007/`
- `/2007/11/20/visual-studio-2008-released/`
- `/2007/11/27/microsoft-development-center-copenhagen/`
- `/2008/01/01/cepa-mobility-enabling-disabled/`
- `/2008/04/10/chris-puzzle-game/`
- `/2008/10/30/source-code-download/`

**Blog Posts with Internal Link Enhancements (13):**
- `/2023/11/19/generate-http-files/`
- `/2023/03/08/refitter/`
- `/2025/09/18/http-test-gen/`
- `/2025/01/07/alba-testing/`
- `/2026/02/22/rest-api-client-code-generator/`
- `/2026/01/26/integration-testing-httprunner/`
- `/2024/02/28/cosmos-cqrs-event-store/`
- `/2025/06/21/http-file-runner-zig/`
- `/2025/10/27/http-file-runner-rust/`
- `/2025/11/25/chlogr/`
- `/2026/02/10/clocz/`
- `/2026/03/17/agentic-engineering/`

**Total URLs to Submit: 24** (2 standalone + 9 blog posts with improved metadata + 13 blog posts with internal links)

### Pages to Monitor But NOT Submit

**Noindexed Pages (3):**
- `/archives/` - Utility page, intentionally noindexed
- `/tags/` - Utility page, intentionally noindexed
- `/privacy/` - Utility page, intentionally noindexed

---

## Submission Timeline

### Recommended Schedule
- **Week 1 (Now):** Submit 5-8 URLs to avoid overwhelming Google's crawl budget
- **Week 2:** Submit next 5-8 URLs
- **Week 3:** Submit remaining URLs
- **Weekly thereafter:** Monitor Search Console for coverage and indexing progress

### Weekly Check-in Questions
- How many URLs have been crawled?
- How many have been indexed?
- Are any showing errors or warnings?
- Any decrease in crawl budget utilization?

---

## Issues Addressed

| Issue # | Topic | Pages | Status |
|---------|-------|-------|--------|
| #248 | SEO URL Audit | N/A - Analysis only | ✅ Complete |
| #249 | Remove from search index | 3 utility pages | ✅ Noindex applied |
| #250 | Canonical tag verification | N/A - Verified correct | ✅ Complete |
| #251 | Broken internal links | N/A - No issues found | ✅ Verified |
| #252 | Content quality improvements | 11 pages | ✅ Enhanced |
| #253 | Internal link strengthening | 13 posts | ✅ Links added |
| #254 | Re-indexing tracker | This document | ✅ Tracking setup |

---

## Next Steps for Christian

1. **Prepare URLs:** Copy the 24 URLs from the "Pages to Submit" list above
2. **Submit in batches:** Use Google Search Console's URL Inspection tool
3. **Track weekly:** Check back each week to monitor indexing progress
4. **Expect timeline:** Most URLs should show in Google Search within 2-6 weeks
5. **Monitor analytics:** Watch for improvements in:
   - Organic search traffic
   - CTR (Click-Through Rate) from SERP
   - Average position in search results
   - Crawl stats in Search Console

---

## Notes

- This tracker will be updated weekly as indexing progresses
- Check `docs/seo/internal-link-plan-2026-03.md` for details on the #253 link additions
- See `docs/seo/non-indexed-url-audit-2026-03.md` for the original audit that identified #249-#253 opportunities
- See `docs/seo/canonical-tags-verification-2026-03.md` for canonical tag verification details from #250
