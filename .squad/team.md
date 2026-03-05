# Squad Team

> blog

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Deckard | Lead | .squad/agents/deckard/charter.md | ✅ Active |
| Rachael | Content Dev | .squad/agents/rachael/charter.md | ✅ Active |
| Pris | Jekyll Dev | .squad/agents/pris/charter.md | ✅ Active |
| Roy | Tester | .squad/agents/roy/charter.md | ✅ Active |
| Scribe | Session Logger | .squad/agents/scribe/charter.md | ✅ Active |
| Ralph | Work Monitor | — | ✅ Active |

## Project Context

- **Project:** blog
- **User:** Christian Helle
- **Created:** 2026-03-05
- **Stack:** Jekyll, GitHub Pages, Ruby 3.2+, Bundler, .NET 8 Playwright tests
- **Domain:** Programming blog — software development, REST APIs, code generation, developer tools
- **Hosting:** GitHub Pages, auto-deploy from `master` via GitHub Actions
- **Theme:** Custom Minima fork with dark skin (https://github.com/christianhelle/minima)
- **Posts:** `_posts/YYYY/YYYY-MM-DD-title.md`
- **Tests:** `tests/playwright/` (.NET Playwright, requires dev server on port 4000)
- **Dev server:** `bundle exec jekyll serve --incremental` → http://127.0.0.1:4000/
- **Build:** `bundle exec jekyll build` (~5s)
- **Config:** `_config_dev.yml` (dev) / `_config_prod.yml` (prod)
