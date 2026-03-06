# Decisions

## 2026-03-06 — Refine X share logo

Use a monochrome, official-style X mark in `_includes/social/twitter.svg` and let `_includes/share.html` control its presentation with a white fill for the dark theme.

**Why:** The prior replacement glyph did not closely resemble the X.com mark, and the legacy Twitter blue styling made the control feel off-brand. Keeping the SVG on `currentColor` and styling it in the share include preserves the existing layout hook while making the icon easier to refine later.

**Impact:**
- Share behavior stays on `https://twitter.com/intent/tweet`
- Accessibility text remains `Share on X`
- The dark theme keeps a clean monochrome X icon without changing other social buttons
