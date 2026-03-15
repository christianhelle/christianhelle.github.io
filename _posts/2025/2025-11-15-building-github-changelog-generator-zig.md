---
layout: post
title: Building a GitHub Changelog Generator in Zig
date: 2025-11-15
author: Christian Helle
tags:
  - Zig
  - CLI
  - GitHub
redirect_from:
  - /2025/11/15/building-github-changelog-generator-zig
  - /2025/11/15/building-github-changelog-generator-zig/
  - /2025/11/building-github-changelog-generator-zig
  - /2025/11/building-github-changelog-generator-zig/
  - /2025/building-github-changelog-generator-zig
  - /2025/building-github-changelog-generator-zig/
  - /building-github-changelog-generator-zig
  - /building-github-changelog-generator-zig/
---

I recently built a CLI tool to automatically generate changelogs from GitHub releases, pull requests, and issues, written in [Zig](https://ziglang.org/). The tool queries the GitHub API, groups merged pull requests by release, categorizes them by label, and writes a Markdown changelog file. I named it [changelog-generator](https://github.com/christianhelle/changelog-generator).

The source code is available on GitHub at [https://github.com/christianhelle/changelog-generator](https://github.com/christianhelle/changelog-generator).

Like my previous Zig projects, GitHub Copilot wrote most of the boilerplate, including the GitHub Actions workflows, README, install scripts, and snapcraft.yaml. The core logic took a couple of evenings to build.

## How it works

The tool follows a simple pipeline: parse command-line arguments, resolve a GitHub token, fetch releases and merged pull requests from the GitHub API, group the PRs under their corresponding releases based on merge timestamps, categorize each entry by label, and write the result to a Markdown file.

```zig
pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const args = try std.process.argsAlloc(allocator);
    defer std.process.argsFree(allocator, args);

    const cli_parser = cli.CliParser.init(allocator);
    const parsed_args = cli_parser.parse(args) catch |err| {
        if (err == error.HelpRequested) {
            cli.CliParser.printHelp();
            return;
        }
        std.debug.print("Error: {}\n", .{err});
        cli.CliParser.printHelp();
        return err;
    };

    const resolver = token_resolver.TokenResolver.init(allocator);
    const resolved_token = try resolver.resolve(parsed_args.token);
    defer resolver.deinit(resolved_token);

    var api_client = github_api.GitHubApiClient.init(
        allocator,
        resolved_token.value,
        parsed_args.owner.?,
        parsed_args.repo.?,
    );
    defer api_client.deinit();

    const releases = try api_client.getReleases();
    defer api_client.freeReleases(releases);

    const prs = try api_client.getMergedPullRequests(100);
    defer api_client.freePullRequests(prs);

    var gen = changelog_generator.ChangelogGenerator.init(allocator, parsed_args.exclude_labels);
    const changelog = try gen.generate(releases, prs);
    defer gen.deinitChangelog(changelog);

    var formatter = markdown_formatter.MarkdownFormatter.init(allocator);
    const markdown = try formatter.formatWithUnreleased(changelog.releases, changelog.unreleased);
    defer formatter.deinit(markdown);

    try formatter.writeToFile(parsed_args.output, markdown);
}
```

The entire flow is orchestrated from `main.zig`, which delegates each responsibility to a focused module. The `GeneralPurposeAllocator` is used in debug builds to catch memory leaks. In release builds, you would swap it out for a more performant allocator.

## Project Structure

The project is organized into focused modules, each with a single responsibility:

```
src/
  ├── main.zig                 # Main orchestration logic
  ├── cli.zig                  # CLI argument parsing
  ├── token_resolver.zig       # GitHub token resolution
  ├── models.zig               # Data structures
  ├── http_client.zig          # HTTP client wrapper
  ├── github_api.zig           # GitHub API integration
  ├── changelog_generator.zig  # Core changelog logic
  ├── markdown_formatter.zig   # Markdown output formatting
  ├── test_data.zig            # Mock test data
  └── test.zig                 # Integration tests
```

This separation of concerns makes each module easy to test, modify, and reason about independently.

## Data Models

The data models in `models.zig` map directly to the GitHub API response shapes. They are straightforward structs with slice fields representing JSON string values:

```zig
pub const Release = struct {
    tag_name: []const u8,
    name: []const u8,
    published_at: []const u8,
};

pub const PullRequest = struct {
    number: u32,
    title: []const u8,
    body: ?[]const u8,
    html_url: []const u8,
    user: User,
    labels: []Label,
    merged_at: ?[]const u8,
};

pub const Label = struct {
    name: []const u8,
    color: []const u8,
};

pub const User = struct {
    login: []const u8,
    html_url: []const u8,
};
```

The optional fields (`?[]const u8`) represent nullable JSON values such as `body` and `merged_at`. Using Zig's explicit optionals makes it immediately clear which fields can be absent from the API response, forcing callers to handle both cases.

## CLI Argument Parsing

The CLI parser in `cli.zig` takes a traditional approach, iterating through `args` and consuming positional values after recognized flags. The `CliArgs` struct captures all supported options:

```zig
pub const CliArgs = struct {
    owner: ?[]const u8 = null,
    repo: ?[]const u8 = null,
    token: ?[]const u8 = null,
    output: []const u8 = "CHANGELOG.md",
    since_tag: ?[]const u8 = null,
    until_tag: ?[]const u8 = null,
    exclude_labels: ?[]const u8 = null,
};
```

The `parse` function advances through the argument slice, recognizing each `--flag` and consuming the following element as its value:

```zig
pub fn parse(_: CliParser, args: []const []const u8) !CliArgs {
    var result = CliArgs{};
    var i: usize = 1; // Skip program name

    while (i < args.len) : (i += 1) {
        const arg = args[i];

        if (std.mem.eql(u8, arg, "--owner")) {
            i += 1;
            if (i >= args.len) return error.MissingOwnerValue;
            result.owner = args[i];
        } else if (std.mem.eql(u8, arg, "--repo")) {
            i += 1;
            if (i >= args.len) return error.MissingRepoValue;
            result.repo = args[i];
        } else if (std.mem.eql(u8, arg, "--output")) {
            i += 1;
            if (i >= args.len) return error.MissingOutputValue;
            result.output = args[i];
        } else if (std.mem.eql(u8, arg, "--since-tag")) {
            i += 1;
            if (i >= args.len) return error.MissingSinceTagValue;
            result.since_tag = args[i];
        } else if (std.mem.eql(u8, arg, "--exclude-labels")) {
            i += 1;
            if (i >= args.len) return error.MissingExcludeLabelsValue;
            result.exclude_labels = args[i];
        } else if (std.mem.eql(u8, arg, "--help") or std.mem.eql(u8, arg, "-h")) {
            return error.HelpRequested;
        } else {
            std.debug.print("Unknown argument: {s}\n", .{arg});
            return error.UnknownArgument;
        }
    }

    return result;
}
```

Returning `error.HelpRequested` rather than printing help directly inside the parser keeps it pure — the caller decides what to do with the error. This is a common pattern in Zig where error values carry intent without side effects.

## Token Resolution

One of the most user-friendly features is the automatic token resolution chain in `token_resolver.zig`. The tool tries four different sources in order, using the first one it finds:

1. The `--token` command-line flag
2. The `GITHUB_TOKEN` environment variable
3. The `GH_TOKEN` environment variable
4. The output of `gh auth token` (the GitHub CLI)

```zig
pub fn resolve(self: TokenResolver, provided_token: ?[]const u8) !ResolvedToken {
    if (provided_token) |token| {
        return ResolvedToken{ .value = token, .owned = false };
    }

    if (std.process.getEnvVarOwned(self.allocator, "GITHUB_TOKEN")) |token| {
        return ResolvedToken{ .value = token, .owned = true };
    } else |_| {}

    if (std.process.getEnvVarOwned(self.allocator, "GH_TOKEN")) |token| {
        return ResolvedToken{ .value = token, .owned = true };
    } else |_| {}

    if (self.getTokenFromGhCli()) |token| {
        return ResolvedToken{ .value = token, .owned = true };
    } else |_| {}

    return error.NoTokenAvailable;
}
```

The `ResolvedToken` struct carries an `owned` flag that tracks whether the token was allocated (from an env var or the CLI) and therefore needs to be freed by the caller. Tokens passed directly via `--token` reference memory owned by the argument slice, so they must not be freed.

Invoking the `gh` CLI as a subprocess is a neat trick for development environments where the GitHub CLI is already authenticated:

```zig
fn getTokenFromGhCli(self: TokenResolver) ![]const u8 {
    var child = std.process.Child.init(
        &[_][]const u8{ "gh", "auth", "token" },
        self.allocator,
    );

    child.stdout_behavior = .Pipe;
    child.stderr_behavior = .Pipe;

    try child.spawn();
    defer {
        _ = child.kill() catch {};
    }

    var stdout_buf: [1024]u8 = undefined;
    const bytes_read = try child.stdout.?.readAll(&stdout_buf);
    const stdout = stdout_buf[0..bytes_read];

    const term = try child.wait();
    if (term.Exited != 0) return error.GhCliExited;

    const token = std.mem.trim(u8, stdout, " \t\n\r");
    if (token.len == 0) return error.EmptyToken;

    return try self.allocator.dupe(u8, token);
}
```

`std.process.Child` provides full control over spawning subprocesses, capturing their output, and checking the exit code — all with Zig's standard error handling.

## HTTP Client

Rather than reimplementing an HTTP client from scratch, the `http_client.zig` module delegates to `curl` as a subprocess. This trades a bit of startup overhead for zero implementation complexity and full support for TLS, proxies, and redirects out of the box:

```zig
pub fn get(self: *HttpClient, endpoint: []const u8) ![]u8 {
    const url = try std.fmt.allocPrint(self.allocator, "{s}{s}", .{ self.base_url, endpoint });
    defer self.allocator.free(url);

    const auth_header = try std.fmt.allocPrint(
        self.allocator,
        "Authorization: token {s}",
        .{self.token},
    );
    defer self.allocator.free(auth_header);

    const args = [_][]const u8{
        "curl", "-s",
        "-H", auth_header,
        "-H", "User-Agent: changelog-generator/0.1.0",
        "-H", "Accept: application/vnd.github.v3+json",
        url,
    };

    var child = std.process.Child.init(&args, self.allocator);
    child.stdout_behavior = .Pipe;
    child.stderr_behavior = .Pipe;

    try child.spawn();

    var buffer: [10 * 1024 * 1024]u8 = undefined;
    const bytes_read = try child.stdout.?.readAll(&buffer);

    const term = try child.wait();
    if (term.Exited != 0) return error.CurlFailed;
    if (bytes_read == 0) return error.EmptyResponse;

    return try self.allocator.dupe(u8, buffer[0..bytes_read]);
}
```

The `Accept: application/vnd.github.v3+json` header ensures the GitHub API returns stable v3 JSON responses. The response is copied into an owned slice that the caller is responsible for freeing.

## GitHub API Integration

The `github_api.zig` module wraps the HTTP client with GitHub-specific API calls. It handles JSON parsing of releases and pull requests, and deep-copies all string data so that the parsed JSON arena can be freed immediately after copying:

```zig
pub fn getReleases(self: *GitHubApiClient) ![]models.Release {
    const endpoint = try std.fmt.allocPrint(
        self.allocator,
        "/repos/{s}/{s}/releases",
        .{ self.owner, self.repo },
    );
    defer self.allocator.free(endpoint);

    const response = try self.http_client.get(endpoint);
    defer self.allocator.free(response);

    if (std.mem.indexOf(u8, response, "\"message\"") != null) {
        return error.GitHubApiError;
    }

    var parsed = try std.json.parseFromSlice(
        []models.Release,
        self.allocator,
        response,
        .{ .ignore_unknown_fields = true },
    );
    defer parsed.deinit();

    var releases = try std.ArrayList(models.Release).initCapacity(
        self.allocator,
        parsed.value.len,
    );
    for (parsed.value) |release| {
        releases.appendAssumeCapacity(.{
            .tag_name = try self.allocator.dupe(u8, release.tag_name),
            .name = try self.allocator.dupe(u8, release.name),
            .published_at = try self.allocator.dupe(u8, release.published_at),
        });
    }
    return try releases.toOwnedSlice(self.allocator);
}
```

The `.ignore_unknown_fields = true` option in `std.json.parseFromSlice` is essential here. The GitHub API returns many fields per release object — keeping only the three we care about would otherwise fail to parse. This option makes the JSON parser resilient to API evolution.

Fetching merged pull requests works the same way but builds a richer data structure including labels and nested user objects:

```zig
pub fn getMergedPullRequests(self: *GitHubApiClient, per_page: u32) ![]models.PullRequest {
    const endpoint = try std.fmt.allocPrint(
        self.allocator,
        "/repos/{s}/{s}/pulls?state=closed&per_page={d}&sort=updated&direction=desc",
        .{ self.owner, self.repo, per_page },
    );
    defer self.allocator.free(endpoint);

    const response = try self.http_client.get(endpoint);
    defer self.allocator.free(response);

    var parsed = try std.json.parseFromSlice(
        []models.PullRequest,
        self.allocator,
        response,
        .{ .ignore_unknown_fields = true },
    );
    defer parsed.deinit();

    var prs = try std.ArrayList(models.PullRequest).initCapacity(
        self.allocator,
        parsed.value.len,
    );
    for (parsed.value) |pr| {
        var labels = try std.ArrayList(models.Label).initCapacity(
            self.allocator,
            pr.labels.len,
        );
        for (pr.labels) |label| {
            labels.appendAssumeCapacity(.{
                .name = try self.allocator.dupe(u8, label.name),
                .color = try self.allocator.dupe(u8, label.color),
            });
        }

        prs.appendAssumeCapacity(.{
            .number = pr.number,
            .title = try self.allocator.dupe(u8, pr.title),
            .body = if (pr.body) |b| try self.allocator.dupe(u8, b) else null,
            .html_url = try self.allocator.dupe(u8, pr.html_url),
            .user = .{
                .login = try self.allocator.dupe(u8, pr.user.login),
                .html_url = try self.allocator.dupe(u8, pr.user.html_url),
            },
            .labels = try labels.toOwnedSlice(self.allocator),
            .merged_at = if (pr.merged_at) |m| try self.allocator.dupe(u8, m) else null,
        });
    }
    return try prs.toOwnedSlice(self.allocator);
}
```

The deep-copy pattern is necessary because `parsed.deinit()` frees all the memory owned by the JSON arena. Without duplicating each string, the returned slice would contain dangling pointers.

## Changelog Generation

The heart of the tool is `changelog_generator.zig`. It takes a slice of releases and a slice of pull requests, and produces a structured `Changelog` value with entries grouped under each release.

The categorization logic maps PR labels to human-friendly section names:

```zig
fn categorizeEntry(_: ChangelogGenerator, labels: []models.Label) []const u8 {
    for (labels) |label| {
        if (std.mem.eql(u8, label.name, "feature") or
            std.mem.eql(u8, label.name, "enhancement"))
        {
            return "Features";
        } else if (std.mem.eql(u8, label.name, "bug") or
            std.mem.eql(u8, label.name, "bugfix"))
        {
            return "Bug Fixes";
        }
    }
    return "Merged Pull Requests";
}
```

The core `generate` function iterates over each release and collects PRs that were merged before that release's publish date:

```zig
pub fn generate(
    self: ChangelogGenerator,
    releases: []models.Release,
    prs: []models.PullRequest,
) !Changelog {
    var result = try std.ArrayList(ChangelogRelease).initCapacity(
        self.allocator,
        releases.len,
    );

    for (releases) |release| {
        var sections_map = std.StringHashMap(
            std.ArrayList(ChangelogEntry),
        ).init(self.allocator);
        defer {
            var it = sections_map.iterator();
            while (it.next()) |entry| {
                entry.value_ptr.deinit(self.allocator);
            }
            sections_map.deinit();
        }

        for (prs) |pr| {
            if (self.shouldExclude(pr.labels)) continue;
            if (pr.merged_at) |merged_at| {
                if (!isBefore(merged_at, release.published_at)) continue;
            } else {
                continue; // Skip unmerged PRs
            }

            const category = self.categorizeEntry(pr.labels);
            var section_list = sections_map.getOrPut(category) catch continue;
            if (!section_list.found_existing) {
                section_list.value_ptr.* = try std.ArrayList(ChangelogEntry).initCapacity(
                    self.allocator,
                    prs.len,
                );
            }

            try section_list.value_ptr.append(self.allocator, .{
                .title = pr.title,
                .url = pr.html_url,
                .author = pr.user.login,
                .number = pr.number,
            });
        }

        // Convert map to sorted sections array...
        var sections_array = try std.ArrayList(ChangelogSection).initCapacity(
            self.allocator,
            sections_map.count(),
        );
        var it = sections_map.iterator();
        while (it.next()) |entry| {
            sections_array.appendAssumeCapacity(.{
                .name = entry.key_ptr.*,
                .entries = try entry.value_ptr.toOwnedSlice(self.allocator),
            });
        }

        result.appendAssumeCapacity(.{
            .version = release.tag_name,
            .date = release.published_at,
            .sections = try sections_array.toOwnedSlice(self.allocator),
        });
    }

    // Collect unreleased PRs (merged after the most recent release)...

    return Changelog{
        .releases = try result.toOwnedSlice(self.allocator),
        .unreleased = unreleased,
    };
}
```

The `StringHashMap` groups entries into sections dynamically. The key is the category name (a static string literal), and the value is an `ArrayList` of entries. A `defer` block ensures the map and its values are cleaned up even if an error occurs partway through.

The date comparison helpers use a simple lexicographic approach that works because GitHub's `published_at` timestamps use ISO 8601 format (`2024-01-15T10:30:00Z`), which sorts correctly as strings:

```zig
fn compareDates(date1: []const u8, date2: []const u8) i32 {
    const d1 = parseDateToSlice(date1);
    const d2 = parseDateToSlice(date2);
    if (d1.len == 0 or d2.len == 0) return 0;
    return switch (std.mem.order(u8, d1, d2)) {
        .lt => -1,
        .eq => 0,
        .gt => 1,
    };
}

fn parseDateToSlice(date_str: []const u8) []const u8 {
    for (date_str, 0..) |c, i| {
        if (c == 'T') return date_str[0..i];
    }
    return date_str;
}
```

The function extracts only the date portion (`YYYY-MM-DD`) before the `T` separator, so two timestamps that fall on the same day compare as equal regardless of their time components.

## Markdown Formatting

The `markdown_formatter.zig` module converts the structured `Changelog` into a Markdown string. It builds an `ArrayList` of string parts and then concatenates them in one allocation:

```zig
pub fn formatWithUnreleased(
    self: MarkdownFormatter,
    releases: []changelog_generator.ChangelogRelease,
    unreleased: ?changelog_generator.UnreleasedChanges,
) ![]u8 {
    var parts = try std.ArrayList([]u8).initCapacity(self.allocator, 64);
    defer parts.deinit(self.allocator);

    try parts.append(self.allocator, try self.allocator.dupe(u8, "# Changelog\n\n"));

    if (unreleased) |un| {
        try parts.append(
            self.allocator,
            try self.allocator.dupe(u8, "## [Unreleased Changes]\n\n"),
        );
        for (un.sections) |section| {
            const header = try std.fmt.allocPrint(
                self.allocator,
                "### {s}\n",
                .{section.name},
            );
            try parts.append(self.allocator, header);
            for (section.entries) |entry| {
                const line = try std.fmt.allocPrint(
                    self.allocator,
                    "- {s} ([#{d}]({s})) (@{s})\n",
                    .{ entry.title, entry.number, entry.url, entry.author },
                );
                try parts.append(self.allocator, line);
            }
            try parts.append(self.allocator, try self.allocator.dupe(u8, "\n"));
        }
    }

    for (releases) |release| {
        const header = try std.fmt.allocPrint(
            self.allocator,
            "## [{s}](https://github.com/owner/repo/releases/tag/{s}) - {s}\n\n",
            .{ release.version, release.version, release.date },
        );
        try parts.append(self.allocator, header);

        for (release.sections) |section| {
            const section_header = try std.fmt.allocPrint(
                self.allocator,
                "### {s}\n",
                .{section.name},
            );
            try parts.append(self.allocator, section_header);

            for (section.entries) |entry| {
                const line = try std.fmt.allocPrint(
                    self.allocator,
                    "- {s} ([#{d}]({s})) (@{s})\n",
                    .{ entry.title, entry.number, entry.url, entry.author },
                );
                try parts.append(self.allocator, line);
            }
            try parts.append(self.allocator, try self.allocator.dupe(u8, "\n"));
        }
    }

    // Concatenate all parts into a single allocation
    var total_len: usize = 0;
    for (parts.items) |part| total_len += part.len;

    var result = try self.allocator.alloc(u8, total_len);
    var offset: usize = 0;
    for (parts.items) |part| {
        @memcpy(result[offset .. offset + part.len], part);
        offset += part.len;
        self.allocator.free(part);
    }

    return result;
}
```

Each entry is formatted as:

```
- PR title ([#123](https://github.com/owner/repo/pull/123)) (@username)
```

This links both the PR number and the author's profile, making the changelog navigable directly in GitHub or any Markdown renderer.

## Usage

The tool requires `--owner` and `--repo` to identify the repository:

```
changelog-generator --owner christianhelle --repo changelog-generator
```

This generates a `CHANGELOG.md` in the current directory. Specify a different output path with `--output`:

```
changelog-generator \
  --owner christianhelle \
  --repo changelog-generator \
  --output docs/CHANGELOG.md
```

Provide a token explicitly if it is not in your environment:

```
changelog-generator \
  --owner github \
  --repo cli \
  --token ghp_xxxxxxxxxxxx \
  --output CHANGELOG.md
```

Use `--since-tag` and `--until-tag` to generate a changelog for a specific range of versions:

```
changelog-generator \
  --owner github \
  --repo cli \
  --since-tag v2.0.0 \
  --until-tag v2.5.0
```

Exclude PRs with specific labels using `--exclude-labels`:

```
changelog-generator \
  --owner github \
  --repo cli \
  --exclude-labels "duplicate,wontfix,skip-changelog"
```

The generated output looks like this:

```markdown
# Changelog

## [Unreleased Changes]

### Features

- Add new feature X ([#201](https://github.com/owner/repo/pull/201)) (@alice)

## [v1.2.0](https://github.com/owner/repo/releases/tag/v1.2.0) - 2024-01-15

### Features

- Add new feature X ([#123](https://github.com/owner/repo/pull/123)) (@alice)

### Bug Fixes

- Fix critical bug ([#124](https://github.com/owner/repo/pull/124)) (@bob)

### Merged Pull Requests

- Update documentation ([#125](https://github.com/owner/repo/pull/125)) (@charlie)

## [v1.1.0](https://github.com/owner/repo/releases/tag/v1.1.0) - 2024-01-10

...
```

## Integration Tests

The project includes integration tests in `test.zig` that verify the core logic using mock data from `test_data.zig`. This makes it possible to run the full pipeline without hitting the real GitHub API:

```zig
pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    std.debug.print("Running integration tests...\n", .{});

    try testJsonParsing(allocator);
    try testChangelogGrouping(allocator);
    try testMarkdownFormatting(allocator);
    try testFileOutput(allocator);

    std.debug.print("All tests passed!\n", .{});
}
```

Tests are run with:

```
zig build test
```

The mock data covers releases and pull requests with various label combinations, ensuring that categorization, grouping, and formatting all produce the expected output. Having tests that run quickly without network access is especially useful during development.

## Distribution

GitHub Copilot generated the `install.sh` and `install.ps1` scripts for downloading the latest binary from GitHub Releases:

```bash
#!/usr/bin/env bash
set -e

OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case $ARCH in
    x86_64) ARCH="x86_64" ;;
    aarch64|arm64) ARCH="aarch64" ;;
    *) echo "Unsupported architecture: $ARCH"; exit 1 ;;
esac

case $OS in
    linux) PLATFORM="linux-$ARCH" ;;
    darwin) PLATFORM="macos-$ARCH" ;;
    *) echo "Unsupported OS: $OS"; exit 1 ;;
esac

URL="https://github.com/christianhelle/changelog-generator/releases/latest/download/changelog-generator-$PLATFORM"

curl -L "$URL" -o changelog-generator
chmod +x changelog-generator
sudo mv changelog-generator /usr/local/bin/

echo "changelog-generator installed successfully!"
```

For Windows:

```powershell
$ErrorActionPreference = "Stop"

$url = "https://github.com/christianhelle/changelog-generator/releases/latest/download/changelog-generator-windows-x86_64.exe"
$dest = "$env:USERPROFILE\bin\changelog-generator.exe"

New-Item -ItemType Directory -Force -Path (Split-Path $dest) | Out-Null
Invoke-WebRequest -Uri $url -OutFile $dest

Write-Host "changelog-generator installed to $dest"
Write-Host "Add $env:USERPROFILE\bin to your PATH if needed."
```

The GitHub Actions release workflow builds binaries for all supported platforms:

```yaml
jobs:
  build:
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            target: x86_64-linux-musl
          - os: ubuntu-latest
            target: aarch64-linux-musl
          - os: macos-latest
            target: x86_64-macos
          - os: macos-latest
            target: aarch64-macos
          - os: windows-latest
            target: x86_64-windows
    steps:
      - uses: actions/checkout@v4
      - uses: mlugg/setup-zig@v2
        with:
          version: 0.15.0
      - run: zig build -Dtarget=${{ matrix.target }} -Doptimize=ReleaseSafe
```

Building with `-Doptimize=ReleaseSafe` gives you the performance of a release build while retaining safety checks for things like out-of-bounds array access. The resulting binaries are single static executables with zero runtime dependencies.

## Conclusion

Building changelog-generator was a great exercise in practical Zig — spawning subprocesses, parsing JSON, managing memory with explicit allocators, and organizing a multi-module project. The tool is fast, produces a single static binary, and works on Linux, macOS, and Windows without any runtime dependencies.

If you maintain a GitHub project and want an automated changelog, give it a try. The source code is on GitHub at [https://github.com/christianhelle/changelog-generator](https://github.com/christianhelle/changelog-generator).
