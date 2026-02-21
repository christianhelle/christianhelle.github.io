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

The tool, called `clocz`, is a fast, multi-threaded command-line tool that counts code, comment, and blank lines per language. It uses Zig's `std.Thread.Pool` for directory scanning and supports over 60 languages out of the box. Being a single static binary with zero external dependencies, it's easy to distribute and run on Linux, macOS, and Windows.

I was pleasantly surprised by how productive I could be with Zig and Copilot, building a performant and cross-platform tool in such a short amount of time.
