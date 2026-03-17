# Rachael — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle (developer, direct technical writing style)
- Posts in `_posts/YYYY/YYYY-MM-DD-title.md`
- Topics: REST APIs, code generation, developer tools, .NET, C#, open-source
- Always reference an existing post for front matter format before writing new ones
- Writing tone: concise, technical, developer-focused

## Learnings

### 2026-03-05: Argiope Web Crawler Post
- Created post about Argiope, a web crawler written in Zig for broken-link detection
- Followed style from "Building a fast line of code counter app in Zig" post (2026-02-10)
- Post structure: intro → multiple technical sections with code examples → usage examples with output → distribution → conclusion
- Each section demonstrates implementation details: crawler BFS, HTML parsing, URL normalization, HTTP client, CLI parsing, report generation
- Christian's preferred code example style: show key functions with comments, explain design decisions inline
- Usage section shows realistic command-line examples with actual output formatting
- Distribution section covers install scripts (bash/PowerShell), snapcraft, GitHub Actions
- Front matter format: layout (post), title, date, author, tags (array)
- Tags for this post: Zig, CLI (matches clocz post tagging pattern)
- Post file naming: `_posts/YYYY/YYYY-MM-DD-title.md` format strictly followed
- Christian uses GitHub Copilot heavily for boilerplate (workflows, README, install scripts)

### 2026-03-17: chlogr Post First Draft

Completed comprehensive 467-line blog post "Building a GitHub Changelog Generator in Zig" with 13 sections. Post structure: intro, how it works, data models, token resolution, HTTP client, GitHub API, changelog generation, markdown formatting, CLI parsing, testing, usage, distribution, conclusion. Heavily grounded in source code: extracted actual functions from main.zig, cli.zig, token_resolver.zig, github_api.zig, changelog_generator.zig, markdown_formatter.zig. Followed established Zig post pattern with multiple substantial sections featuring real code examples and explanatory narrative. Tagged: Zig, CLI. Committed with detailed message describing post contents (f110feb). Front matter includes redirect_from aliases per Argiope convention. Ready for team review and publication workflow.

## Team Updates (2026-03-17)

**Deckard (Research & Planning):** Completed chlogr structure brief with 13 sections, must-cover implementation details, narrative arc, and consistency checklist. Key technical insights: token fallback chain, deep-copy-then-deinit JSON pattern, label-based categorization, std.http.Client usage, standalone executable testing pattern. Unique hook identified: chlogr is first Zig project in series with real HTTP I/O and REST API integration.

**Pris (UI/Layout Dev):** Completed Jekyll conventions guidance by cross-referencing Argiope and clocz posts. Deliverables: standardized front matter template with redirect_from aliases, file naming convention, heading rhythm (2–3 intro + H2 sections), code fence conventions with language hints, link formatting guidelines, style guidance (developer diary tone, code-backed explanations). Established Argiope-style front matter as newer baseline.

**Roy (Validation & Testing):** Completed validation briefing flagging November 2025 vs March 2026 timeline drift. Earliest authored commits (2025-11-13) vs public repo creation (2026-03-15). Identified March 2026 changes to be explicitly framed as later evolution: chlogr rename, CLI consolidation, curl → std.http.Client, snapcraft packaging. Validation consequence: article should use November 2025 baseline with optional "now called chlogr" framing.

**Rachael (Content Dev):** Completed first draft of blog post with 467 lines, 13 sections, and comprehensive code examples. Post committed to f110feb with detailed message. Ready for team review and publication workflow.



### 2025-11-15: GitHub Changelog Generator (chlogr) Post
- Created comprehensive 467-line blog post documenting the chlogr project
- Post structure: intro → motivation → CLI design → token resolution → GitHub API → changelog generation → formatting → usage → design decisions → lessons learned → limitations
- Heavily grounded in source code: extracted actual functions from main.zig, cli.zig, token_resolver.zig, github_api.zig, changelog_generator.zig, markdown_formatter.zig
- Key technical sections include: memory management patterns (deep copy after JSON), token resolution fallback chain (flag → env vars → gh CLI), label-based PR categorization, Markdown formatting strategy
- Followed established Zig post pattern (CLOCZ/Argiope) with multiple substantial sections, each featuring real code examples and explanatory narrative
- Post emphasizes practical patterns: no external dependencies (pure Zig stdlib), explicit memory management, design rationale for constraints
- Included example output, building/testing instructions, and honest limitations section
- Tagged: Zig, CLI (consistent with other Zig project posts)
- Commit: f110feb — focused single commit with detailed message body describing post contents

## Team Updates (2026-03-06)

**Roy (Build & Test):** Validated projects page refresh with clean Jekyll build and dev server checks. Playwright suite has pre-existing unrelated archive test failure at `BlogArchiveTests.cs:115` (GetByRole name matching issue). Future test harness note: run.ps1 still calls net6.0 path despite project targeting net8.0.
