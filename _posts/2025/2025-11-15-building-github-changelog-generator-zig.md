---
layout: post
title: Building a GitHub Changelog Generator in Zig
date: 2025-11-15
author: Christian Helle
tags:
  - Zig
  - CLI
---

I recently built a command-line tool for automatically generating changelogs from GitHub releases, pull requests, and issues. Named [chlogr](https://github.com/christianhelle/chlogr), the tool queries the GitHub API, categorizes merged PRs by their labels, and generates a nicely formatted Markdown changelog. It's written entirely in [Zig](https://ziglang.org/) with zero external dependencies.

The source code is available on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

As with my previous Zig projects, GitHub Copilot assisted with scaffolding the GitHub workflows, README, and installation scripts. The core logic took a few focused evenings to build. The tool demonstrates several practical patterns: CLI argument parsing, GitHub token resolution with environment variable fallbacks, HTTP-based API calls, JSON parsing, date filtering, and Markdown generation.

## Motivation

Maintaining accurate changelogs is tedious. Most projects either manually edit CHANGELOG.md, which becomes outdated, or ignore changelogs entirely. GitHub's releases and PR labels already contain structured information about what changed—why not use that?

chlogr automates this by:
- Fetching all releases/tags from a repository
- Retrieving all merged pull requests with their labels
- Filtering and grouping PRs by semantic labels (Features, Bug Fixes, Other)
- Generating a Markdown changelog with links to PRs, issues, and contributors

The result is a changelog that's always up-to-date and requires only that you tag releases and label your PRs appropriately.

## CLI Design and Argument Parsing

The CLI is straightforward. chlogr accepts a required `--repo` argument in the format `owner/repository` and optional flags for output path, GitHub token, and filtering options.

```zig
pub const CliArgs = struct {
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

            if (std.mem.eql(u8, arg, "--repo")) {
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

        return result;
    }
};
```

The parser is a simple linear scan through arguments. Each flag consumes the next value, with validation that required values are present. The design prioritizes simplicity and clarity—no complex flag handling or abbreviations.

## Smart GitHub Token Resolution

A key usability feature is automatic token resolution. chlogr needs a GitHub token to access the API, but requiring users to manually pass a token on every invocation would be tedious. Instead, the tool implements a fallback chain:

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

    // No token found - return empty token but don't error
    std.debug.print("No GitHub token provided or found - proceeding without token (may have lower rate limits)\n", .{});
    return ResolvedToken{
        .value = "",
        .has_token = false,
        .is_owned = false,
    };
}
```

The resolver follows this priority:
1. Explicit `--token` flag
2. `GITHUB_TOKEN` environment variable
3. `GH_TOKEN` environment variable
4. Token from `gh auth token` command (if GitHub CLI is installed)
5. Anonymous access (works for public repos but with stricter rate limits)

The `ResolvedToken` struct tracks ownership, since tokens from environment variables and the `gh` command must be freed, while the provided token and empty string should not be. This prevents both use-after-free and unnecessary allocations.

## GitHub API Client

The API client wraps HTTP calls to GitHub's REST API. It handles JSON parsing, error responses, and memory management for fetched data.

```zig
pub fn getReleases(self: *GitHubApiClient) ![]models.Release {
    const endpoint = try std.fmt.allocPrint(self.allocator, "/repos/{s}/releases", .{self.repo});
    defer self.allocator.free(endpoint);

    const response = try self.http_client.get(endpoint);
    defer self.allocator.free(response.body);

    if (response.status != .ok) {
        return error.GitHubApiError;
    }

    // Parse JSON response with ignoring unknown fields
    var parsed = try std.json.parseFromSlice(
        []models.Release,
        self.allocator,
        response.body,
        .{ .ignore_unknown_fields = true },
    );
    defer parsed.deinit();

    // Deep copy releases with string duplication
    var releases = try std.ArrayList(models.Release).initCapacity(self.allocator, parsed.value.len);
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

The key detail is that we parse JSON with `.ignore_unknown_fields = true`. GitHub's API response contains many fields we don't need—release body, prerelease status, asset information, etc. By ignoring unknown fields, the parser only extracts the fields our `Release` struct declares, simplifying the code and reducing sensitivity to future API changes.

After parsing, we perform a deep copy, duplicating every string field using `allocator.dupe()`. This is necessary because the JSON parser holds references into the response body string, which we then free. By duplicating, we ensure the parsed data remains valid after the response is deallocated.

```zig
pub fn getMergedPullRequests(self: *GitHubApiClient, per_page: u32) ![]models.PullRequest {
    const endpoint = try std.fmt.allocPrint(self.allocator, "/repos/{s}/pulls?state=closed&per_page={d}&sort=updated&direction=desc", .{ self.repo, per_page });
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

The changelog generator takes releases and pull requests, then groups PRs by release based on merge date and categorizes them by label. It also collects "unreleased" changes—merged PRs that don't yet belong to any tagged release.

```zig
pub fn generate(
    self: ChangelogGenerator,
    releases: []models.Release,
    prs: []models.PullRequest,
) !Changelog {
    var result = try std.ArrayList(ChangelogRelease).initCapacity(self.allocator, releases.len);

    var last_release_date: []const u8 = "";
    if (releases.len > 0) {
        last_release_date = releases[0].published_at;
    }

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

        if (compareDates(release.published_at, last_release_date) > 0) {
            last_release_date = release.published_at;
        }
    }

    // Generate unreleased section...
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

The unreleased section collects PRs merged after the most recent release:

```zig
var unreleased_sections_map = std.StringHashMap(std.ArrayList(ChangelogEntry)).init(self.allocator);
defer {
    var it = unreleased_sections_map.iterator();
    while (it.next()) |entry| {
        entry.value_ptr.deinit(self.allocator);
    }
    unreleased_sections_map.deinit();
}

var has_unreleased = false;
for (prs) |pr| {
    if (self.shouldExclude(pr.labels)) continue;
    if (pr.merged_at) |merged_at| {
        if (!isAfter(merged_at, last_release_date)) continue;
    } else {
        continue;
    }

    has_unreleased = true;
    const category = self.categorizeEntry(pr.labels);
    // ... add to unreleased_sections_map
}
```

## Markdown Formatting

The formatter converts the structured changelog into a readable Markdown document.

```zig
pub fn formatWithUnreleased(
    self: MarkdownFormatter,
    releases: []changelog_generator.ChangelogRelease,
    unreleased: ?changelog_generator.UnreleasedChanges,
) ![]u8 {
    var parts = try std.ArrayList([]u8).initCapacity(self.allocator, total_items + 20);
    defer parts.deinit(self.allocator);

    try parts.append(self.allocator, try self.allocator.dupe(u8, "# Changelog\n\n"));

    if (unreleased) |un| {
        try parts.append(self.allocator, try self.allocator.dupe(u8, "## [Unreleased Changes]\n\n"));

        for (un.sections) |section| {
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

Using chlogr is straightforward:

```bash
./zig-out/bin/chlogr --repo github/cli --output CHANGELOG.md
```

If you have a `GITHUB_TOKEN` environment variable set (common in CI/CD), it will use that automatically. Otherwise, it attempts to use the `gh` CLI's token.

The generated CHANGELOG.md looks like this:

```markdown
# Changelog

## [Unreleased Changes]

### Features

- Add support for custom headers ([#456](https://github.com/github/cli/pull/456)) (@alice)
- Implement async job processing ([#457](https://github.com/github/cli/pull/457)) (@bob)

### Bug Fixes

- Fix rate limit handling ([#458](https://github.com/github/cli/pull/458)) (@charlie)

## [v2.5.0](https://github.com/github/cli/releases/tag/v2.5.0) - 2025-10-15

### Features

- Add API v4 support ([#400](https://github.com/github/cli/pull/400)) (@alice)

### Bug Fixes

- Fix incorrect header parsing ([#401](https://github.com/github/cli/pull/401)) (@bob)

### Other

- Update documentation ([#402](https://github.com/github/cli/pull/402)) (@charlie)

## [v2.4.0](https://github.com/github/cli/releases/tag/v2.4.0) - 2025-09-20

...
```

## Building and Testing

The project uses Zig's built-in build system:

```bash
zig build
```

This compiles the binary to `zig-out/bin/chlogr`. The build configuration is minimal:

```zig
const exe = b.addExecutable(.{
    .name = "chlogr",
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
- Categorization logic (feature → Features, bug → Bug Fixes, unlabeled → Other)
- Changelog grouping by release date
- Markdown formatting with proper links and formatting
- Unreleased changes detection
- Label exclusion filters

## Key Design Decisions

**No external HTTP library**: Zig's standard library provides low-level HTTP primitives, but no high-level HTTP client. Rather than adding a dependency, chlogr wraps the basic functionality needed for GET requests with the `Authorization` header.

**Pure Zig, zero dependencies**: This is a conscious constraint. It makes the binary truly standalone and easy to distribute. It also forced thoughtful API design—every feature had to justify its existence without the convenience of a third-party library.

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

**Mock data only**: The project currently uses mock GitHub API responses for testing. Real API integration is pending, but the structure is in place.

**No pagination for large repositories**: The tool fetches a fixed number of PRs (100 by default). Repositories with thousands of PRs would need pagination logic.

**Date filtering limited**: The `--since-tag` and `--until-tag` flags are defined but not yet implemented in the core logic.

**No caching**: Every run makes fresh API calls. Adding caching would improve performance for frequent runs.

## Conclusion

chlogr demonstrates practical CLI tool development in Zig. It combines several real-world patterns: environment-aware configuration, authenticated API calls, structured data processing, and formatted output generation. The ~800 lines of code are straightforward to read and modify, and the tool is immediately useful for teams already using GitHub releases and PR labels.

If you maintain a project with git tags and labeled pull requests, chlogr can automate changelog generation and keep it up-to-date with your releases. The source code is available at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr), and the binary is ready to build and use.
