# Decisions

## 2026-03-20 — Chronicles Blog Post — Publication Approved

The blog post "Cosmos DB, Event Sourcing, and CQRS using Chronicles for .NET" has passed all six review gates and is **approved for publication**.

### Review Summary

| Gate | Result |
|------|--------|
| Technical Accuracy | ✅ Sound |
| Teal Content Removed | ✅ Clean |
| Comprehensive Coverage | ✅ Excellent |
| Domain Neutrality | ✅ Pass |
| Attribution | ✅ Proper |
| Editorial Quality | ✅ Good |

### Key Changes from Draft Review

The first review requested revisions on missing Chronicles repository and package references. The revised draft now includes:
- GitHub repository link in the introduction
- NuGet package name (`Chronicles`) in the introduction

**Why:** A technical post that teaches library usage without providing access to the library undermines reader trust and practical value.

### Advisory Notes (Optional Polish)

1. **A1**: OrderReservationDocument code block missing `using Chronicles.Documents;`
2. **A2**: Eventual consistency could be flagged earlier (near projections section)
3. **A3**: `IDocumentPublisher` mentioned but not demonstrated in code

These do not affect publication readiness and are noted for the author's consideration.

### Recommendation

Publish as-is. The post is technically accurate, free of internal details, comprehensive in scope, and properly attributed.

---

## 2026-03-20 — Chronicles Blog Post Publication Links

Decision to add critical references to the Chronicles technical post introduction:

1. **GitHub Repository Link**: https://github.com/chronicles-net/chronicles
   - Placed inline in the first paragraph where Chronicles is introduced
   - Formatted as a markdown link on the library name

2. **NuGet Package Availability**: `Chronicles` (NuGet package name)
   - Appended to the same introductory paragraph
   - No version number specified (readers consult NuGet.org directly for current version)

### Rationale

- Chronicles is first mentioned early in the post; placing both references immediately gives readers a clear path to explore and install
- Adding a standalone "Installation" section would break narrative rhythm; integration into existing prose maintains conversational tone
- Version numbers should not be invented; NuGet package registry is the source of truth for available versions

### Future Application

When adding library references to future technical posts, follow this model: repo link inline when first mentioned + availability note in same paragraph. Do not invent version numbers; rely on readers consulting the package registry directly.

---

## 2026-03-20 — Chronicles Blog Post Design Decisions

Used the following approach for the Chronicles blog post:

1. **Single public-safe order domain throughout** — Ensures coherent code examples while avoiding any private project details
2. **Explicit attribution** — Chronicles and Atc.Cosmos.EventStore credited to Lars Skovslund; earlier ATC article referenced as lineage and context
3. **Verified public APIs only** — Article focuses on publicly documented Chronicles APIs from provided references, with shortened examples for readability

---

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
