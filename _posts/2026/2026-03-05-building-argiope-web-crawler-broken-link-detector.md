---
layout: post
title: Building a web crawler and broken link detector in Zig
date: 2026-03-05
author: Christian Helle
tags:
  - Zig
  - CLI
redirect_from:
  - /2026/03/05/building-argiope-web-crawler-broken-link-detector
  - /2026/03/05/building-argiope-web-crawler-broken-link-detector/
  - /2026/03/building-argiope-web-crawler-broken-link-detector
  - /2026/03/building-argiope-web-crawler-broken-link-detector/
  - /2026/building-argiope-web-crawler-broken-link-detector
  - /2026/building-argiope-web-crawler-broken-link-detector/
  - /building-argiope-web-crawler-broken-link-detector
  - /building-argiope-web-crawler-broken-link-detector/
---

I recently built a web crawler for broken link detection and image downloading in [Zig](https://ziglang.org/). The tool can crawl websites, detect broken links, generate reports in multiple formats, and download images from web pages. I named it [Argiope](https://github.com/christianhelle/argiope) after the genus of orb-weaving spiders, which seemed fitting for a web crawler.

The source code is available on GitHub at [https://github.com/christianhelle/argiope](https://github.com/christianhelle/argiope).

Like my previous Zig project, GitHub Copilot wrote most of the boilerplate, including the GitHub workflows, README, install scripts, and snapcraft.yaml file. The entire project took a few evenings to build.

## How it works

The crawler uses a [Breadth-first search (BFS)](https://en.wikipedia.org/wiki/Breadth-first_search) approach to traverse web pages. It starts with a seed URL, fetches the page, extracts all links, and adds them to a queue for processing. Each URL is normalized and checked against a visited set to avoid processing the same URL twice.

```zig
pub const Crawler = struct {
    allocator: std.mem.Allocator,
    base_url: []u8,
    base_host: []u8,
    queue: std.ArrayListUnmanaged(QueueEntry) = .empty,
    visited: std.StringHashMapUnmanaged(void) = .empty,
    results: std.ArrayListUnmanaged(CrawlResult) = .empty,
    options: CrawlOptions,

    pub fn init(allocator: std.mem.Allocator, url: []const u8, options: CrawlOptions) Crawler {
        const base_url = allocator.dupe(u8, url) catch "";
        const base_host = url_mod.extractHost(base_url) orelse "";
        return .{
            .allocator = allocator,
            .base_url = base_url,
            .base_host = base_host,
            .options = options,
        };
    }

    pub fn crawl(self: *Crawler) !void {
        try self.queue.append(self.allocator, .{
            .url = try self.allocator.dupe(u8, self.base_url),
            .depth = 0,
        });

        if (self.options.parallel) {
            try self.crawlParallel();
        } else {
            try self.crawlSequential();
        }
    }
};
```

The tool supports both sequential and parallel crawling. In parallel mode, a thread pool processes multiple URLs concurrently, which significantly speeds up crawling for sites with many links.

Domain restriction is enforced by extracting the host from each URL and comparing it to the base URL's host. External links are still checked for broken status but not followed for further crawling. This keeps the crawler focused on the target site while still validating outbound links.

```zig
pub fn isInternal(self: *const Crawler, url: []const u8) bool {
    const host = url_mod.extractHost(url) orelse return false;
    return std.mem.eql(u8, host, self.base_host);
}
```

## HTML Parsing

Rather than pulling in a full HTML parser dependency, I wrote a lightweight scanner that extracts links and image sources. It iterates through the HTML looking for opening tags and extracts `href` attributes from anchor tags and `src` attributes from image tags.

```zig
pub fn extractLinks(allocator: std.mem.Allocator, html: []const u8) ![]Link {
    var links: std.ArrayListUnmanaged(Link) = .empty;

    var pos: usize = 0;
    while (pos < html.len) {
        const tag_start = std.mem.indexOfPos(u8, html, pos, "<") orelse break;
        pos = tag_start + 1;
        if (pos >= html.len) break;

        // Skip comments
        if (pos + 2 < html.len and html[pos] == '!' and
            html[pos + 1] == '-' and html[pos + 2] == '-') {
            const comment_end = std.mem.indexOfPos(u8, html, pos, "-->") orelse break;
            pos = comment_end + 3;
            continue;
        }

        // Read tag name and extract attributes...
        const tag_name = html[tag_name_start..pos];

        // Determine what attribute we're looking for
        const attr_name: ?[]const u8 = blk: {
            if (asciiEqlIgnoreCase(tag_name, "a") or
                asciiEqlIgnoreCase(tag_name, "link") or
                asciiEqlIgnoreCase(tag_name, "area"))
            {
                break :blk "href";
            }
            if (asciiEqlIgnoreCase(tag_name, "img") or
                asciiEqlIgnoreCase(tag_name, "script") or
                asciiEqlIgnoreCase(tag_name, "source"))
            {
                break :blk "src";
            }
            break :blk null;
        };

        // Extract and store the attribute value...
    }

    return links.toOwnedSlice(allocator);
}
```

The scanner also handles `srcset` attributes on image tags, parsing the comma-separated list of image URLs. It skips JavaScript, mailto, tel, data URLs, and fragment-only links.

## URL Normalization

URL handling is surprisingly complex. Relative URLs need to be resolved against the base URL, query parameters may need to be normalized, and trailing slashes should be handled consistently.

```zig
pub fn resolve(allocator: std.mem.Allocator, base: []const u8, href: []const u8) ![]u8 {
    // Absolute URL
    if (std.mem.indexOf(u8, href, "://") != null) {
        return allocator.dupe(u8, href);
    }

    // Protocol-relative URL
    if (std.mem.startsWith(u8, href, "//")) {
        const proto = extractProtocol(base) orelse "https";
        return std.fmt.allocPrint(allocator, "{s}:{s}", .{ proto, href });
    }

    // Absolute path
    if (href.len > 0 and href[0] == '/') {
        const origin = try extractOrigin(allocator, base);
        defer allocator.free(origin);
        return std.fmt.allocPrint(allocator, "{s}{s}", .{ origin, href });
    }

    // Relative path
    const base_dir = extractDirectory(base);
    return std.fmt.allocPrint(allocator, "{s}/{s}", .{ base_dir, href });
}
```

The `normalize` function ensures URLs are in a consistent form by converting to lowercase (for the scheme and host), removing default ports, and collapsing path segments.

## HTTP Client

The tool uses Zig's standard library HTTP client with custom timeout and redirect handling. Each request is wrapped with a timeout to avoid hanging on unresponsive servers.

```zig
pub const FetchOptions = struct {
    max_redirects: u8 = 5,
    timeout_ms: u32 = 10_000,
    max_body_size: usize = 10 * 1024 * 1024,
};

pub fn fetch(
    client: *std.http.Client,
    allocator: std.mem.Allocator,
    url: []const u8,
    options: FetchOptions,
) !FetchResult {
    const uri = try std.Uri.parse(url);

    var req = try client.open(.GET, uri, .{
        .server_header_buffer = try allocator.alloc(u8, 16384),
    });
    defer req.deinit();

    req.send() catch |err| {
        return FetchResult{
            .status = 0,
            .body = null,
            .error_msg = try allocator.dupe(u8, @errorName(err)),
        };
    };

    try req.wait();

    const status = @intFromEnum(req.response.status);

    // Handle redirects
    if (status >= 300 and status < 400 and options.max_redirects > 0) {
        const location = req.response.headers.getFirstValue("location") orelse {
            return error.InvalidRedirect;
        };
        // Follow redirect...
    }

    // Read response body...
    const body = try req.reader().readAllAlloc(allocator, options.max_body_size);

    return FetchResult{
        .status = @intCast(status),
        .body = body,
        .error_msg = null,
    };
}
```

The client handles HTTP redirects up to a configurable limit and collects both the status code and response body. Errors during the request are captured and returned as part of the result rather than propagated, allowing the crawler to continue processing other URLs.

## Command Line Interface

The CLI supports two main commands: `check` for broken link detection and `images` for downloading images. Options include crawl depth, timeout, request delay, and output format.

```zig
pub const Command = enum {
    check,
    images,
    help,
    version_cmd,
};

pub const Options = struct {
    command: Command = .help,
    url: ?[]const u8 = null,
    depth: u16 = 3,
    timeout_ms: u32 = 10_000,
    delay_ms: u32 = 100,
    output_dir: []const u8 = "./download",
    verbose: bool = false,
    parallel: bool = false,
    report: ?[]const u8 = null,
    report_format: ReportFormat = .text,
    include_positives: bool = false,
};

pub fn parseArgs(args: []const []const u8) !Options {
    var opts = Options{};

    if (args.len < 2) return opts;

    var i: usize = 1;
    while (i < args.len) : (i += 1) {
        const arg = args[i];

        if (std.mem.eql(u8, arg, "check")) {
            opts.command = .check;
        } else if (std.mem.eql(u8, arg, "images")) {
            opts.command = .images;
        } else if (std.mem.startsWith(u8, arg, "--depth")) {
            // Parse depth value...
        } else if (std.mem.startsWith(u8, arg, "--timeout")) {
            // Parse timeout value...
        } else if (!std.mem.startsWith(u8, arg, "-")) {
            opts.url = arg;
        }
    }

    return opts;
}
```

The parser iterates through command-line arguments, identifying commands, flags, and values. It handles both short (`-v`) and long (`--verbose`) flag formats.

## Report Generation

The tool generates reports in three formats: plain text, Markdown, and HTML. Reports can include just broken links or all checked URLs depending on the `--include-positives` flag.

```zig
pub fn write(
    allocator: std.mem.Allocator,
    path: []const u8,
    format: cli_mod.ReportFormat,
    url: []const u8,
    results: []const crawler_mod.CrawlResult,
    summary: summary_mod.CheckSummary,
    include_positives: bool,
) !void {
    const file = try std.fs.cwd().createFile(path, .{ .truncate = true });
    defer file.close();

    var buf: [65536]u8 = undefined;
    var fw = file.writer(&buf);
    const w = &fw.interface;

    switch (format) {
        .text => try writeText(w, url, results, summary, include_positives),
        .markdown => try writeMarkdown(w, url, results, summary, include_positives),
        .html => try writeHtml(allocator, w, url, results, summary, include_positives),
    }

    try w.flush();
}
```

The HTML report is self-contained with inline CSS and uses a card-based layout with color-coded status badges. This makes it suitable for embedding in CI/CD pipelines or sharing as a standalone file.

## Usage

The basic usage is straightforward. Run `argiope check <url>` to scan a website for broken links:

```
$ argiope check https://christianhelle.com --depth 3

Crawling https://christianhelle.com (depth=3, timeout=10s)...

------------------------------------------------------------------------
Status   Type       Time(ms)   URL
------------------------------------------------------------------------
404      internal   45         https://christianhelle.com/missing-page
------------------------------------------------------------------------

Summary:
  Total URLs checked: 127
  OK:                 126
  Broken:             1
  Errors:             0
  Internal:           115
  External:           12

Timing:
  Total crawl time:   2345ms
  Avg response time:  18ms
  Min response time:  8ms
  Max response time:  156ms
```

For downloading images, use the `images` command:

```
$ argiope images https://example.com/gallery -o ./images

Downloading images from https://example.com/gallery...

Downloaded: page_1/image_1.jpg
Downloaded: page_1/image_2.jpg
Downloaded: page_2/image_1.png
...

Downloaded 42 images to ./images
```

Generate a report file instead of printing to the console:

```
argiope check https://christianhelle.com --report report.html --report-format html

argiope check https://christianhelle.com --report report.md --report-format markdown --include-positives
```

Use parallel crawling for faster processing on sites with many links:

```
argiope check https://christianhelle.com --parallel --depth 5
```

## Distribution

Like my previous Zig project, I wanted simple distribution across platforms. GitHub Copilot generated the installation scripts and snapcraft configuration.

The `install.sh` script downloads the latest release for Linux or macOS:

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

URL="https://github.com/christianhelle/argiope/releases/latest/download/argiope-$PLATFORM"

curl -L "$URL" -o argiope
chmod +x argiope
sudo mv argiope /usr/local/bin/

echo "argiope installed successfully!"
```

For Windows users, `install.ps1` does the same:

```powershell
$ErrorActionPreference = "Stop"

$url = "https://github.com/christianhelle/argiope/releases/latest/download/argiope-windows-x86_64.exe"
$dest = "$env:USERPROFILE\bin\argiope.exe"

New-Item -ItemType Directory -Force -Path (Split-Path $dest) | Out-Null
Invoke-WebRequest -Uri $url -OutFile $dest

Write-Host "argiope installed to $dest"
Write-Host "Add $env:USERPROFILE\bin to your PATH if needed."
```

The `snapcraft.yaml` configuration allows publishing to the Snap Store:

```yaml
name: argiope
base: core22
version: "0.1.0"
summary: A web crawler for broken-link detection
description: |
  A fast, multi-threaded web crawler that detects broken links,
  generates reports, and downloads images.
grade: stable
confinement: strict

apps:
  argiope:
    command: bin/argiope
    plugs:
      - network
      - home
```

The GitHub Actions workflow builds binaries for all platforms and attaches them to releases:

```yaml
jobs:
  build:
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            target: x86_64-linux
          - os: ubuntu-latest
            target: aarch64-linux
          - os: macos-latest
            target: x86_64-macos
          - os: macos-latest
            target: aarch64-macos
          - os: windows-latest
            target: x86_64-windows
```

## Conclusion

Building Argiope was a great exercise in working with Zig's standard library, particularly the HTTP client and file system APIs. The tool is fast, produces a single static binary with zero dependencies, and runs on Linux, macOS, and Windows.

If you need to check your website for broken links or download images from web pages, give Argiope a try. The source code is on GitHub at [https://github.com/christianhelle/argiope](https://github.com/christianhelle/argiope).
