# Atc.Test Post Redirect Fix

## Decision Date
2025-07-22

## Context
The canonical Atc.Test post (`2025-07-22-atc-test-unit-testing-for-net-with-a-touch-of-class.md`) existed in draft form on multiple dates before finalization. Additionally, there were URL variants using `dotnet` instead of `net` in the slug. The cleanup retained the canonical `net` slug post but left old URLs without redirect coverage, causing 404 errors for bookmarks and external links.

## What Changed
Updated the `redirect_from` front matter in the Atc.Test post to comprehensively cover:

### 1. `dotnet` Slug Variants
Added redirect entries for the alternate slug form using `dotnet` instead of `net`:
- `/2025/07/22/atc-test-unit-testing-for-dotnet-with-a-touch-of-class` (with/without trailing slash)
- `/2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class` (with/without trailing slash)
- `/2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class` (with/without trailing slash)
- `atc-test-unit-testing-for-dotnet-with-a-touch-of-class` (with/without trailing slash)

### 2. Deleted Draft Route Coverage
Added redirect entries for the five dated draft versions that were deleted:
- `/2025/07/01/...` (with/without trailing slash)
- `/2025/07/12/...` (with/without trailing slash)
- `/2025/07/15/...` (with/without trailing slash)
- `/2025/07/18/...` (with/without trailing slash)
- `/2025/07/20/...` (with/without trailing slash)

All draft dates use the final canonical `net` slug form.

## Impact
- **Breakage prevented:** Old bookmarks and external links using `dotnet` variant URLs now redirect cleanly
- **Draft history handling:** Users accessing dated draft URLs are redirected to the canonical post
- **No body changes:** Only front matter modified; post content remains unchanged
- **Jekyll compatibility:** All redirect paths follow Jekyll permalink patterns and Jekyll's built-in redirect plugin will handle them correctly

## Rationale
Comprehensive redirect coverage ensures robust URL handling across multiple content evolution phases:
1. Original dated drafts (seven attempts across 07-01 to 07-22)
2. Slug form variants (net vs dotnet)
3. Canonical hierarchy (dated → monthly → yearly → slug-only)

This prevents users and external systems from hitting 404 errors when accessing the post via legacy URLs.
