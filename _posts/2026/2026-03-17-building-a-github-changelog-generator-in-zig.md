---
layout: post
title: Building a Github Changelog Generator in Zig
date: 2025-11-25
author: Christian Helle
tags:
  - Zig
  - CLI
redirect_from:
  - /2026/03/17/building-a-github-changelog-generator-in-zig
  - /2026/03/17/building-a-github-changelog-generator-in-zig/
  - /2026/03/building-a-github-changelog-generator-in-zig
  - /2026/03/building-a-github-changelog-generator-in-zig/
  - /2026/building-a-github-changelog-generator-in-zig
  - /2026/building-a-github-changelog-generator-in-zig/
  - /building-a-github-changelog-generator-in-zig
  - /building-a-github-changelog-generator-in-zig/
---

I recently built a GitHub changelog generator in [Zig](https://ziglang.org/). The tool fetches GitHub releases and merged pull requests from any public repository, then automatically generates a Markdown changelog organized by version and category (Features, Bug Fixes, Other).

The source code is available on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

Like my most of my recent projects, GitHub Copilot wrote most of the boilerplate including GitHub workflows, README, install scripts, and snapcraft.yaml. The core functionality took a few evenings to build and test.

## How it works

The changelog generator orchestrates several key components. It parses command-line arguments, resolves a GitHub token (with intelligent fallback), fetches GitHub releases and merged pull requests from the API, groups pull requests by release and category, and formats the result as Markdown.

```zig
pub fn main() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer _ = gpa.deinit();
    const allocator = gpa.allocator();

    const args = try std.process.argsAlloc(allocator);
    defer std.process.argsFree(allocator, args);

    // Parse CLI arguments
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

    // Validate required arguments
    if (parsed_args.repo == null) {
        std.debug.print("Error: --repo is required\n\n", .{});
        cli.CliParser.printHelp();
        return error.MissingRequiredArgs;
    }

    // Resolve GitHub token (optional - can work without token for public repos)
    const resolver = token_resolver.TokenResolver.init(allocator);
    const resolved_token = try resolver.resolve(parsed_args.token);
    defer resolver.deinit(resolved_token);

    // ... fetch and generate changelog
}
```

The main flow is straightforward: validate inputs, resolve credentials, fetch data, generate the changelog structure, format it as Markdown, and write to a file.

## Command Line Interface

The CLI parser handles flag parsing and validation. It supports required arguments like `--repo` and optional ones like `--token`, `--output`, and `--exclude-labels`.

```zig
pub const CliArgs = struct {
    repo: ?[]const u8 = null,
    token: ?[]const u8 = null,
    output: []const u8 = "CHANGELOG.md",
    since_tag: ?[]const u8 = null,
    until_tag: ?[]const u8 = null,
    exclude_labels: ?[]const u8 = null,
};

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
        }
    }

    return result;
}
```

The parser increments through arguments, recognizing flags and their values. Unknown arguments trigger an error with helpful usage information.

## Token Resolution with Fallback Chain

One of the key features is smart GitHub token resolution. The tool attempts to find a token in this order: the `--token` flag, the `GITHUB_TOKEN` environment variable, the `GH_TOKEN` environment variable, or by running the `gh auth token` command.

```zig
pub const ResolvedToken = struct {
    value: []const u8,
    has_token: bool,
    is_owned: bool,
};

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
    std.debug.print("No GitHub token provided or found - proceeding without token\n", .{});
    return ResolvedToken{
        .value = "",
        .has_token = false,
        .is_owned = false,
    };
}
```

The resolver is smart about memory management—it tracks whether the returned token is borrowed (from the caller or command-line) or owned (from environment or subprocess), and only frees what it allocated. This is important in Zig where manual memory management is the default.

## GitHub API Integration

The API client wraps HTTP requests with proper headers and JSON parsing. It fetches GitHub releases (via the Releases API) and merged pull requests from the GitHub API.

```zig
pub const GitHubApiClient = struct {
    allocator: std.mem.Allocator,
    http_client: http_client.HttpClient,
    repo: []const u8,

    pub fn init(allocator: std.mem.Allocator, token: []const u8, repo: []const u8) GitHubApiClient {
        return GitHubApiClient{
            .allocator = allocator,
            .http_client = http_client.HttpClient.init(allocator, token),
            .repo = repo,
        };
    }

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

    pub fn getMergedPullRequests(self: *GitHubApiClient, per_page: u32) ![]models.PullRequest {
        const endpoint = try std.fmt.allocPrint(
            self.allocator,
            "/repos/{s}/pulls?state=closed&per_page={d}&sort=updated&direction=desc",
            .{ self.repo, per_page },
        );
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
            // Copy labels...
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
};
```

The API client handles the quirk of Zig's ownership model: it deep-copies JSON-parsed data into owned allocations since the parser's temporary slice is freed after `deinit()`. It also specifies `.ignore_unknown_fields` to future-proof against API changes.

## Changelog Generation and Grouping

The changelog generator groups pull requests by release and category. It uses a HashMap to collect entries by category for each release, then converts them to arrays.

```zig
pub const ChangelogGenerator = struct {
    allocator: std.mem.Allocator,
    exclude_labels: ?[]const u8 = null,

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

        return Changelog{ .releases = try result.toOwnedSlice(self.allocator), .unreleased = unreleased };
    }
};
```

The generator also creates an "Unreleased Changes" section for pull requests merged after the latest release. Date comparison is done as string slicing (extracting the date portion before the `T`), which works for ISO 8601 formatted timestamps.

## Markdown Formatting

The formatter converts the changelog structure into Markdown strings, then writes to a file. It uses a string concatenation pattern, allocating small parts and combining them.

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
    }

    for (releases) |release| {
        const header = try std.fmt.allocPrint(
            self.allocator,
            "## [{s}](https://github.com/owner/repo/releases/tag/{s}) - {s}\n\n",
            .{ release.version, release.version, release.date },
        );
        try parts.append(self.allocator, header);

        // ... format sections and entries
    }

    // Calculate total length
    var total_len: usize = 0;
    for (parts.items) |part| {
        total_len += part.len;
    }

    // Allocate result and concatenate
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

