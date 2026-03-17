# Decisions

## 2026-03-17 — November 2025 Historical Accuracy Revision

The blog post "Building a GitHub Changelog Generator in Zig" (2025-11-15) required corrections to accurately reflect the November 2025 project state. Content was updated to use "changelog-generator" naming (renamed to chlogr in March 2026), separate `--owner` and `--repo` flags (original November 2025 interface), curl-based subprocess HTTP (actual implementation, not std.http.Client), and required token (no anonymous fallback added in March 2026). All code samples and feature descriptions now match the November 2025 codebase baseline (commit 4976f54). Redirect_from added to front matter for URL continuity.

**Status:** Revision completed by Rachael. Post transferred to Deckard for structural review gate.

## 2026-03-17 — chlogr Post Structural Alignment

The revised chlogr post is historically accurate and technically sound but structurally diverges from reference posts (Argiope, clocz) that define the Zig series pattern. Missing two signature sections: "How it works" (architectural overview) and "Distribution" (cross-compilation, packaging, CI/CD). Four standalone sections (Motivation, Key Design Decisions, Lessons Learned, Limitations) break the pattern where reference posts embed these in intro and conclusion. Conclusion is thinner than established pattern.

**Assignment:** Pris (structural revision). Rachael locked out per reviewer rules. Code examples and historical facts verified correct — only section reorganization needed to align with series pattern.

## 2026-03-06 — Normalize share icon sizes

Normalize the X share glyph by preserving the shared 40x40 CSS sizing in `_includes/share.html` and centering the X path inside a 40x40 outer SVG viewBox in `_includes/social/twitter.svg`.

**Why:** This is the safest UI-only fix because it corrects visual alignment without altering share include markup, the existing `twitter.com/intent/tweet` behavior, or the other icon assets. Ensures all three share icons (X, LinkedIn, Facebook) render at equal size.

**Impact:**
- X icon visually aligns with LinkedIn and Facebook in dark theme
- Share behavior preserved on `https://twitter.com/intent/tweet`
- Accessibility text remains `Share on X`
- No changes to share include markup or Liquid parameters

## 2026-03-06 — Validate share icon sizing with browser tests

Treat same-size share icons as a regression requirement and verify it in Playwright by comparing the rendered SVG boxes for X, LinkedIn, and Facebook on a representative post page.

**Why:** File-level SVG inspection alone is not enough to prove rendered size matches after centering. Browser-level size assertion catches future drift in CSS sizing or SVG framing while keeping tests focused on user-visible outcomes.

**Impact:**
- `tests/playwright/ShareUiTests.cs` guards equal 40x40 rendering for three share icons
- Regression coverage prevents visual drift in share button styling
- Post-page validation includes X label, Twitter intent endpoint, and rendered SVG dimensions

## 2026-03-06 — Harden archive test locators with exact name matching

Apply `Exact = true` to link role locators in `tests/playwright/BlogArchiveTests.cs` to prevent substring-match collisions on post titles with shared prefixes (e.g., "Danish Developer Conference 2012" and "Multi-platform Mobile Development").

**Why:** Multiple post titles can share prefixes, causing `GetByRole(...Name = "...")` to match multiple elements unintentionally. Exact matching ensures link lookups target the correct archive entry.

**Impact:**
- Archive regression coverage stays stable on static page crawls
- `DOMContentLoaded` waiter replaces `load` event for faster, more reliable navigation
- No pre-existing timeouts introduced by share rebrand work
