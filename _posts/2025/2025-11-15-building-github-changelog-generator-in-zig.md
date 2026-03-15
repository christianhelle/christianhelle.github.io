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
  - /2025/11/15/building-github-changelog-generator-in-zig/
  - /2025/11/building-github-changelog-generator-in-zig
  - /2025/11/building-github-changelog-generator-in-zig/
  - /2025/building-github-changelog-generator-in-zig
  - /2025/building-github-changelog-generator-in-zig/
  - /building-github-changelog-generator-in-zig
  - /building-github-changelog-generator-in-zig/
---

I built a small, focused CLI in Zig to automatically generate changelogs from GitHub tags, pull requests, and issues. The tool is compact, fast, and deliberately conservative: zero dependencies, a single static binary, and a pragmatic feature set that makes it useful for small and medium repositories.

This post walks through the project design and implementation, with concrete Zig snippets that show how I solved the tricky parts: token discovery, GitHub API interaction, grouping PRs by labels, producing tidy Markdown output, and verifying behavior using tests. The code examples are simplified to highlight the ideas; the full project is on [Github](https://github.com/christianhelle/changelog-generator).

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

Section 5 — Markdown formatting
Markdown is the output format. The formatter groups entries by section and emits links with PR numbers and authors.

Example renderer:

```zig
pub fn writeChangelog(w: anytype, tag: Tag, entries: []Entry) !void {
    try w.print("# Changelog\n\n", .{});
    try w.print("## {s} - {s}\n\n", .{ tag.name, tag.date });
    const sections = groupByKind(entries);

    if (sections.Features.len > 0) {
        try w.print("### Features\n\n", .{});
        for (sections.Features) |e| {
            try w.print("- {s} ([#{d}]({s})) (@{s})\n", .{ e.title, e.number, e.url, e.author });
        }
        try w.print("\n", .{});
    }

    if (sections.BugFixes.len > 0) {
        try w.print("### Bug Fixes\n\n", .{});
        for (sections.BugFixes) |e| {
            try w.print("- {s} ([#{d}]({s})) (@{s})\n", .{ e.title, e.number, e.url, e.author });
        }
        try w.print("\n", .{});
    }

    if (sections.Other.len > 0) {
        try w.print("### Other\n\n", .{});
        for (sections.Other) |e| {
            try w.print("- {s} ([#{d}]({s})) (@{s})\n", .{ e.title, e.number, e.url, e.author });
        }
        try w.print("\n", .{});
    }
}
```

Notes

- The real project has a slightly richer renderer (linking to commits and handling contributors).
- Keeping the renderer small makes it trivial to add other formats later (HTML, JSON).

Section 6 — Tests and integration strategy
Testing a tool that calls an external API is easier when you decouple HTTP from logic. I use two layers:

- HTTP abstraction layer: `GitHubClient` that takes an `HttpClient` implementation.
- A mock `HttpClient` in tests that returns canned JSON (see `test_data.zig`).
- Integration tests exercise the end-to-end logic by loading the mock responses and asserting the formatted output.

Example test skeleton:

```zig
test "generates changelog for simple repo" {
    const allocator = std.testing.allocator;
    var mock = MockHttpClient.init(allocator, test_data.simple_repo);
    var client = GitHubClient.init(&mock);
    const entries = try collectEntries(allocator, &client, "owner", "repo", someDate, otherDate);
    var buf: [4096]u8 = undefined;
    var writer = std.io.fixedBufferStream(&buf).writer();
    try writeChangelog(&writer, testTag, entries);
    try std.testing.expectEqualStrings(expected_markdown, writer.toSlice());
}
```

Why this helps

- Tests run offline, are fast, and validate formatting and grouping without rate limits or API flakiness.
- The production `HttpClient` integrates with Zig `std` HTTP and can be used by the CLI.

Section 7 — Rate limiting and backoff

- For small repos this is rarely a problem. For larger repos, implement exponential backoff on 403 responses with X-RateLimit-Reset.
- The project currently makes conservative use of requests (paginate only when necessary) and uses a token when available.

Section 8 — Distribution and CI

- The repo provides a GitHub Actions workflow that builds Zig binaries for Linux, macOS and Windows and attaches them to releases.
- Tests run via `zig test` on the matrix to ensure formatting and core logic stay correct.
- Typical `build` step is `zig build` and `zig build test` for CI.

Section 9 — Usage examples
Basic:

```bash
./changelog-generator --owner christianhelle --repo changelog-generator --output CHANGELOG.md
```

Between tags:

```bash
./changelog-generator --owner foo --repo bar --since-tag v1.2.0 --until-tag v1.3.0 --output changelog-1.3.0.md
```

With token explicit:

```bash
./changelog-generator --owner foo --repo bar --token $GH_TOKEN --output CHANGELOG.md
```

Section 10 — Implementation pitfalls and tradeoffs

- Pagination: forgetting to follow Link headers leads to incomplete changelogs.
- Token handling: failing to provide a token quickly degrades UX in CI (GH rate limits are low for anonymous callers).
- Label strategy: label names vary across projects. Default heuristics are fine but consider per-repo config for perfect results.
- Perf: Most time is spent on network I/O; concurrent page fetching can speed up collection but complicates rate limit handling.

Conclusion
Building a changelog generator in Zig highlights the language's strengths: predictable performance, a small standard library with useful process and HTTP primitives, and the ability to produce a single static binary for distribution. The full project on GitHub contains the implementation, tests and GitHub Actions workflows if you want to inspect or reuse it.

If you'd like I can:

1. Add the post file to the repository and commit in the small increments I outlined (recommended).
2. Or produce a shorter version for the site (if you want less detail).
3. Or generate complementary README updates or code snippets if you want more code shown inline.
