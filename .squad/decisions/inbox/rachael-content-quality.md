# Decision: SEO Content Quality Strategy for Thin Posts

**Date**: 2026-01-23  
**Decided by**: Rachael (Content Dev)  
**Context**: Issue #252 - Improve content quality for crawled-not-indexed pages

## Decision

When improving thin content from older blog posts (especially 2007-2013 Blogger migrations), focus on:

1. **Add missing `description:` front matter** - Write 1-2 sentence SEO-friendly descriptions based on existing content
2. **Add missing `tags:`** - Use existing site taxonomy, don't invent new tags
3. **Never fabricate technical details** - Only enhance metadata and context that's clearly inferable
4. **Prioritize indexed valuable pages first** - Standalone pages (about, projects) before blog posts

## Rationale

- Older Blogger-migrated posts often lack structured metadata (description, tags)
- Even short announcement posts can have good SEO value with proper descriptions
- Search engines need clear meta descriptions to properly index and present content
- Tag consistency helps with site navigation and categorization
- Maintaining voice and technical accuracy is critical to site credibility

## Selection Criteria for Posts to Improve

- Line count under 25 (very thin)
- Missing `description:` front matter
- Missing or empty `tags:` 
- From 2007-2013 era (Blogger migration period)
- Has a valuable title/topic worth preserving

## What NOT to Do

- ❌ Don't fabricate technical content or code examples
- ❌ Don't change post dates, permalinks, or categories
- ❌ Don't modify the core technical meaning
- ❌ Don't invent new tag names - use existing taxonomy
- ❌ Don't add content that wasn't clearly in the original

## Impact

This approach allows us to:
- Improve SEO discoverability without compromising accuracy
- Maintain Christian's authentic developer voice
- Preserve historical posts while making them search-friendly
- Apply consistent metadata standards across the site

## Related

- Issue #252 - Improve content quality for low-value pages
- docs/seo/non-indexed-url-audit-2026-03.md - SEO audit findings
- PR #260 - Implementation of this strategy