This approach avoids repeated allocations and reallocations by pre-calculating the total size before allocating once and copying chunks into the final buffer.

## Usage

Using the tool is simple. For a public repository with anonymous access:

```
$ chlogr --repo github/cli --output CHANGELOG.md

GitHub Changelog Generator v0.1.0
Repo: github/cli
Output: CHANGELOG.md
Token: none (anonymous access - may have lower rate limits)
  To get higher rate limits, provide a token via --token flag, GITHUB_TOKEN env var, GH_TOKEN env var, or gh CLI

Fetching data from GitHub...
Found 142 releases and 523 pull requests
Changelog written to CHANGELOG.md
```

With a token for higher rate limits:

```
$ chlogr --repo github/cli --token ghp_xxxxxxxx --output HISTORY.md

GitHub Changelog Generator v0.1.0
Repo: github/cli
Output: HISTORY.md
Using token from environment variable
Token: ******* (truncated)

Fetching data from GitHub...
Found 142 releases and 523 pull requests
Changelog written to HISTORY.md
```

To exclude certain labels from the changelog:

```
chlogr --repo github/cli --exclude-labels "duplicate,wontfix" --output CHANGELOG.md
```

Note: The flags `--since-tag` and `--until-tag` are currently parsed but not yet wired into the changelog generator. They are reserved for future date-range filtering functionality.

The generated Markdown looks like:

```markdown
# Changelog

## [Unreleased Changes]

### Features

- Add new experimental feature (#456) (@alice)

### Bug Fixes

- Fix critical crash in parser (#457) (@bob)

## [v1.2.0](https://github.com/github/cli/releases/tag/v1.2.0) - 2024-01-15

### Features

- Add authentication command (#440) (@charlie)
- Support for new API endpoints (#441) (@alice)

### Bug Fixes

- Fix URL encoding issue (#442) (@bob)

### Merged Pull Requests

- Update documentation (#443) (@charlie)

## [v1.1.0](https://github.com/github/cli/releases/tag/v1.1.0) - 2024-01-10

...
```

## Building and Testing

Building the tool is straightforward with Zig's build system:

```bash
zig build
```

This compiles the binary to `zig-out/bin/chlogr`. The build file also includes an integration test target:

```bash
zig build test
```

The integration tests use mock GitHub API responses to verify changelog grouping, categorization, and Markdown formatting without making real API calls.

The `build.zig` configuration is minimal:

```zig
pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const exe = b.addExecutable(.{
        .name = "chlogr",
        .root_module = b.createModule(.{
            .root_source_file = b.path("src/main.zig"),
            .target = target,
            .optimize = optimize,
        }),
    });

    b.installArtifact(exe);

    const run_cmd = b.addRunArtifact(exe);
    if (b.args) |args| {
        run_cmd.addArgs(args);
    }

    const run_step = b.step("run", "Run the app");
    run_step.dependOn(&run_cmd.step);

    // Integration test...
}
```

## Distribution

Like my previous Zig projects, the installation is kept simple. The `install.sh` script downloads the latest release binary for Linux or macOS from GitHub Releases:

