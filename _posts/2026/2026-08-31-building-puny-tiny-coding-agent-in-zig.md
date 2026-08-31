---
layout: post
title: Building Puny — A Tiny Coding Agent in Zig
date: 2026-08-31
author: Christian Helle
tags:
  - Zig
  - CLI
  - AI
  - LLM
redirect_from:
  - /2026/08/building-puny
  - /2026/08/building-puny/
  - /2026/building-puny
  - /2026/building-puny/
  - /building-puny
  - /building-puny/
---

I recently built [Puny](https://github.com/christianhelle/puny) — a minimal, natively compiled, single-binary terminal coding agent with a ~1 MB footprint.

The source code is available on GitHub at [https://github.com/christianhelle/puny](https://github.com/christianhelle/puny).

Like most of my recent projects, GitHub Copilot wrote the boilerplate including workflows, README, install scripts, and `snapcraft.yaml`. The core took a few focused evenings. Puny is opinionated, fast, and intentionally small.

## The problem Puny is solving

Modern coding agents are heavy. They ship as Electron apps, Node.js bundles with hundreds of megabytes of dependencies, or Python packages that pull half of PyPI. They start slowly, idle hot, animate their spinners, and insist on MCP servers, subagents, plugins, and extensions I never asked for. Worse, some try to take credit for your work by injecting themselves as co-authors on your commits.

I wanted the opposite:

- I work over SSH on slow remote VMs, on a Raspberry Pi, and on a decade-old laptop.
- I want a tool that starts in under 1 millisecond, fits in ~1 MB on disk, uses minimal RAM, stays at <1% CPU, and gets out of my way.
- I want a *tool*, not a platform. Read files, write files, run commands, search code, fetch a URL, load a skill. If I want parallelism, I'll run another instance.

Puny is that tool. It lets you chat with an LLM and gives it a curated set of coding tools so it can read, edit, search, and inspect your codebase — nothing more. No hidden runtime, no garbage collector, no Node.js, no JavaScript.

The four providers I actually use are the only ones Puny supports:

- [LM Studio](https://lmstudio.ai/) — local inference on my own hardware
- [OpenCode Zen](https://opencode.ai/zen) — reliable hosted models
- [OpenCode Go](https://opencode.ai/go) — cheaper hosted models
- [GitHub Copilot](https://github.com/features/copilot) — bundled with my sponsored GitHub subscription

If you want another provider badly enough, you can add it — but Puny will never become a comparison-table feature dump. Each feature has to earn its place.

## Why Zig

Zig was the obvious choice for a tool that has to be tiny, fast, and portable.

- **No runtime**: no GC, no hidden allocations. Startup is a direct `main` entry with an arena allocator — ~1 ms cold start.
- **Tiny binaries**: `ReleaseSmall` produces a ~1 MB single binary with zero external dependencies. It runs anywhere without installing a runtime.
- **Cross-compilation first-class**: one `build.zig` emits Linux (x86_64, aarch64), macOS (x86_64, aarch64), and Windows (x86_64, aarch64) artifacts. No matrix of Docker images.
- **Explicit control**: manual memory management with `ArenaAllocator` and `GeneralPurposeAllocator`, `defer`, and `errdefer` everywhere. When you read the code, you know who owns what.
- **Tooling**: `zig build`, `zig fmt`, `zig build test` — no extra package manager, no lock-file churn.

Compared to Rust, Zig's learning curve is flatter and the project stays closer to the metal without fighting the borrow checker. For a CLI that is mostly I/O and string handling, Zig's simplicity wins.

## Performance and resource usage

The numbers are the point.

| Metric | Puny |
|---|---|
| Binary size | ~1 MB (`ReleaseSmall`) |
| Cold start | ~1 ms |
| Idle CPU | <1% |
| Dependencies | 0 (single static binary) |
| Runtime | None (no GC, no Node, no Python) |

Startup time is measured from process entry to the welcome screen — before any network call. The binary is the whole tool; there is nothing to `npm install` or `pip install`. On a Raspberry Pi or a cheap VPS over SSH, that matters.

Memory usage is kept low by design:

- **Arena per session**: the main loop uses an `ArenaAllocator` backed by `page_allocator` for transient chat state. At session boundaries the arena is reset rather than freeing piecemeal.
- **Bounded reads**: file and prompt reads are capped (10 MiB limits on prompt files and remote fetches, 1 MiB skill files, 30 s remote timeout) so a single bad path can't blow up RAM.
- **Streaming by default**: LLM responses are streamed chunk-by-chunk with a buffered writer; the full response is never held twice.
- **No background threads**: apart from the detached update-check child process, Puny is single-threaded. No watchers, no LSP server, no file-system thread pool burning CPU in the background.

This is deliberate. Puny should be invisible on `htop`.

## Architecture overview

```
src/
├── main.zig              # Entry, startup timing, signal handling
├── build.zig / build.zig.zon
├── chat/                 # Interactive loop, session, display, stats
├── cli/                  # Argument parsing
├── config/               # config.json + XChaCha20-Poly1305 key encryption
├── core/                 # Session UUIDs, git root, cancel/sigint
├── providers/            # Provider union + 4 transports + OpenAPI clients
├── models/               # Model picker
├── skills/               # Skill registry (scan, frontmatter, triggers)
├── tools/                # Tool definitions (read/write/list, shell, grep, git, fetch, skill)
├── prompts/              # History + prompt-file loading
├── agents/               # Agent instructions (AGENTS.md)
├── tui/                  # Welcome screen, ANSI/VT helpers
├── sessions/             # Session store (list, resume, prune)
└── update_check.zig      # Background update check (detached child)
```

The entry point wires everything together in `main.zig`:

```zig
pub fn main(init: std.process.Init) !void {
    vt.enableAnsi();
    vt.enableUtf8();
    const arena: std.mem.Allocator = init.arena.allocator();
    var messages_arena_state = std.heap.ArenaAllocator.init(std.heap.page_allocator);
    const messages_arena = messages_arena_state.allocator();
    const startup_time = std.Io.Clock.Timestamp.now(init.io, .awake);

    upgrade.cleanupOldBackup(arena, init.io);

    const args_slice = try init.minimal.args.toSlice(arena);
    var parsed = cli.parseArgs(init.io, init.environ_map, args_slice);

    if (parsed.upgrade) {
        try upgrade.runUpgrade(arena, init.io, init.environ_map, parsed.force_upgrade);
        update_check.clearFlag(init.io, arena, init.environ_map) catch {};
        return;
    }

    // Detached child: PUNY_UPDATE_CHECK=1 means we are the background updater
    if (std.mem.eql(u8, init.environ_map.get(update_check.check_env_var) orelse "", "1")) {
        _ = update_check.runCheck(init.io, arena, init.environ_map) catch {};
        return;
    }
    // ... provider/model init, skill scan, system prompt, chat loop
}
```
