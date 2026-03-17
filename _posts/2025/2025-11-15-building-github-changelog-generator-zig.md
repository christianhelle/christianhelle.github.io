---
layout: post
title: Building a GitHub Changelog Generator in Zig
date: 2025-11-15
author: Christian Helle
tags:
  - Zig
  - CLI
redirect_from:
  - /2025/11/building-github-changelog-generator-zig
  - /2025/11/building-github-changelog-generator-zig/
  - /2025/building-github-changelog-generator-zig
  - /2025/building-github-changelog-generator-zig/
  - /building-github-changelog-generator-zig
  - /building-github-changelog-generator-zig/
---

I recently built a command-line tool for automatically generating changelogs from GitHub releases, pull requests, and issues. Called **changelog-generator** at the time (now called [chlogr](https://github.com/christianhelle/chlogr)), the tool queries the GitHub API, categorizes merged PRs by their labels, and generates a nicely formatted Markdown changelog. It's written entirely in [Zig](https://ziglang.org/) with zero external dependencies.

The source code is available on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

As with my previous Zig projects, GitHub Copilot assisted with scaffolding the GitHub workflows, README, and installation scripts. The core logic took a few focused evenings to build. The tool demonstrates several practical patterns: CLI argument parsing, GitHub token resolution with environment variable fallbacks, HTTP-based API calls via curl, JSON parsing, date filtering, and Markdown generation.

## Motivation

Maintaining accurate changelogs is tedious. Most projects either manually edit CHANGELOG.md, which becomes outdated, or ignore changelogs entirely. GitHub's releases and PR labels already contain structured information about what changed—why not use that?

changelog-generator automates this by:
- Fetching all releases/tags from a repository
- Retrieving all merged pull requests with their labels
- Filtering and grouping PRs by semantic labels (Features, Bug Fixes, Other)
- Generating a Markdown changelog with links to PRs, issues, and contributors

The result is a changelog that's always up-to-date and requires only that you tag releases and label your PRs appropriately.

## CLI Design and Argument Parsing

The CLI is straightforward. changelog-generator accepts required `--owner` and `--repo` arguments and optional flags for output path, GitHub token, and filtering options.

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

pub const CliParser = struct {
    allocator: std.mem.Allocator,

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
            } else if (std.mem.eql(u8, arg, "--token")) {
                i += 1;
                if (i >= args.len) return error.MissingTokenValue;
                result.token = args[i];
            } else if (std.mem.eql(u8, arg, "--output")) {
                i += 1;
                if (i >= args.len) return error.MissingOutputValue;
                result.output = args[i];
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

        if (result.owner == null or result.repo == null) {
            return error.MissingRequiredArgs;
        }

        return result;
    }
};
```

The parser is a simple linear scan through arguments. Each flag consumes the next value, with validation that required values are present. Both `--owner` and `--repo` are required. The design prioritizes simplicity and clarity—no complex flag handling or abbreviations.

## Smart GitHub Token Resolution

A key usability feature is automatic token resolution. changelog-generator needs a GitHub token to access the API, but requiring users to manually pass a token on every invocation would be tedious. Instead, the tool implements a fallback chain:

```zig
pub fn resolve(self: TokenResolver, provided_token: ?[]const u8) !ResolvedToken {
    // 1. Check provided token (not owned - don't free)
    if (provided_token) |token| {
        if (token.len > 0) {
            return ResolvedToken{
                .value = token,
                .has_token = true,
                .is_owned = false,
            };
        }
    }

    // 2. Check GITHUB_TOKEN env var (owned - must free)
    if (std.process.getEnvVarOwned(self.allocator, "GITHUB_TOKEN")) |token| {
        if (token.len > 0) {
            std.debug.print("Using GITHUB_TOKEN from environment variable\n", .{});
            return ResolvedToken{
                .value = token,
                .has_token = true,
                .is_owned = true,
            };
        } else {
            self.allocator.free(token);
        }
    } else |err| {
        if (err != error.EnvironmentVariableNotFound) return err;
    }

    // 3. Check GH_TOKEN env var (owned - must free)
    if (std.process.getEnvVarOwned(self.allocator, "GH_TOKEN")) |token| {
        if (token.len > 0) {
            std.debug.print("Using GH_TOKEN from environment variable\n", .{});
            return ResolvedToken{
                .value = token,
                .has_token = true,
                .is_owned = true,
            };
        } else {
            self.allocator.free(token);
        }
    } else |err| {
        if (err != error.EnvironmentVariableNotFound) return err;
    }

    // 4. Try to get token from gh CLI (owned - must free)
    if (self.getTokenFromGhCli()) |token| {
        std.debug.print("Using token from 'gh auth token' command\n", .{});
        return ResolvedToken{
            .value = token,
            .has_token = true,
            .is_owned = true,
        };
    } else |err| {
        if (err != error.GhCliExited and err != error.EmptyToken and err != error.FileNotFound) return err;
    }

    // No token found - return error
    return error.NoTokenAvailable;
}
```

The resolver follows this priority:
1. Explicit `--token` flag
2. `GITHUB_TOKEN` environment variable
3. `GH_TOKEN` environment variable
4. Token from `gh auth token` command (if GitHub CLI is installed)
5. If no token is found, the tool exits with an error

The `ResolvedToken` struct tracks ownership, since tokens from environment variables and the `gh` command must be freed, while the provided token and empty string should not be. This prevents both use-after-free and unnecessary allocations.

## HTTP Client with curl

The API client wraps HTTP calls to GitHub's REST API. Rather than using external dependencies, changelog-generator spawns curl as a child process for HTTP requests, which is a pragmatic solution that keeps the binary lightweight and leverages a ubiquitous system utility.

```zig
pub fn get(self: HttpClient, path: []const u8) !HttpResponse {
    // Build the full URL
    const url = try std.fmt.allocPrint(self.allocator, "https://api.github.com{s}", .{path});
    defer self.allocator.free(url);

    // Prepare curl command with headers
    var cmd = try std.ArrayList([]const u8).initCapacity(self.allocator, 10);
    defer cmd.deinit(self.allocator);

    try cmd.append(self.allocator, "curl");
    try cmd.append(self.allocator, "-s");
    try cmd.append(self.allocator, "-H");
    try cmd.append(self.allocator, "Accept: application/vnd.github.v3+json");

    if (self.token.len > 0) {
        const auth_header = try std.fmt.allocPrint(self.allocator, "Authorization: Bearer {s}", .{self.token});
        defer self.allocator.free(auth_header);
        try cmd.append(self.allocator, "-H");
        try cmd.append(self.allocator, auth_header);
    }

    try cmd.append(self.allocator, "-H");
    try cmd.append(self.allocator, "User-Agent: changelog-generator/0.1.0");
    try cmd.append(self.allocator, url);

    // Spawn curl process
    var child = std.process.Child.init(try cmd.toOwnedSlice(self.allocator), self.allocator);
    child.stdout_behavior = .Pipe;

    _ = try child.spawnAndWait();

    // Read response body from stdout
    const body = try child.stdout.?.readToEndAlloc(self.allocator, 1024 * 1024);

    return HttpResponse{
        .status = .ok, // curl exit code indicates success
        .body = body,
    };
}
```

This approach spawns curl with appropriate headers and collects its output. While it adds a runtime dependency on curl (not a Zig dependency), curl is almost universally available on Unix-like systems and Windows through various package managers. The trade-off keeps the Zig code simple and avoids adding a Zig HTTP library dependency.

## GitHub API Integration

Now we have the HTTP mechanism, we can fetch actual GitHub data. The API client uses the `owner` and `repo` parameters to construct endpoints for releases and merged PRs:

```zig
pub fn getMergedPullRequests(self: *GitHubApiClient, per_page: u32) ![]models.PullRequest {
    const endpoint = try std.fmt.allocPrint(self.allocator, "/repos/{s}/{s}/pulls?state=closed&per_page={d}&sort=updated&direction=desc", .{ self.owner, self.repo, per_page });
    defer self.allocator.free(endpoint);

    const response = try self.http_client.get(endpoint);
    defer self.allocator.free(response.body);

    if (response.status != .ok) {
        return error.GitHubApiError;
    }

    var parsed = try std.json.parseFromSlice(
        []models.PullRequest,
        self.allocator,
        response.body,
        .{ .ignore_unknown_fields = true },
    );
    defer parsed.deinit();

    // Deep copy PRs with string and struct duplication
    var prs = try std.ArrayList(models.PullRequest).initCapacity(self.allocator, parsed.value.len);
    for (parsed.value) |pr| {
        // Copy labels
        var labels = try std.ArrayList(models.Label).initCapacity(self.allocator, pr.labels.len);
        for (pr.labels) |label| {
            labels.appendAssumeCapacity(.{
                .name = try self.allocator.dupe(u8, label.name),
                .color = try self.allocator.dupe(u8, label.color),
            });
        }

        prs.appendAssumeCapacity(.{
            .number = pr.number,
            .title = try self.allocator.dupe(u8, pr.title),
            .body = if (pr.body) |body| try self.allocator.dupe(u8, body) else null,
            .html_url = try self.allocator.dupe(u8, pr.html_url),
            .user = .{
                .login = try self.allocator.dupe(u8, pr.user.login),
                .html_url = try self.allocator.dupe(u8, pr.user.html_url),
            },
            .labels = try labels.toOwnedSlice(self.allocator),
            .merged_at = if (pr.merged_at) |merged| try self.allocator.dupe(u8, merged) else null,
        });
    }
    return try prs.toOwnedSlice(self.allocator);
}
```

Note the query parameter `state=closed` to fetch closed PRs (which includes merged ones) and `sort=updated&direction=desc` to get the most recently updated PRs first. This is practical filtering—we want merged PRs, not just closed ones, and sorting by recency helps with pagination.

## Changelog Generation and Categorization

The changelog generator takes releases and pull requests, then groups PRs by release based on merge date and categorizes them by label.

```zig
pub fn generate(
    self: ChangelogGenerator,
    releases: []models.Release,
    prs: []models.PullRequest,
) !Changelog {
    var result = try std.ArrayList(ChangelogRelease).initCapacity(self.allocator, releases.len);

    for (releases) |release| {
        var sections_map = std.StringHashMap(std.ArrayList(ChangelogEntry)).init(self.allocator);
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
                continue;
            }

            const category = self.categorizeEntry(pr.labels);

            var section_list = sections_map.getOrPut(category) catch continue;
            if (!section_list.found_existing) {
                const arr = try std.ArrayList(ChangelogEntry).initCapacity(self.allocator, prs.len);
                section_list.value_ptr.* = arr;
            }

            const entry = ChangelogEntry{
                .title = pr.title,
                .url = pr.html_url,
                .author = pr.user.login,
                .number = pr.number,
            };

            try section_list.value_ptr.append(self.allocator, entry);
        }

        var sections_array = try std.ArrayList(ChangelogSection).initCapacity(self.allocator, sections_map.count());

        var it = sections_map.iterator();
        while (it.next()) |entry| {
            const changelog_section = ChangelogSection{
                .name = entry.key_ptr.*,
                .entries = try entry.value_ptr.toOwnedSlice(self.allocator),
            };
            sections_array.appendAssumeCapacity(changelog_section);
        }

        const release_entry = ChangelogRelease{
            .version = release.tag_name,
            .date = release.published_at,
            .sections = try sections_array.toOwnedSlice(self.allocator),
        };

        result.appendAssumeCapacity(release_entry);
    }

    return Changelog{
        .releases = try result.toOwnedSlice(self.allocator),
    };
}
```

The algorithm is:
1. For each release, iterate through all PRs
2. Filter out excluded labels and check if the PR was merged before the release date
3. Categorize the PR based on its labels
4. Add it to a hash map under that category
5. Convert the category entries into sections

Categorization logic is simple:

```zig
fn categorizeEntry(_: ChangelogGenerator, labels: []models.Label) []const u8 {
    for (labels) |label| {
        if (std.mem.eql(u8, label.name, "feature") or std.mem.eql(u8, label.name, "enhancement")) {
            return "Features";
        } else if (std.mem.eql(u8, label.name, "bug") or std.mem.eql(u8, label.name, "bugfix")) {
            return "Bug Fixes";
        }
    }
    return "Merged Pull Requests";
}
```

Anything labeled "feature" or "enhancement" goes into Features. Anything labeled "bug" or "bugfix" goes into Bug Fixes. Everything else goes into a default "Merged Pull Requests" section.

## Markdown Formatting

The formatter converts the structured changelog into a readable Markdown document.

```zig
pub fn format(
    self: MarkdownFormatter,
    releases: []changelog_generator.ChangelogRelease,
) ![]u8 {
    var parts = try std.ArrayList([]u8).initCapacity(self.allocator, releases.len * 20);
    defer parts.deinit(self.allocator);

    try parts.append(self.allocator, try self.allocator.dupe(u8, "# Changelog\n\n"));

    for (releases) |release| {
        const header = try std.fmt.allocPrint(self.allocator, "## [{s}](https://github.com/owner/repo/releases/tag/{s}) - {s}\n\n", .{
            release.version,
            release.version,
            release.date,
        });
        try parts.append(self.allocator, header);

        for (release.sections) |section| {
            const section_header = try std.fmt.allocPrint(self.allocator, "### {s}\n", .{section.name});
            try parts.append(self.allocator, section_header);

            for (section.entries) |entry| {
                const entry_line = try std.fmt.allocPrint(self.allocator, "- {s} ([#{d}]({s})) (@{s})\n", .{
                    entry.title,
                    entry.number,
                    entry.url,
                    entry.author,
                });
                try parts.append(self.allocator, entry_line);
            }

            try parts.append(self.allocator, try self.allocator.dupe(u8, "\n"));
        }

        try parts.append(self.allocator, try self.allocator.dupe(u8, "\n"));
    }

    // Concatenate all parts into a single string
    var total_len: usize = 0;
    for (parts.items) |part| {
        total_len += part.len;
    }

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

Rather than building a single large buffer or using a string builder, we collect all formatted lines in an `ArrayList` and then concatenate them once. This avoids repeated reallocations if using a growing buffer.

Each entry is formatted as a bullet point with the PR title, number (as a link), and author handle:
```
- Add user authentication (#123) (@alice)
```

## Example Usage and Output

Using changelog-generator is straightforward:

```bash
./zig-out/bin/changelog-generator --owner github --repo cli --output CHANGELOG.md
```

If you have a `GITHUB_TOKEN` environment variable set (common in CI/CD), it will use that automatically. Otherwise, it attempts to use the `gh` CLI's token.

The generated CHANGELOG.md looks like this:

```markdown
# Changelog

## [v2.5.0](https://github.com/github/cli/releases/tag/v2.5.0) - 2025-10-15

### Features

- Add API v4 support ([#400](https://github.com/github/cli/pull/400)) (@alice)

### Bug Fixes

- Fix incorrect header parsing ([#401](https://github.com/github/cli/pull/401)) (@bob)

### Merged Pull Requests

- Update documentation ([#402](https://github.com/github/cli/pull/402)) (@charlie)

## [v2.4.0](https://github.com/github/cli/releases/tag/v2.4.0) - 2025-09-20

...
```

## Building and Testing

The project uses Zig's built-in build system:

```bash
zig build
```

This compiles the binary to `zig-out/bin/changelog-generator`. The build configuration is minimal:

```zig
const exe = b.addExecutable(.{
    .name = "changelog-generator",
    .root_module = b.createModule(.{
        .root_source_file = b.path("src/main.zig"),
        .target = target,
        .optimize = optimize,
    }),
});

b.installArtifact(exe);
```

Testing uses a separate test executable with mock GitHub data:

```bash
zig build test
```

The test module (`src/test.zig`) provides mock JSON responses for releases and pull requests, allowing the changelog generator to be tested without hitting the real GitHub API. Key test scenarios include:

- JSON parsing of releases and PRs with various label combinations
- Categorization logic (feature → Features, bug → Bug Fixes, unlabeled → Merged Pull Requests)
- Changelog grouping by release date
- Markdown formatting with proper links and formatting
- Label exclusion filters

## Key Design Decisions

**Curl for HTTP**: Rather than implementing low-level HTTP details in Zig or adding a dependency, changelog-generator spawns curl as a subprocess. This keeps the binary lightweight, the code simple, and leverages a ubiquitous system utility. The trade-off is a runtime dependency on curl, which is available on almost all Unix-like systems and Windows.

**Pure Zig, minimal dependencies**: This is a conscious constraint for the Zig code itself. The binary has zero Zig dependencies, making it truly standalone and easy to distribute. While we depend on curl at runtime, we don't add any Zig library dependencies, which forced thoughtful API design—every feature had to justify its existence without the convenience of a third-party Zig library.

**Mock-based testing**: Using mock data instead of hitting the real GitHub API during tests makes the test suite fast and deterministic. It also doesn't require a valid GitHub token to run tests.

**Label-driven categorization**: Assuming your PRs are labeled is reasonable. Most teams using GitHub already enforce labeling as part of PR review. This makes categorization automatic and maintainable.

**Deep copy after JSON parsing**: Zig's JSON parser works with borrowed references. Deep copying every string field after parsing adds a small cost but prevents subtle use-after-free bugs that are hard to debug.

## Lessons Learned

**Memory management is explicit**: Zig forces you to think about allocation ownership. Every slice, string, and array you allocate must be explicitly freed. This is tedious at first but prevents entire categories of bugs. The benefit is worth the friction.

**Zig's string handling is minimal by design**: There's no built-in String type—everything is `[]const u8` or `[]u8` slices. This takes adjustment coming from languages with String types, but it's actually liberating. Strings are just bytes, and you manage their lifetime explicitly.

**GitHub's API is well-designed for this use case**: The combination of releases (for version markers) and PR metadata (for change description and categorization) maps naturally to changelog structure. The API is straightforward to query with standard HTTP.

**Allocator patterns matter**: Patterns like deep copying, deferred cleanup, and arena allocators become important when manual memory management is explicit. Consistency in who "owns" each allocation prevents bugs and makes code reviews easier.

## Limitations and Future Work

The current implementation has a few constraints worth noting:

**No pagination for large repositories**: The tool fetches a fixed number of PRs (100 by default). Repositories with thousands of PRs would need pagination logic to capture all changes.

**Date filtering limited**: The `--since-tag` and `--until-tag` flags are defined in the CLI but not yet fully implemented in the core changelog generation logic.

**No caching**: Every run makes fresh API calls to GitHub. Adding caching would improve performance for frequent runs, especially during development and testing.

**Curl requirement**: The tool requires curl to be installed and available in the system PATH. While this is nearly universal on Unix-like systems, it's worth documenting as a runtime dependency.

## Conclusion

changelog-generator demonstrates practical CLI tool development in Zig. It combines several real-world patterns: environment-aware configuration, authenticated API calls, structured data processing, and formatted output generation. The code is straightforward to read and modify, and the tool is immediately useful for teams already using GitHub releases and PR labels.

If you maintain a project with git tags and labeled pull requests, changelog-generator can automate changelog generation. The project was later renamed to chlogr as the tool evolved, gaining features like unreleased changes tracking and improved HTTP handling. The source code is available at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).