```bash
#!/usr/bin/env bash
set -euo pipefail

REPO="christianhelle/chlogr"
INSTALL_DIR="${INSTALL_DIR:-$HOME/.local/bin}"
tmp_dir=""

cleanup() {
  if [ -n "${tmp_dir:-}" ] && [ -d "$tmp_dir" ]; then
    rm -rf -- "$tmp_dir"
  fi
}

detect_platform() {
  local os arch
  os="$(uname -s)"
  arch="$(uname -m)"

  case "$os" in
  Linux) os="linux" ;;
  Darwin) os="macos" ;;
  *)
    echo "Unsupported OS: $os" >&2
    exit 1
    ;;
  esac

  case "$arch" in
  x86_64 | amd64) arch="x86_64" ;;
  aarch64 | arm64) arch="aarch64" ;;
  *)
    echo "Unsupported architecture: $arch" >&2
    exit 1
    ;;
  esac

  echo "${os}-${arch}"
}

main() {
  local platform artifact_name url

  platform="$(detect_platform)"
  artifact_name="chlogr-${platform}.tar.gz"

  echo "Detecting platform: ${platform}"

  url="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" |
    grep -o "\"browser_download_url\": *\"[^\"]*${artifact_name}\"" |
    head -1 |
    cut -d'"' -f4)"

  if [ -z "$url" ]; then
    echo "Error: could not find release asset ${artifact_name}" >&2
    exit 1
  fi

  tmp_dir="$(mktemp -d)"
  trap cleanup EXIT

  echo "Downloading ${url}..."
  curl -fsSL "$url" -o "${tmp_dir}/${artifact_name}"

  echo "Installing to ${INSTALL_DIR}..."
  tar xzf "${tmp_dir}/${artifact_name}" -C "$tmp_dir"
  install -d "$INSTALL_DIR"
  install -m 755 "${tmp_dir}/chlogr" "$INSTALL_DIR/chlogr"

  echo "chlogr installed to ${INSTALL_DIR}/chlogr"
}

main
```

For Windows, `install.ps1` downloads and unzips the Windows x86_64 binary:

```powershell
$ErrorActionPreference = 'Stop'

$repo = "christianhelle/chlogr"
$artifact = "chlogr-windows-x86_64.zip"
$installDir = "$env:USERPROFILE\.local\bin"

Write-Host "Fetching latest release..."
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/releases/latest"
$asset = $release.assets | Where-Object { $_.name -eq $artifact }

if (-not $asset) {
    Write-Error "Could not find release asset: $artifact"
    exit 1
}

$url = $asset.browser_download_url
$tmpFile = Join-Path $env:TEMP $artifact

Write-Host "Downloading $url..."
Invoke-WebRequest -Uri $url -OutFile $tmpFile

Write-Host "Installing to $installDir..."
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Expand-Archive -Path $tmpFile -DestinationPath $installDir -Force
Remove-Item $tmpFile -Force

# Add to user PATH if not already present
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$installDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$installDir", "User")
    Write-Host "Added $installDir to user PATH (restart your terminal to use)"
}

Write-Host "chlogr installed to $installDir\chlogr.exe"
```

GitHub Actions builds tar.gz archives for Linux (x86_64 and aarch64) and macOS (x86_64 and aarch64), and a zip archive for Windows (x86_64). These are automatically attached to releases via the workflow.

## Known Limitations and Future Work

The current implementation has several limitations worth noting:

1. **GitHub Releases Required**: The tool uses the GitHub Releases API, not raw Git tags. A repository with only tags but no releases will not generate a changelog. Releases must be explicitly created.

2. **Fixed PR Pagination**: The tool fetches a fixed number of pull requests (100 per page). Large repositories with thousands of pull requests may have incomplete results unless pagination is enhanced.

3. **Placeholder Release Links**: Generated Markdown release headers use hard-coded placeholder links (`https://github.com/owner/repo/releases/tag/{tag}`). The actual repository owner and name are not yet wired into the formatter.

4. **Date Range Filtering Not Implemented**: The `--since-tag` and `--until-tag` flags are parsed but not yet connected to the changelog generator logic. They are reserved for future implementation.

5. **Label Filtering by Substring**: The `--exclude-labels` option uses simple substring matching rather than proper CSV parsing. For example, "wontfix" would also match a label named "wontfixme".

6. **Per-Release Grouping Overlap**: Pull requests are grouped by checking whether they merged before a given release date. There is no lower bound per release, so older pull requests can theoretically appear in multiple release sections if they are manually re-categorized.

7. **No Caching**: Each run makes fresh API calls. For frequent changelog generation, caching would improve performance.

8. **Basic HTTP Client**: The HTTP client is functional but minimal. Testing uses mock data rather than real API calls.

## Conclusion

Building chlogr was a good exercise in working with Zig's standard library for HTTP, JSON parsing, and string allocation. The tool generates a useful artifact (changelogs) while demonstrating real-world concerns like credential handling, API integration, and Markdown formatting.

The source code is on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr). If you need to automatically generate changelogs from GitHub, give it a try. Contributions and improvements are welcome!
