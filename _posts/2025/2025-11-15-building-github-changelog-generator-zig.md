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

I recently built a command-line tool for automatically generating changelogs from GitHub releases and pull requests. Called **changelog-generator** at the time (later renamed to [chlogr](https://github.com/christianhelle/chlogr)), the tool queries the GitHub API, categorizes merged PRs by their labels, and generates a nicely formatted Markdown changelog. It is written in [Zig](https://ziglang.org/) with zero Zig package dependencies, though it shells out to `curl` at runtime for HTTP requests.

The source code is available on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

Maintaining accurate changelogs is tedious. Most projects either manually edit CHANGELOG.md, which becomes outdated, or ignore changelogs entirely. GitHub's releases and PR labels already contain structured information about what changed—why not use that to generate changelogs automatically? That was the motivation for this project. As with my previous Zig projects, GitHub Copilot assisted with scaffolding the GitHub workflows, README, and installation scripts. The core logic took a few focused evenings to build.

## How it works

The tool follows a straightforward pipeline: parse CLI arguments, resolve a GitHub token, fetch data from the GitHub API, generate a structured changelog, format it as Markdown, and write it to a file. Each step is handled by a dedicated module:

```
main.zig → cli.zig → token_resolver.zig → http_client.zig → github_api.zig → changelog_generator.zig → markdown_formatter.zig
```

The `main.zig` entry point wires everything together:

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

    // Resolve GitHub token
    const resolver = token_resolver.TokenResolver.init(allocator);
    const resolved_token = resolver.resolve(parsed_args.token) catch |err| {
        std.debug.print("Error: Could not retrieve GitHub token\n", .{});
        return err;
    };
    defer resolver.deinit(resolved_token);

    // Initialize GitHub API client
    var api_client = github_api.GitHubApiClient.init(
        allocator, resolved_token.value, parsed_args.owner.?, parsed_args.repo.?,
    );
    defer api_client.deinit();

    // Fetch releases and PRs
    const releases = try api_client.getReleases();
    defer api_client.freeReleases(releases);

    const prs = try api_client.getMergedPullRequests(100);
    defer api_client.freePullRequests(prs);

    // Generate changelog
    var gen = changelog_generator.ChangelogGenerator.init(allocator, parsed_args.exclude_labels);
    const changelog = try gen.generate(releases, prs);
    defer gen.deinit(changelog);

    // Format to Markdown and write to file
    var formatter = markdown_formatter.MarkdownFormatter.init(allocator);
    const markdown = try formatter.format(changelog);
    defer formatter.deinit(markdown);

    try formatter.writeToFile(parsed_args.output, markdown);
}
```

Each module has a clear responsibility and manages its own allocations. The `defer` statements ensure cleanup in reverse order, which is a pattern you will see throughout the codebase. A GitHub token is required—the tool errors out if no token can be resolved through any of the fallback methods.
## CLI Design and Argument Parsing

The CLI accepts required `--owner` and `--repo` arguments and optional flags for output path, GitHub token, tag filtering, and label exclusion.

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
            } else if (std.mem.eql(u8, arg, "--since-tag")) {
                i += 1;
                if (i >= args.len) return error.MissingSinceTagValue;
                result.since_tag = args[i];
            } else if (std.mem.eql(u8, arg, "--until-tag")) {
                i += 1;
                if (i >= args.len) return error.MissingUntilTagValue;
                result.until_tag = args[i];
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

The parser is a simple linear scan through arguments. Each flag consumes the next value, with validation that required values are present. Both `--owner` and `--repo` are required—this was the original interface before the project later consolidated them into a single `--repo owner/repo` flag. The `--since-tag` and `--until-tag` flags are defined in the CLI struct for future use but are not yet wired into the changelog generation logic. Validation of required fields happens in `main.zig` before the API calls begin.
## Smart GitHub Token Resolution

A key usability feature is automatic token resolution. The tool needs a GitHub token to access the API, but requiring users to manually pass a token on every invocation would be tedious. Instead, the tool implements a fallback chain:

```zig
pub const ResolvedToken = struct {
    value: []const u8,
    owned: bool,
};

pub const TokenResolver = struct {
    allocator: std.mem.Allocator,

    pub fn resolve(self: TokenResolver, provided_token: ?[]const u8) !ResolvedToken {
        // 1. Check provided token (not owned - don't free)
        if (provided_token) |token| {
            return ResolvedToken{
                .value = token,
                .owned = false,
            };
        }

        // 2. Check GITHUB_TOKEN env var (owned - must free)
        if (std.process.getEnvVarOwned(self.allocator, "GITHUB_TOKEN")) |token| {
            return ResolvedToken{
                .value = token,
                .owned = true,
            };
        } else |_| {}

        // 3. Check GH_TOKEN env var (owned - must free)
        if (std.process.getEnvVarOwned(self.allocator, "GH_TOKEN")) |token| {
            return ResolvedToken{
                .value = token,
                .owned = true,
            };
        } else |_| {}

        // 4. Try to get token from gh CLI (owned - must free)
        if (self.getTokenFromGhCli()) |token| {
            return ResolvedToken{
                .value = token,
                .owned = true,
            };
        } else |_| {}

        return error.NoTokenAvailable;
    }

    pub fn deinit(self: TokenResolver, resolved_token: ResolvedToken) void {
        if (resolved_token.owned) {
            self.allocator.free(resolved_token.value);
        }
    }
};
```

The resolver follows this priority:
1. Explicit `--token` flag (not owned by the resolver—don't free)
2. `GITHUB_TOKEN` environment variable
3. `GH_TOKEN` environment variable
4. Token from `gh auth token` command (if GitHub CLI is installed)
5. If no token is found, the tool exits with an error

The `ResolvedToken` struct tracks ownership with a simple `owned` boolean. Tokens from environment variables and the `gh` command are heap-allocated via `getEnvVarOwned` and must be freed, while the `--token` flag value points to memory owned by the argument parser. This distinction prevents both use-after-free and double-free bugs.
## HTTP Client with curl

Rather than pulling in a Zig HTTP library or using the standard library's HTTP client directly, changelog-generator spawns `curl` as a child process for HTTP requests. This is a pragmatic trade-off: it keeps the Zig code simple and leverages a ubiquitous system utility, at the cost of a runtime dependency on `curl`.

```zig
pub const HttpClient = struct {
    allocator: std.mem.Allocator,
    token: []const u8,
    base_url: []const u8 = "https://api.github.com",

    pub fn get(self: *HttpClient, endpoint: []const u8) ![]u8 {
        const url = try std.fmt.allocPrint(
            self.allocator, "{s}{s}", .{ self.base_url, endpoint },
        );
        defer self.allocator.free(url);

        const auth_header = try std.fmt.allocPrint(
            self.allocator, "Authorization: token {s}", .{self.token},
        );
        defer self.allocator.free(auth_header);

        const user_agent = "User-Agent: changelog-generator/0.1.0";

        const args = [_][]const u8{
            "curl",
            "-s",
            "-H", auth_header,
            "-H", user_agent,
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
        if (term.Exited != 0) {
            return error.CurlFailed;
        }

        if (bytes_read == 0) {
            return error.EmptyResponse;
        }

        return try self.allocator.dupe(u8, buffer[0..bytes_read]);
    }
};
```

The function builds a curl command with authentication and GitHub API headers, spawns it as a child process, and reads the response from stdout into a 10 MB stack buffer. The response bytes are then copied to a heap-allocated slice so the caller owns the memory. The curl exit code is checked—a non-zero exit means the request failed, and an empty response is treated as an error.

While `curl` is nearly universal on Unix-like systems and available on Windows through various package managers, this does mean the tool is not truly zero-dependency at runtime. The Zig code itself has no package dependencies, but `curl` must be installed and available in the system PATH. This was later addressed when the project migrated to Zig's native `std.http.Client`.
## GitHub API Integration

The API client uses the HTTP client to fetch GitHub data. It constructs endpoints for releases and merged pull requests, parses the JSON responses, and deep-copies the parsed data to ensure ownership.

```zig
pub fn getReleases(self: *GitHubApiClient) ![]models.Release {
    const endpoint = try std.fmt.allocPrint(
        self.allocator, "/repos/{s}/{s}/releases", .{ self.owner, self.repo },
    );
    defer self.allocator.free(endpoint);

    const response = try self.http_client.get(endpoint);
    defer self.allocator.free(response);

    var parsed = try std.json.parseFromSlice(
        []models.Release,
        self.allocator,
        response,
        .{ .ignore_unknown_fields = true },
    );
    defer parsed.deinit();

    // Deep copy releases with string duplication
    var releases = try std.ArrayList(models.Release).initCapacity(
        self.allocator, parsed.value.len,
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

The `ignore_unknown_fields` option is important—GitHub's API returns many more fields than we need, and this lets us parse only the fields defined in our model structs without erroring on the rest.

The deep-copy-then-deinit pattern deserves special attention. When you call `std.json.parseFromSlice`, the parsed values contain string slices that point into the JSON source buffer. If you `deinit()` the parsed result (which frees the JSON arena), those string slices become dangling pointers. By duplicating every string field with `allocator.dupe(u8, ...)` before calling `parsed.deinit()`, we ensure our data is fully independent and safely owned.

The same pattern applies to pull request fetching, where we also deep-copy nested structs like labels and user information:

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
        self.allocator, parsed.value.len,
    );
    for (parsed.value) |pr| {
        var labels = try std.ArrayList(models.Label).initCapacity(
            self.allocator, pr.labels.len,
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
            .body = if (pr.body) |body| try self.allocator.dupe(u8, body) else null,
            .html_url = try self.allocator.dupe(u8, pr.html_url),
            .user = .{
                .login = try self.allocator.dupe(u8, pr.user.login),
                .html_url = try self.allocator.dupe(u8, pr.user.html_url),
            },
            .labels = try labels.toOwnedSlice(self.allocator),
            .merged_at = if (pr.merged_at) |merged|
                try self.allocator.dupe(u8, merged)
            else
                null,
        });
    }
    return try prs.toOwnedSlice(self.allocator);
}
```

Note the query parameter `state=closed` to fetch closed PRs (which includes merged ones) and `sort=updated&direction=desc` to get the most recently updated first. The models are intentionally minimal—just the fields we need for changelog generation:

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
## Changelog Generation and Categorization

The changelog generator takes releases and pull requests, then groups PRs under each release and categorizes them by label.

```zig
pub fn generate(
    self: ChangelogGenerator,
    releases: []models.Release,
    prs: []models.PullRequest,
) ![]ChangelogRelease {
    var result = try std.ArrayList(ChangelogRelease).initCapacity(
        self.allocator, releases.len,
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

            const category = self.categorizeEntry(pr.labels);

            var section_list = sections_map.getOrPut(category) catch continue;
            if (!section_list.found_existing) {
                const arr = try std.ArrayList(ChangelogEntry).initCapacity(
                    self.allocator, prs.len,
                );
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

        var sections_array = try std.ArrayList(ChangelogSection).initCapacity(
            self.allocator, sections_map.count(),
        );

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

    return result.toOwnedSlice(self.allocator);
}
```

The algorithm iterates each release, then iterates all fetched PRs, categorizing them by label into a hash map of sections. In this initial version, all fetched PRs are associated with every release—there is no date-based filtering to assign PRs only to the release window they belong to. This is a simplification that works well enough when the API response is limited and sorted by most recently updated, but it means each release section will contain the same set of PRs. Date-based assignment was added in a later version.

The categorization logic is straightforward:

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
    return "Other";
}
```

Anything labeled "feature" or "enhancement" goes into **Features**. Anything labeled "bug" or "bugfix" goes into **Bug Fixes**. Everything else falls into an **Other** category. The `shouldExclude` function also checks if any of a PR's labels match the `--exclude-labels` filter, allowing users to suppress noise from things like dependency updates.
## Markdown Formatting

The formatter converts the structured changelog into a readable Markdown document.

```zig
pub fn format(
    self: MarkdownFormatter,
    releases: []changelog_generator.ChangelogRelease,
) ![]u8 {
    var parts = try std.ArrayList([]u8).initCapacity(
        self.allocator, releases.len * 20,
    );
    defer parts.deinit(self.allocator);

    try parts.append(
        self.allocator, try self.allocator.dupe(u8, "# Changelog\n\n"),
    );

    for (releases) |release| {
        const header = try std.fmt.allocPrint(
            self.allocator,
            "## [{s}](https://github.com/owner/repo/releases/tag/{s}) - {s}\n\n",
            .{ release.version, release.version, release.date },
        );
        try parts.append(self.allocator, header);

        for (release.sections) |section| {
            const section_header = try std.fmt.allocPrint(
                self.allocator, "### {s}\n", .{section.name},
            );
            try parts.append(self.allocator, section_header);

            for (section.entries) |entry| {
                const entry_line = try std.fmt.allocPrint(
                    self.allocator,
                    "- {s} ([#{d}]({s})) (@{s})\n",
                    .{ entry.title, entry.number, entry.url, entry.author },
                );
                try parts.append(self.allocator, entry_line);
            }

            try parts.append(
                self.allocator, try self.allocator.dupe(u8, "\n"),
            );
        }

        try parts.append(
            self.allocator, try self.allocator.dupe(u8, "\n"),
        );
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

Rather than building a single large buffer or using a string builder, the formatter collects all formatted lines in an `ArrayList` of heap-allocated strings, then concatenates them in a single pass at the end. This two-phase approach avoids repeated reallocations from a growing buffer.

Each entry is formatted as a bullet point with the PR title, number (as a link), and author handle:
```
- Add user authentication ([#123](https://github.com/user/repo/pull/123)) (@alice)
```

One thing worth noting: in this version, the release header links use a hardcoded `https://github.com/owner/repo/releases/tag/{s}` placeholder rather than constructing the URL from the actual owner and repo values passed to the tool. This means the release heading links in the generated Markdown point to a non-existent `owner/repo` path. The individual PR links are correct since they come directly from the GitHub API response's `html_url` field. The formatter was updated in a later version to accept the repository context and emit correct release links.
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

Testing uses a separate test executable that exercises the pipeline with mock GitHub JSON data:

```bash
zig build test
```

The test module (`src/test.zig`) provides hardcoded JSON strings for releases and pull requests, allowing the changelog generator to be tested end-to-end without hitting the real GitHub API. It parses the mock data through `std.json.parseFromSlice`, runs the changelog generator, formats the output to Markdown, and writes a test file. Key test scenarios include:

- JSON parsing of releases and PRs with various label combinations
- Categorization logic (feature → Features, bug → Bug Fixes, unlabeled → Other)
- Markdown formatting with proper links and PR references
- Label exclusion filters
- End-to-end pipeline from mock JSON to formatted Markdown output

The test executable is defined in `build.zig` alongside the main executable, using the same target and optimization settings. This pattern—a standalone test binary with embedded test data—avoids the need for network access or a valid GitHub token during testing.
## Usage

Using changelog-generator is straightforward:

```bash
./zig-out/bin/changelog-generator --owner github --repo cli --output CHANGELOG.md
```

If you have a `GITHUB_TOKEN` environment variable set (common in CI/CD), the tool uses that automatically. Otherwise, it attempts to use the `gh` CLI's token. A valid token is required—the tool will error out if none is found.

The generated CHANGELOG.md looks something like this:

```markdown
# Changelog

## [v2.5.0](https://github.com/owner/repo/releases/tag/v2.5.0) - 2025-10-15T14:30:00Z

### Features

- Add API v4 support ([#400](https://github.com/github/cli/pull/400)) (@alice)

### Bug Fixes

- Fix incorrect header parsing ([#401](https://github.com/github/cli/pull/401)) (@bob)

### Other

- Update documentation ([#402](https://github.com/github/cli/pull/402)) (@charlie)
```

Note that the release heading links use the hardcoded `owner/repo` placeholder in this version, so they do not point to the actual repository. The individual PR links are correct since they come directly from the GitHub API response.
## Distribution

Like my previous Zig projects, I wanted simple cross-platform distribution. GitHub Copilot generated the installation scripts and the release workflow.

The `install.sh` script detects the platform and downloads the latest release from GitHub:

```bash
#!/usr/bin/env bash
set -euo pipefail

REPO="christianhelle/changelog-generator"
INSTALL_DIR="${INSTALL_DIR:-$HOME/.local/bin}"

detect_platform() {
  local os arch
  os="$(uname -s)"
  arch="$(uname -m)"

  case "$os" in
  Linux) os="linux" ;;
  Darwin) os="macos" ;;
  *) echo "Unsupported OS: $os" >&2; exit 1 ;;
  esac

  case "$arch" in
  x86_64 | amd64) arch="x86_64" ;;
  aarch64 | arm64) arch="aarch64" ;;
  *) echo "Unsupported architecture: $arch" >&2; exit 1 ;;
  esac

  echo "${os}-${arch}"
}

main() {
  local platform artifact_name url
  platform="$(detect_platform)"
  artifact_name="changelog-generator-${platform}.tar.gz"

  url="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" |
    grep -o "\"browser_download_url\": *\"[^\"]*${artifact_name}\"" |
    head -1 | cut -d'"' -f4)"

  curl -fsSL "$url" -o "/tmp/${artifact_name}"
  tar xzf "/tmp/${artifact_name}" -C /tmp
  install -d "$INSTALL_DIR"
  install -m 755 /tmp/changelog-generator "$INSTALL_DIR/changelog-generator"
}

main
```

For Windows, `install.ps1` fetches the latest release and adds the install directory to the user's PATH:

```powershell
$ErrorActionPreference = 'Stop'

$repo = "christianhelle/changelog-generator"
$artifact = "changelog-generator-windows-x86_64.zip"
$installDir = "$env:USERPROFILE\.local\bin"

$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/releases/latest"
$asset = $release.assets | Where-Object { $_.name -eq $artifact }
$url = $asset.browser_download_url

Invoke-WebRequest -Uri $url -OutFile (Join-Path $env:TEMP $artifact)
Expand-Archive -Path (Join-Path $env:TEMP $artifact) -DestinationPath $installDir -Force

$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$installDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$installDir", "User")
}
```

The GitHub Actions release workflow (`release.yml`) cross-compiles for five platform targets in a single pipeline:

```yaml
jobs:
  build:
    strategy:
      matrix:
        include:
          - target: x86_64-linux
            os: ubuntu-latest
            artifact: changelog-generator-linux-x86_64
          - target: aarch64-linux
            os: ubuntu-latest
            artifact: changelog-generator-linux-aarch64
          - target: x86_64-macos
            os: macos-latest
            artifact: changelog-generator-macos-x86_64
          - target: aarch64-macos
            os: macos-latest
            artifact: changelog-generator-macos-aarch64
          - target: x86_64-windows
            os: windows-latest
            artifact: changelog-generator-windows-x86_64
```

This is one of Zig's strengths for CLI tools—cross-compilation is a first-class feature of the build system. A single `zig build -Doptimize=ReleaseFast -Dtarget=aarch64-linux` produces a statically-linked binary for ARM Linux, even when building on an x86 host. The release workflow packages each binary as a tarball (Unix) or zip (Windows) and attaches them to the GitHub Release.

## Conclusion

Building changelog-generator was a satisfying exercise in practical Zig development. The project combines several real-world patterns—CLI argument parsing, environment-aware token resolution, REST API integration via process spawning, JSON parsing with the deep-copy-then-deinit pattern, label-based categorization, and Markdown generation. Each component is straightforward, but together they form a tool that automates a genuinely tedious task.

A few design decisions shaped the project. Using `curl` for HTTP was a pragmatic choice—it kept the Zig code focused on the business logic rather than HTTP protocol details, at the cost of a runtime dependency. Label-driven categorization assumes your PRs are labeled, which is reasonable for teams already using GitHub's workflow. Mock-based testing with hardcoded JSON made the test suite fast and deterministic without requiring a valid token or network access. And the deep-copy-after-parse pattern, while adding a small cost, prevents an entire category of use-after-free bugs that would be hard to track down.

Working with Zig's explicit memory management was again a highlight. The `ResolvedToken` ownership tracking, the consistent use of `defer` for cleanup, and the careful allocation of every string field during JSON parsing all forced me to think about allocation lifetimes in a way that garbage-collected languages abstract away. It is more work upfront, but the result is code where every allocation is accounted for.

The tool has some intentional simplifications in this initial version—PRs are not filtered by merge date relative to each release, the Markdown formatter uses a hardcoded release link placeholder, and pagination is not implemented for repositories with large numbers of PRs. These were addressed in later versions as the project evolved. It was eventually renamed to [chlogr](https://github.com/christianhelle/chlogr) and gained features like Zig's native `std.http.Client` for HTTP, unreleased changes tracking, and the combined `--repo owner/repo` flag.

If you maintain a project with git tags and labeled pull requests, give it a try. The source code is on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).
