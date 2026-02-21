---
layout: post
title: Building a fast line counter in Zig in one evening
date: 2026-02-20
author: Christian Helle
tags:
  - Zig
  - Performance
  - CLI
  - GitHub Copilot
---

I recently wrote a CLI tool for counting lines of code in [Zig](https://ziglang.org/). This was nothing more than a coding exercise to see how fast a Zig compiled tool would be compared to the well-known tool [cloc](https://github.com/AlDanial/cloc), which was originally written in Perl.

The source code is available on GitHub at [https://github.com/christianhelle/clocz](https://github.com/christianhelle/clocz).

It only took me an evening to build, and GitHub Copilot wrote the GitHub workflows, README, install scripts, and the snapcraft.yaml file.

## How it works

The implementation is quite straightforward. It uses `std.fs.Dir.iterate` to walk the directory tree. For each file found, a job is spawned in a `std.Thread.Pool`. This allows the tool to process multiple files in parallel, maximizing I/O and CPU usage.

I added some basic filtering to skip hidden files (starting with `.`) and directories like `node_modules` and `vendor`, as these usually contain dependencies rather than source code. There's also a file size limit of 128MB to avoid loading massive files into memory.

For language detection, I went with a simple extension-based lookup. The file extension is extracted, lowercased, and matched against a list of known languages. It supports over 60 languages, handling single-line and block comments specific to each language.

## Performance and Progress

One of the goals was to see how fast Zig can be compared to Perl. The tool reads files into memory buffers using `readToEndAlloc` (up to 128MB) and quickly scans for line breaks and comments. It also detects binary files to avoid counting them as source code.

While the main thread and worker threads are busy scanning, a separate thread runs a simple progress loop. It sleeps for 100ms and prints the current count of scanned files (`\rScanning... {d} files`), giving visual feedback without slowing down the processing.

The final output is a sorted table showing the number of files, blank lines, comments, and code lines for each language, along with the total time taken and files processed per second.

## Usage

The CLI is simple. You run `clocz` to scan the current directory, or `clocz [path]` to scan a specific directory. The `-h` and `-v` flags show help and version info respectively.

Here's an example of the output:

```
------------------------------------------------------------------------
Language                          files    blank    comment       code
------------------------------------------------------------------------
Zig                                   6       45         22        340
Markdown                              1        8          0         30
------------------------------------------------------------------------
SUM:                                  7       53         22        370
------------------------------------------------------------------------
Time=0.01s  (700.0 files/s)
```

The tool is a single static binary with zero external dependencies, making it easy to distribute and run on Linux, macOS, and Windows.

I was pleasantly surprised by how productive I could be with Zig and Copilot, building a performant and cross-platform tool in such a short amount of time.
