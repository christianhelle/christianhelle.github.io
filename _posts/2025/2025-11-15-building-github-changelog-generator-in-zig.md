---
layout: post
title: Building a Github Changelog Generator in Zig
date: 2025-11-15
author: Christian Helle
tags:
  - Zig
  - CLI
  - GitHub
redirect_from:
  - /2025/11/15/building-github-changelog-generator-in-zig
  - /2025/building-github-changelog-generator-in-zig
---

I built a small, focused CLI in Zig to automatically generate changelogs from GitHub tags, pull requests, and issues. The tool is compact, fast, and deliberately conservative: zero dependencies, a single static binary, and a pragmatic feature set that makes it useful for small and medium repositories.

This post walks through the project design and implementation, with concrete Zig snippets that show how I solved the tricky parts: token discovery, GitHub API interaction, grouping PRs by labels, producing tidy Markdown output, and verifying behavior using tests. The code examples are simplified to highlight the ideas; the full project is at https://github.com/christianhelle/changelog-generator.

Why a changelog generator?
- Changelogs are invaluable for users and maintainers, but maintaining them by hand is tedious.
- GitHub already contains most of the structured data we need: tags, releases, PR titles, labels, and associated issues.
- A small CLI that generates Markdown from that data can be integrated into release workflows or run locally before tagging.

What this tool does
- Fetches repository tags / releases
- Collects pull requests and issues between tags
- Categorizes entries by label (Features, Bug Fixes, Other)
- Emits a Markdown changelog with links to PRs/issues and contributors
- Resolves tokens via flag → env vars → `gh auth token`

What I'll commit in this post series:
- skeleton: metadata, intro (this commit)
- core: CLI, token resolver, models, and pagination
- usage: examples, formatting, and renderer
- tests & CI: testing approach, distribution and conclusion
