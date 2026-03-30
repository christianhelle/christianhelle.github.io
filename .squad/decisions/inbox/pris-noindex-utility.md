# Decision: Conditional Robots Meta Tag Pattern for SEO Control

**Date:** 2026-03-21  
**Agent:** Pris (Jekyll Dev)  
**Context:** GitHub Issue #249 — SEO cleanup to remove low-value utility pages from sitemap

## Decision

Implemented a conditional robots meta tag pattern in `_includes/head.html` that allows any page to control crawler indexing via front matter:

```liquid
{%- if page.robots -%}
<meta name="robots" content="{{ page.robots }}">
{%- endif -%}
```

This pattern was placed BEFORE the `{%- seo -%}` tag to ensure explicit directives take precedence over SEO plugin defaults.

## Rationale

1. **Flexibility**: Any page can now set `robots: noindex,follow` (or other directives) in front matter without code changes
2. **No Plugin Dependency**: Uses native Liquid templating instead of requiring additional Jekyll plugins
3. **SEO Best Practice**: Placing before `{%- seo -%}` ensures crawlers see explicit directives first
4. **Separation of Concerns**: Combines with `sitemap: false` for dual control:
   - `sitemap: false` → removes from sitemap.xml (Jekyll sitemap plugin)
   - `robots: noindex,follow` → tells crawlers not to index (HTML meta tag)

## Implementation

Applied to three utility pages identified in SEO audit:
- `archives.html` — post listing duplicate of homepage navigation
- `tags.html` — tag cloud, navigation-only page  
- `privacy.md` — legal boilerplate with no unique value

Each page now has:
```yaml
sitemap: false
robots: noindex,follow
```

## Verification

- ✅ Utility pages excluded from `_site/sitemap.xml`
- ✅ Noindex meta tag renders in generated HTML: `<meta name="robots" content="noindex,follow">`
- ✅ Blog posts and other pages unaffected
- ✅ Follow directive preserves link equity while preventing indexing

## Team Impact

- **Content Team (Rachael)**: Can now control page indexing with simple front matter
- **SEO**: Cleaner sitemap focused on high-value content
- **Maintenance**: Pattern is reusable for future utility/internal pages
