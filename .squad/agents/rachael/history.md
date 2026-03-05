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
