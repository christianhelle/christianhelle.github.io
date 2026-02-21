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

The tool, called `clocz`, is a fast, multi-threaded command-line tool that counts code, comment, and blank lines per language. It uses Zig's `std.Thread.Pool` for directory scanning and supports over 60 languages out of the box. Being a single static binary with zero external dependencies, it's easy to distribute and run on Linux, macOS, and Windows.

I was pleasantly surprised by how productive I could be with Zig and Copilot, building a performant and cross-platform tool in such a short amount of time.
