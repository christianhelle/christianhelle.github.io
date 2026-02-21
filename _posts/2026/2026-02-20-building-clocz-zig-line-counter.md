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

For language detection, I went with a simple extension-based lookup. The file extension is extracted, lower cased, and matched against a list of known languages. It supports over 60 languages, handling single-line and block comments specific to each language.

## Command Line Parsing

Currently the tool only really a single functional argument, which is the folder to scan. I manually iterate through the arguments provided by `std.process.argsAlloc`. The implementation checks for flags like `-h` or `--help` and interprets any non-flag argument as the target directory path. If an unknown option is encountered, it helpfully prints the usage information and exits.

## Performance and Progress

One of the goals was to see how fast Zig can be compared to the original Perl version. The tool reads files into memory buffers using `readToEndAlloc` (up to 128MB) and quickly scans for line breaks and comments. It also detects binary files to avoid counting them as source code.

While the main thread and worker threads are busy scanning, a separate thread runs a simple progress loop. It sleeps for 100ms and prints the current count of scanned files (`\rScanning... {d} files`), giving visual feedback without slowing down the processing.

The final output is a sorted table showing the number of files, blank lines, comments, and code lines for each language, along with the total time taken and files processed per second.

## Usage

The CLI is simple. You run `clocz` to scan the current directory, or `clocz [path]` to scan a specific directory. The `-h` and `-v` flags show help and version info respectively.

Here's an example of the output for scanning a reasonably large system with a bankend written in C# and a React frontend, running on a 10 year old laptop with a Intel Core i7-7660U @ 4x 4GHz CPU with 8GB of RAM:

```
Scanned 10822 files

------------------------------------------------------------------------
Language                          files    blank    comment       code
------------------------------------------------------------------------
C#                                 4045    26273      19500     170743
TSX                                 720     5119       1248      58860
TypeScript                          726     4123       2585      43640
Markdown                             59     1347          0       4480
CSS                                   9      370         52       2889
PowerShell                           40      460        305       2046
HTML                                  4       32         12       1619
JavaScript                            4      193        167       1034
Shell                                 4       22         14        108
XML                                   1        0          0          9
------------------------------------------------------------------------
SUM:                               5612    37939      23883     285428
------------------------------------------------------------------------
Time=0.50s  (11203.0 files/s)
```

The tool is a single static binary with zero external dependencies, making it easy to distribute and run on Linux, macOS, and Windows.

## Distribution

Since I wanted this to be easy to use, I asked GitHub Copilot to help me set up the distribution channels. It generated the `install.sh` and `install.ps1` scripts for quick installation on Linux/macOS and Windows respectively.

It also wrote the `snapcraft.yaml` file so I could publish `clocz` to the Snap Store.

```yaml
name: clocz
base: core22
version: '0.1.0'
summary: A fast line counter written in Zig
description: |
  A fast, multi-threaded command-line tool for counting lines of code.
grade: stable
confinement: strict
```

The GitHub Actions workflow (`release.yml`) builds binaries for Linux (x86_64, aarch64), macOS (x86_64, aarch64), and Windows (x86_64) and attaches them to the GitHub Release.

## Conclusion

I was pleasantly surprised by how productive I could be with Zig and Copilot, building a performant and cross-platform tool in such a short amount of time. If you want to check it out or contribute, the source code is on GitHub at [https://github.com/christianhelle/clocz](https://github.com/christianhelle/clocz).
