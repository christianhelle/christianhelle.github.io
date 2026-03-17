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

### 2026-03-17: Chlogr GitHub Changelog Generator Post
- Created comprehensive 634-line blog post documenting the chlogr project in depth
- Post structure: intro → CLI parsing → token resolution → GitHub API → changelog generation → Markdown formatting → usage → build/test → distribution → limitations/future work → conclusion
- Each major section pairs substantial Zig code examples (100-300 lines) with explanatory text
- Code patterns documented: memory ownership tracking (is_owned flag on token resolver), error handling with error union types, HashMap-based grouping and conversion to slices, pre-calculated size concatenation for Markdown output
- Token resolver demonstrates fallback chain: flag → GITHUB_TOKEN env → GH_TOKEN env → `gh auth token` subprocess → anonymous (all with proper memory cleanup tracking)
- API client shows JSON parsing with `ignore_unknown_fields`, deep-copy pattern from parsed temporary data to owned allocations
- Changelog generator uses StringHashMap for category grouping, date comparison via ISO 8601 string slicing, filtering logic for labels
- Usage section includes realistic command-line examples, error messages, and generated Markdown output
- Grounded entirely in actual source (all examples from chlogr repository)
- Learnings: Each Zig project post emphasizes different implementation aspects: clocz focused on parallelism, Argiope on HTTP/HTML parsing, chlogr on auth/API/data transformation
- Updated projects.md with 4 new projects: Argiope, CLOCZ, Azure SDK for Zig, Otaku
- Search method: Use GitHub search API `user:christianhelle sort:updated` to get recent repositories
- Filtering criteria: Exclude forks (fork: false), exclude archived projects, include only active creator/maintainer repos
- New projects were added at the end of the list, preserving existing items (Christian requested keeping existing items)
- Projects selected for public-facing projects page: Must have meaningful descriptions and represent Christian's current work focus
- Zig projects trending: Argiope and CLOCZ both recent Zig projects, indicating renewed focus on the language
- Projects page serves as portfolio piece, so descriptions emphasize practical value and technical highlights

## Team Updates (2026-03-06)

**Roy (Build & Test):** Validated projects page refresh with clean Jekyll build and dev server checks. Playwright suite has pre-existing unrelated archive test failure at `BlogArchiveTests.cs:115` (GetByRole name matching issue). Future test harness note: run.ps1 still calls net6.0 path despite project targeting net8.0.

### 2026-03-17: Chlogr Polish Pass
- Applied tight factual/polish corrections to align post with actual chlogr implementation
- Key corrections made:
  * **Intro**: Removed claim about "issues" support; clarified tool fetches "releases and merged pull requests"
  * **How it works**: Changed "data from GitHub API" to "GitHub releases and merged pull requests from the API" for clarity
  * **API section**: Clarified distinction between GitHub Releases API vs raw tags; removed "closed issues" claim
  * **Usage section**: Added caveat that `--since-tag` and `--until-tag` flags are parsed but not yet wired into generator
  * **Distribution section**: Replaced placeholder install scripts with actual current versions from chlogr repo (including proper tar.gz/zip handling, error checking, PATH management)
  * **Known Limitations**: Expanded from 5 items to 8 items with specific constraints (releases-only requirement, pagination limit, placeholder links, unimplemented flags, substring label matching, per-release overlap, no caching, basic HTTP client)
- All corrections verified against actual source artifacts (install.sh, install.ps1, .github/workflows/release.yml, source code)
- Followed review guidance from Deckard and Roy validation briefs
- Made single focused commit with clear message and Copilot co-author trailer
- Polish pass preserved title exactly and kept scope to post file only
- Learnings: Polish passes must balance accuracy against practical concerns—limitation docs should be specific enough to be actionable (user won't hit "pagination" problem without knowing the fixed 100-per-page limit)

## Team Updates (2026-03-17)

**Orchestration (2026-03-17T13:02:00Z):** Draft comprehensive post  
**Orchestration (2026-03-17T13:10:00Z):** Polish pass with factual corrections  
**Rachael Summary:** Coordinated chlogr blog post drafting and polish. Draft created 634-line post with 11 sections and verified code examples grounded entirely in source. Polish pass applied 5 factual corrections and expanded limitations from 5 to 8 specific constraints. Commits: 5fdafad (draft), b87af59 (polish).

**Team Context:**
- Deckard (12:45, 13:08 UTC): Writing brief established 11-section roadmap. Review gate verified all code against source, approved for publication.
- Roy (13:05, 13:14 UTC): Pre-draft validation identified 8 claims and 8 caveats. Final sweep: all checks passed.
- Pris (13:12 UTC): README update (commit ff8e640).
- Orchestration Log: All agent activities recorded in `.squad/orchestration-log/`.
- Decision merged: `.squad/decisions.md` updated with comprehensive chlogr decision entry.
