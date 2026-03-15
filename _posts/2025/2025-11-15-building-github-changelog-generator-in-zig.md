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

Section 1 — CLI and options
I keep the CLI intentionally simple and explicit. Here is a small example of parsing minimal options:

```zig
const std = @import("std");

pub const Options = struct {
    owner: []const u8,
    repo: []const u8,
    token: ?[]const u8,
    output: []const u8 = "CHANGELOG.md",
    since_tag: ?[]const u8 = null,
    until_tag: ?[]const u8 = null,
    verbose: bool = false,
};

pub fn parseArgs(allocator: std.mem.Allocator, argv: [][]const u8) !Options {
    var opts = Options{};
    var i: usize = 1;
    while (i < argv.len) : (i += 1) {
        const a = argv[i];
        if (std.mem.eql(u8, a, "--owner")) {
            i += 1;
            opts.owner = argv[i];
        } else if (std.mem.eql(u8, a, "--repo")) {
            i += 1;
            opts.repo = argv[i];
        } else if (std.mem.eql(u8, a, "--token")) {
            i += 1;
            opts.token = argv[i];
        } else if (std.mem.eql(u8, a, "--output")) {
            i += 1;
            opts.output = argv[i];
        } else if (std.mem.eql(u8, a, "--verbose")) {
            opts.verbose = true;
        }
    }
    return opts;
}
```

Explanation
- The parser is intentionally small — only what we need. Options map 1:1 to flags so it's easy to reason about scripting.
- Defaults: `CHANGELOG.md` and `verbose` false. Token is optional because we'll try environment variables / `gh` fallback.

Section 2 — Token resolution (a surprisingly important UX detail)
People run tools in many environments: CI, local shell with `GH_TOKEN`, or systems where they use `gh auth login`. The tool attempts token resolution using this order:

1. `--token` flag
2. `GITHUB_TOKEN` env var
3. `GH_TOKEN` env var
4. `gh auth token` (fallback to GitHub CLI)

The essential part is running `gh auth token` and capturing stdout if needed. Here's the resolver:

```zig
const std = @import("std");

pub fn resolveToken(allocator: std.mem.Allocator, flag_token: ?[]const u8) !?[]u8 {
    if (flag_token) return flag_token;
    const env = std.os.getenv("GITHUB_TOKEN");
    if (env) return env.*;
    const env2 = std.os.getenv("GH_TOKEN");
    if (env2) return env2.*;

    // Fallback to `gh auth token`
    var ghexec = try std.ChildProcess.init(allocator);
    defer ghexec.deinit();
    ghexec.argv = &[_][]const u8{"gh", "auth", "token"};
    const result = try ghexec.spawnAndWait();
    if (result.exit_code == 0 and result.stdout.len > 0) {
        // Trim newline
        var s = result.stdout;
        if (s.len > 0 and s[s.len - 1] == '\n') s = s[0..s.len - 1];
        return s;
    }
    return null;
}
```

Notes
- We return null when no token is found — the caller can decide whether to proceed unauthenticated (public data) or error.
- Using `gh` improves UX for many local devs; using `std.ChildProcess` is straightforward and keeps us dependency-free.

Section 3 — GitHub API: pagination, rate limits, and models

GitHub responses are paginated. I prefer a small, reusable HTTP wrapper that repeatedly follows `Link` headers until all pages are read. This keeps higher-level code concise.

A small paging helper:

```zig
pub fn fetchAllPaged(allocator: std.mem.Allocator, client: *http.Client, url: []const u8, token: ?[]const u8) ![]u8 {
    var bufList: std.ArrayListUnmanaged(u8) = .empty;
    var next = allocator.dupe(u8, url) catch return error.OutOfMemory;
    defer allocator.free(next);

    while (true) {
        const resp = try client.get(allocator, next, token);
        try bufList.appendSlice(allocator, resp.body);
        const link = resp.headers.get("link");
        if (link) {
            const nextUrl = parseLinkHeaderForNext(link.*) orelse break;
            allocator.free(next);
            next = try allocator.dupe(u8, nextUrl);
            continue;
        } else {
            break;
        }
    }
    return bufList.toOwnedSlice(allocator);
}
```

Models
- Tag: name, commit sha, date
- PullRequest: number, title, labels, author, merged_at
- Issue: number, title, labels, author, closed_at
- Entry: unified view with type (feature/fix/other), title, link, contributors

Section 4 — Collating PRs between two tags
The basic flow for generating a changelog between tags is:

1. Resolve `since_tag` (or latest tag) and `until_tag` (or current HEAD/release).
2. Get commit dates for these tags.
3. Fetch PRs merged between those dates (use search/PR list APIs with `state=closed` and `merged:true`).
4. Deduplicate PRs (sometimes multiple refs) and group by labels.

Example: fetching PRs and turning them into entries:

```zig
pub fn prToEntry(pr: PullRequest) Entry {
    var entry = Entry{
        .kind = classifyByLabels(pr.labels),
        .title = pr.title,
        .number = pr.number,
        .url = pr.html_url,
        .author = pr.user.login,
    };
    return entry;
}

pub fn collectEntries(allocator: std.mem.Allocator, client: *GitHubClient, owner: []const u8, repo: []const u8, since: DateTime, until: DateTime) ![]Entry {
    var prs = try client.listPRs(allocator, owner, repo, since, until);
    var entries: std.ArrayListUnmanaged(Entry) = .empty;
    for (prs) |pr| {
        if (pr.merged_at == null) continue;
        try entries.append(allocator, prToEntry(pr));
    }
    return entries.toOwnedSlice(allocator);
}
```

Label categorization
- I map labels to buckets using a simple ruleset:
  - If label contains "feature" or "enhancement" → Features
  - If label contains "fix" or "bug" → Bug Fixes
  - Otherwise → Other

This simple approach works well in most projects. You can allow users to pass a label map via config if needed.
