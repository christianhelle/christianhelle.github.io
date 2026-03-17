# Skill: Documenting Zig Projects in Blog Posts

**Status:** Established  
**Learned from:** CLOCZ, Argiope, and chlogr blog posts  
**Owner:** Rachael

## Pattern

When writing blog posts about Zig projects, structure each post to:

1. **Opening**: 1-2 sentences describing the tool, link to GitHub source
2. **Context**: Brief note on development process ("took a few evenings," "Copilot wrote boilerplate")
3. **Body**: 7-10 technical sections, each with:
   - A 2-3 sentence explanation of what the section covers
   - 100-300 line Zig code example (real, unmodified from source)
   - Analysis of design decisions, patterns, or tradeoffs
4. **Usage section**: Real command-line examples with actual or realistic output
5. **Distribution section** (if applicable): Install scripts, snapcraft, GitHub Actions workflow
6. **Limitations/Future**: Honest assessment of what's not implemented or could improve
7. **Conclusion**: Wrap-up with call to action (link to repo, invite to contribute)

## Code Example Guidelines

- **Always pull real code** from the source repository. Never simplify beyond recognition.
- **Include context comments** where the original source has them
- **Show function/struct signatures** to help readers understand the full interface
- **Omit parts only if necessary** (e.g., "..." for brevity), but always note what's omitted
- **Each example should teach a design principle**, not just be pretty code

## Section Design Patterns

Each project post emphasizes different technical depth:

| Project | Key Focus | Example Sections |
|---------|-----------|------------------|
| CLOCZ   | Parallelism, language detection, performance | Walk-the-tree, language mapping, line counting, progress printing |
| Argiope | HTTP client, HTML parsing, web crawling | BFS crawling, HTML link extraction, URL normalization, report generation |
| chlogr  | API integration, auth fallback, data transformation | CLI parsing, token resolution (4-tier), JSON parsing, changelog grouping, Markdown building |

**Pattern**: Let the project's natural architecture drive section structure. Don't force sections into posts.

## Tone

- Direct, technical, developer-focused (matching Christian's voice)
- Explain "why" alongside "what" — design decisions matter
- Be honest about limitations; don't overpromise
- Assume audience has Zig experience but may not know this specific tool

## Front Matter

```yaml
---
layout: post
title: Building a [Tool] in Zig
date: YYYY-MM-DD
author: Christian Helle
tags:
  - Zig
  - CLI
---
```

Always include these tags: Zig, CLI (or relevant category). Include GitHub redirect_from entries if this supersedes older posts.

## Real Example

The chlogr post demonstrates full pattern:
- Intro + GitHub link (line 1-15)
- 9 technical sections covering orchestration → CLI → token resolution → API → generation → formatting → usage → build/test → distribution → limitations
- Each section: explanation + code block + analysis
- Usage with real terminal output (lines ~330-370)
- Build/test with build.zig code (lines ~380-420)
- Distribution with install scripts (lines ~430-480)
- Known limitations (lines ~510-530)
- Conclusion (lines ~535-545)
