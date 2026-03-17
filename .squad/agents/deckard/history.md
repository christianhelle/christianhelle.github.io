# Deckard — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle
- Stack: Jekyll, Ruby 3.2+, Bundler, .NET 8 Playwright tests, custom Minima dark fork
- Posts live in `_posts/YYYY/YYYY-MM-DD-title.md`
- Dev server: `bundle exec jekyll serve --incremental` → port 4000
- Configs: `_config_dev.yml` (dev) / `_config_prod.yml` (prod)
- Deployment: push to `master` → GitHub Actions → GitHub Pages

## Learnings

### March 17, 2026: Chlogr Blog Post Brief
- **Analysis:** Examined chlogr repository architecture across 8 core modules (CLI, token resolution, API client, changelog generation, markdown formatting, models, testing, build)
- **Pattern Discovered:** Zig zero-dependency philosophy (no external HTTP library; mock data for now) mirrors clocz approach; both posts follow "problem → architecture → algorithm → code → lessons" narrative
- **Decision:** Drafted comprehensive writing brief with 11 major sections, ranked code examples by impact, identified 6 risky claims to avoid, and created 12-commit strategy for incremental narrative delivery
- **Key Insight:** chlogr's token resolver demonstrates Zig ownership patterns beautifully; changelog generator's label categorization and two-phase generation (releases + unreleased) are algorithm showcases; GitHub API integration limitation (mock data) must be front-and-center, not buried
- **Handoff:** Brief stored at `.squad/decisions/inbox/deckard-chlogr-post-brief.md` for Rachael (writer) to follow; 12 focused commits planned for detailed history trail

### January 14, 2025: Chlogr Blog Post Review (APPROVED)
- **Scope:** Full post review against chlogr source, reference posts (clocz/argiope), and Jekyll conventions
- **Code Verification:** All 8 code examples cross-referenced against source files—100% accuracy on main.zig, cli.zig, token_resolver.zig, github_api.zig, changelog_generator.zig, markdown_formatter.zig, build.zig, install.sh
- **Factual Check:** Known limitations accurately stated (HTTP mock testing, fixed pagination, unimplemented date range, no caching); no unsupported claims detected
- **Style Consistency:** Front matter correct; narrative structure matches clocz/argiope pattern (intro → how-it-works → deep dives → usage → limitations → conclusion); tag consistency (Zig, CLI)
- **Completeness:** 634-line post covers 11 major sections across orchestration, CLI, token resolution, API, generation logic, formatting, build, distribution, limitations, and conclusion
- **Commits:** Focused logical chunks observed in git history; detailed revision trail preserved
- **Verdict:** ✅ **APPROVED** — Ready for publication. No material issues. Minor polish opportunities noted (verify GitHub Actions binaries, add caveat for unimplemented flags, confirm platform binary naming)
- **Documentation:** Full review decision written to `.squad/decisions/inbox/deckard-chlogr-review.md`
