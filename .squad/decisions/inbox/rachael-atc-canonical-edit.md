# Atc.Test Post Canonicalization — Rachael Decision Log

**Date:** 2026-05-XX  
**Decision Made By:** Rachael (Content Dev)  
**Status:** COMPLETED

## Summary

Consolidated six Atc.Test blog post drafts into single canonical post. Winner: `2025-07-22-atc-test-unit-testing-for-net-with-a-touch-of-class.md`. Fixed non-publishable version placeholder; deleted five losing drafts.

## Decision Details

**Primary Issue:** Six near-identical post drafts existed across July 2025, creating maintenance burden and SEO confusion. One contained unpublishable placeholder `$(LatestOrPinned)`.

**Selection Criteria:**
1. **Recency**: July 22 > July 20 > July 18 > July 15 > July 12 > July 1 (winning post has freshest perspective)
2. **Accuracy**: Winning post uses current stable Atc.Test version; July 18 & 20 had outdated 1.0.0
3. **Structure**: Comprehensive redirect_from coverage already in place; covers all variant slugs
4. **Factual soundness**: No showstopper inaccuracies identified; minor ceremony/wording differences acceptable

**Version Fix:**
- Changed: `Version="$(LatestOrPinned)"`
- Changed To: `Version="2.0.17"`
- Verified: NuGet.org shows 2.0.17 as latest stable (released 2/8/2026)

**Redirect Coverage:**
Already present in winning post's front matter:
- `/2025/07/22/atc-test-unit-testing-for-net-with-a-touch-of-class` (exact)
- `/2025/07/22/atc-test-unit-testing-for-net-with-a-touch-of-class/` (trailing slash)
- `/2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class` (month-level)
- `/2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class/` (trailing slash)
- `/2025/atc-test-unit-testing-for-net-with-a-touch-of-class` (year-level)
- `/2025/atc-test-unit-testing-for-net-with-a-touch-of-class/` (trailing slash)
- `atc-test-unit-testing-for-net-with-a-touch-of-class` (slug only)
- `atc-test-unit-testing-for-net-with-a-touch-of-class/` (slug + trailing slash)

No additional redirects required for deleted drafts (all had matching slug names; only dates differed).

## Files Deleted

1. `_posts/2025/2025-07-01-atc-test-unit-testing-for-dotnet-with-a-touch-of-class.md`
2. `_posts/2025/2025-07-12-atc-test-unit-testing-for-dotnet-with-a-touch-of-class.md`
3. `_posts/2025/2025-07-15-atc-test-unit-testing-for-net-with-a-touch-of-class.md`
4. `_posts/2025/2025-07-18-atc-test-unit-testing-for-net-with-a-touch-of-class.md`
5. `_posts/2025/2025-07-20-atc-test-unit-testing-for-net-with-a-touch-of-class.md`

## Files Modified

- `_posts/2025/2025-07-22-atc-test-unit-testing-for-net-with-a-touch-of-class.md` (version fix only, line 65)

## No Blockers

- Post is publishable and technically accurate
- Front matter valid; redirects sufficient
- No passages from losing drafts justified surgical transplant
- No layout/template changes required
- No external links broken

## Next Steps

- Roy (Build & Test): Validate post renders correctly in dev build; run Playwright suite if full validation needed
- Christian Helle: Review merged post for final editorial sign-off before merge to master
- No config, README, or infrastructure changes required
