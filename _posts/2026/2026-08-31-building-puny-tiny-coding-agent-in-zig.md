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

## Build system

The build is intentionally boring — `build.zig` with `ReleaseSmall` as the default optimization:

```zig
pub fn build(b: *std.Build) !void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});
    const docker = b.option(bool, "docker", "Build for Docker container") orelse false;

    const build_options = createBuildInfoOptions(b);
    build_options.addOption(bool, "docker", docker);

    const exe = addPunyExecutable(b, "puny", target, optimize, build_options);
    b.installArtifact(exe);

    const exe_tests = b.addTest(.{
        .root_module = exe.root_module,
        .test_runner = .{ .path = b.path("src/custom_test_runner.zig"), .mode = .server },
    });
    const run_exe_tests = b.addRunArtifact(exe_tests);
    const test_step = b.step("test", "Run unit tests");
    test_step.dependOn(&run_exe_tests.step);
    // ... docker, cross-targets, regression checker, provider regeneration
}
```

The test harness is a custom `custom_test_runner.zig` that suppresses noisy warning logs and stack traces during `zig build test`.

Cross-platform coverage is built into `zig build test-regression`:

```zig
const cross_targets = [_][]const u8{
    "x86_64-linux", "aarch64-linux",
    "x86_64-macos", "aarch64-macos",
    "x86_64-windows-gnu", "aarch64-windows-gnu",
};
for (cross_targets) |triple| {
    const cross_query = try std.Target.Query.parse(.{ .arch_os_abi = triple });
    const cross_target = b.resolveTargetQuery(cross_query);
    const cross_exe = addPunyExecutable(b, b.fmt("puny-{s}", .{triple}), cross_target, optimize, build_options);
    const cross_step = b.step(b.fmt("build-{s}", .{triple}), b.fmt("Build for {s}", .{triple}));
    cross_step.dependOn(&cross_exe.step);
    test_regression_step.dependOn(cross_step);
}
```

Provider clients are generated from OpenAPI specs via `openapi2zig` and wired into a single regenerate step (`zig build regenerate-providers`) that also patches Google's quirks (fixing `/v1beta/ss/` → `/v1beta/models/` and `Bearer` → `x-goog-api-key`).

Common commands:

```bash
zig build                  # Debug build → zig-out/bin/puny
zig build test             # Unit tests (custom runner, suppressed warnings)
zig build test-regression  # Cross-targets + unit + slow + regression checker
zig build install-release  # ReleaseSmall → $HOME/.local/bin/puny
```

Install directory honors `INSTALL_DIR` or `--prefix`, and falls back to `$HOME/.local/bin` / `%USERPROFILE%\.local\bin`.

## Providers and transports

Puny supports four providers via a single `Provider` tagged union. Each provider is a thin wrapper around a shared `client.Client` — the difference is which HTTP transport it uses per model:

```zig
pub const Provider = union(enum) {
    lmstudio: client.Client,
    opencode: client.Client,
    opencode_go: client.Client,
    copilot: copilot.Client,
    mock: mock.MockClient,

    pub fn chatStreaming(self: *Provider, request: openai.ChatRequest, callback: openai.StreamCallback) !void {
        return switch (self.*) {
            .lmstudio => |*c| chatStreamingCaptured(c, request, callback, chatStreamingOpenAi),
            .opencode => |*c| if (opencode_zen.isResponsesModel(request.model))
                chatStreamingCaptured(c, request, callback, chatStreamingResponses)
            else if (opencode_zen.isAnthropicModel(request.model))
                chatStreamingCaptured(c, request, callback, anthropic.chatStreaming)
            else if (google.isGoogleModel(request.model))
                chatStreamingCaptured(c, request, callback, google.chatStreamingGoogle)
            else
                chatStreamingCaptured(c, request, callback, chatStreamingOpenAi),
            .opencode_go => |*c| if (opencode_go.isResponsesModel(request.model))
                chatStreamingCaptured(c, request, callback, chatStreamingResponses)
            else if (opencode_go.isAnthropicModel(request.model))
                chatStreamingCaptured(c, request, callback, anthropic.chatStreaming)
            else
                chatStreamingCaptured(c, request, callback, chatStreamingOpenAi),
            .copilot => |*c| chatStreamingCopilotCaptured(c, request, callback),
            .mock => |*c| c.chatStreaming(request, callback),
        };
    }
};
```

Transports in `src/providers/`:

- **OpenAI `/v1/chat/completions`** — LM Studio, OpenCode Zen/Go (DeepSeek, GPT, GLM, Kimi, MiMo, Grok), Copilot
- **Anthropic `/v1/messages`** — OpenCode Go (MiniMax, Qwen), OpenCode Zen (Claude, Qwen)
- **Google `/v1/models/<model>:streamGenerateContent`** — OpenCode Zen (Gemini)
- **Responses `/v1/responses`** — OpenCode Zen/Go (Muse Spark, GPT, Grok variants)

Every streaming path goes through `chatStreamingCaptured`, which captures non-success HTTP bodies into `HttpFailure` so error reporting survives body drainage. Provider clients themselves are generated from OpenAPI via `openapi2zig` (`src/providers/openai`, `lmstudio`, `anthropic`, `google`) sharing a single `runtime.zig`.

**Copilot auth** is the most involved flow: resolve an OAuth token (`--api-key` → `PUNY_API_KEY` → `config.json` → `GITHUB_COPILOT_OAUTH_TOKEN` → `apps.json`/`hosts.json` discovery → device-flow login), exchange it for a short-lived Copilot token, filter the model list to picker-enabled `chat` models, and hide legacy/internal models from the picker.

**HTTP internals** use `std.http` with SSE streaming, cancellation-aware reads (Ctrl+C closes the socket via `core/cancel.zig` + `sigint.zig`), and a `HttpFailureCapture` observer that records the raw error body for the debug log and user-facing hints.

## Tool calling — the curated set

Puny exposes a small, fixed tool registry. The model sees these tools on every request; when it decides to call one, Puny executes it immediately (YOLO mode — no confirmation) and feeds the result back.

```zig
pub const registry = blk: {
    @setEvalBranchQuota(10000);
    break :blk &[_]Tool{
        filesystem.read_file,
        filesystem.write_file,
        filesystem.list_directory,
        shell.execute_shell,
        search.grep_search,
        git.git_status,
        git.git_diff,
        web.web_fetch,
        skill_loader.load_skill,
    };
};

pub const planning_registry = blk: {
    @setEvalBranchQuota(10000);
    break :blk &[_]Tool{
        filesystem.read_file,
        filesystem.list_directory,
        search.grep_search,
        web.web_fetch,
        git.git_status,
        git.git_diff,
        core_session.save_prd_tool,
        skill_loader.load_skill,
    };
};
```

Planning mode trims the set further — no `write_file` or `execute_shell` until the PRD is saved, where `save_prd` writes both `plan.md` and `plan.html` to the session folder:

```zig
pub const save_prd_tool = Tool{
    .name = "save_prd",
    .description = "Save the Product Requirements Document...",
    .schema = savePrdSchema,
    .execute = struct {
        pub fn exec(allocator: std.mem.Allocator, io: std.Io, args: std.json.Value) ![]const u8 {
            var md_file = try std.Io.Dir.cwd().createFile(io, prd_path_global, .{});
            defer md_file.close(io);
            try md_file.writeStreamingAll(io, markdown.string);
            var html_file = try std.Io.Dir.cwd().createFile(io, html_path_global, .{});
            defer html_file.close(io);
            try html_file.writeStreamingAll(io, html.string);
            return std.fmt.allocPrint(allocator, " - {s}\n - {s}", .{ abs_md, abs_html });
        }
    }.exec,
};
```

Each tool is defined with a comptime schema:

```zig
pub fn defineTool(
    comptime name: []const u8,
    comptime description: []const u8,
    comptime Params: type,
    comptime handler: fn (allocator: std.mem.Allocator, io: std.Io, params: Params) anyerror![]const u8,
) Tool {
    const Schema = schema.ToolDefinition(name, description, Params);
    return .{
        .name = name,
        .description = description,
        .schema = Schema.schema,
        .execute = struct {
            pub fn exec(allocator: std.mem.Allocator, io: std.Io, args: std.json.Value) ![]const u8 {
                const parsed = try std.json.parseFromValue(Params, allocator, args, .{});
                defer parsed.deinit();
                return handler(allocator, io, parsed.value);
            }
        }.exec,
    };
}
```

Status lines are summarized — `🔧 Reading "src/main.zig"`, `🔧 Running "zig build test"`, `🔧 Writing 12 lines (384 bytes) to "README.md"` — large payloads are counted, not dumped.

## Skills system

Puny loads reusable prompt-engineering skills from markdown files — no plugin runtime, just a `SKILL.md` with YAML frontmatter:

```markdown
---
name: my-skill
description: >
  Expert knowledge of MyTool for integration testing.
  Covers setup, configuration, and common patterns.
triggers: mytool, integration test, configure
disable-model-invocation: false
---

# MyTool Instructions

When asked about MyTool, follow these guidelines...
```

Two locations are scanned:

- **Global**: `~/.agents/skills/` — shared across repos
- **Repository**: `<repo-root>/.agents/skills/` — per-project, rooted via `git rev-parse --show-toplevel`

Each subdirectory is a skill; the directory name *is* the skill name. The registry does a light scan (directory names only) at startup and a full scan (parsing `SKILL.md` frontmatter) right after:

```zig
var skill_registry = skills.Registry.init(arena);
defer skill_registry.deinit();
if (!parsed.no_skills) {
    if (try skills.homeDir(arena, init.environ_map)) |home| {
        const global_path = try std.fs.path.join(arena, &.{ home, ".agents", "skills" });
        try skill_registry.lightScan(init.io, global_path);
    }
    if (try git_root.findGitRepoRoot(arena, init.io)) |repo_root| {
        const repo_path = try std.fs.path.join(arena, &.{ repo_root, ".agents", "skills" });
        try skill_registry.lightScan(init.io, repo_path);
    }
    skill_registry.fullScan(init.io) catch {};
}
skills.setGlobalRegistry(&skill_registry);
```

Loaded skills are injected as system messages; the registry listing becomes an `<available_skills>` block in the system prompt:

```xml
<available_skills>
  <skill>
    <name>nano-commits</name>
    <description>Commit often. One logical change per commit.</description>
  </skill>
</available_skills>
```

Skills load three ways:

1. **Slash command** — `/nano-commits`, `/grill-me`, any directory name
2. **Keyword trigger** — mentioning a `triggers` phrase or the directory name as a whole word in your message automatically loads the skill
3. **Model invocation** — the model calls the `load_skill` tool when it sees a relevant entry in `<available_skills>`. Skills with `disable-model-invocation: true` are slash-only.

`--no-skills` / `PUNY_NO_SKILLS=1` disables all of this; skill directories are never scanned and `/skills` reports that skills are disabled.

## Configuration and secrets

Puny stores configuration in `config.json` under a platform-appropriate directory:

- Linux/macOS: `$XDG_CONFIG_HOME/puny` or `~/.config/puny`
- Windows: `%APPDATA%\puny` or `%USERPROFILE%\puny`

On first launch, a setup wizard prompts for provider, URL (LM Studio only), and API key. Subsequent runs skip the wizard unless you pass `--reconfigure` or `/config`.

API keys are encrypted at rest with XChaCha20-Poly1305. The encryption key is a random 32-byte file:

- POSIX: `~/.local/share/puny/encryption.key` (`0600`)
- Windows: `%LOCALAPPDATA%\puny\encryption.key`

`config.json` itself is written `0600` on POSIX, and plaintext keys are migrated to `enc:v1:` blobs on the next save. `PUNY_API_KEY` / `--api-key` / `--api-key-file` remain session-only and are never persisted.

Per-provider settings (URL, API key, last-selected model, reasoning effort) are stored so switching providers doesn't require re-entering credentials.

Resolution order everywhere is `CLI flag` → `environment variable` → `config.json` → `default`. For example:

```
provider: --provider > PUNY_PROVIDER > config.json > lmstudio
api key:  --api-key > --api-key-file > PUNY_API_KEY > config.json
model:    --model > PUNY_MODEL > config.json (per-provider entry)
```

## Sessions — persistence that actually works

Every run creates a UUID-identified session folder:

```
~/.config/puny/sessions/<uuid>/
├── plan.md        # PRD markdown (planning mode)
├── plan.html      # PRD HTML
├── messages.json  # Full conversation (saved every turn)
└── session.json   # Metadata: planning mode, first prompt
```

Sessions use a v4 UUID with the variant/variant bits set explicitly — no external dependency:

```zig
pub fn generateUuid(random: std.Random, arena: std.mem.Allocator) ![]const u8 {
    var bytes: [16]u8 = undefined;
    random.bytes(&bytes);
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;

    const hex = "0123456789abcdef";
    var buf: [36]u8 = undefined;
    var i: usize = 0;
    var j: usize = 0;
    while (i < 16) : (i += 1) {
        if (i == 4 or i == 6 or i == 8 or i == 10) {
            buf[j] = '-'; j += 1;
        }
        buf[j] = hex[bytes[i] >> 4];
        buf[j + 1] = hex[bytes[i] & 0x0f];
        j += 2;
    }
    return try arena.dupe(u8, &buf);
}
```

Conversation persistence is automatic — after every turn, on `/quit`, on `Ctrl+C`, and on `/reset` — including tool results so a restored session has full context. Empty sessions (no user message, no reply, no PRD) are pruned automatically.

You can resume several ways:

```bash
puny --session abc-12          # By UUID prefix
puny --resume                  # Most recent with a conversation
puny --prune --session <uuid>  # Delete all except one
```

Or interactively: `/resume`, `/resume abc-12`, `/sessions`, `/prune`.

Planning mode is special-cased: read-only tools plus `save_prd`, with `planning_mode` and `first_prompt` persisted in `session.json` and restored so the conversation picks up exactly where it left off.

## Interactive chat and model selection

On startup, Puny resolves the provider, API key, and model — then shows an interactive model picker if no model is configured:

```zig
const configured_model: ?[]const u8 = blk: {
    const raw = if (reconfigure_force_picker) null
        else parsed.model orelse cfg.providerEntry(selected_provider.*).model;
    if (raw) |id| if (http_client.isValidUtf8(id)) break :blk id;
    break :blk null;
};

prov.* = resolver.createProvider(parsed.mock, selected_provider.*, provider_url.*, api_key, provider_arena, io);
if (!parsed.mock) try resolver.ensureCopilotAuth(arena, io, init, cfg, stdout_writer, prov);

const init_result = (try model_selection.select(
    prov, configured_model, arena, io, init, skip_validation,
    cfg, selected_provider.*, init.environ_map, random,
)) orelse {
    // Fallback: show picker when the configured model isn't in the running list
    break :blk try model_selection.select(prov, null, arena, io, init, false, cfg, selected_provider.*, init.environ_map, random);
};
model_key.* = init_result.model_key;
```

The chat loop is cancellation-aware, prompt-history-aware, skill-aware, and session-persistent. Slash commands handled inline:

- `/quit` / `/exit`, `/new` / `/reset`, `/stats`, `/config` (reconfigure mid-session)
- `/plan [task]` / `/build [task]`, `/model [id]`, `/provider [name]`, `/thinking [level]`
- `/sessions`, `/resume [id]`, `/prune`, `/skills`, `/file <path|url>`

The welcome screen shows provider, URL, model, reasoning effort, session ID, and whether the session is one-shot or prefilled. Startup time is printed with ANSI dim:

```zig
fn printStartupTime(io: std.Io, stdout_writer: *std.Io.Writer, startup_time: std.Io.Clock.Timestamp) !void {
    const now = std.Io.Clock.Timestamp.now(io, .awake);
    const elapsed_ns: u64 = @intCast(startup_time.raw.durationTo(now.raw).nanoseconds);
    var startup_buf: [64]u8 = undefined;
    try stdout_writer.print("{s}Startup time: {s}{s}", .{
        ansi.dim, formatDuration(&startup_buf, elapsed_ns), ansi.reset,
    });
    try stdout_writer.flush();
}
```

## Installation and usage

Quick install:

```bash
# Linux/macOS
curl -fsSL https://christianhelle.com/puny/install | bash

# Windows (PowerShell)
irm https://christianhelle.com/puny/install.ps1 | iex

# Pin or customize
VERSION=v0.1.0 curl -fsSL https://christianhelle.com/puny/install | bash
curl -fsSL https://christianhelle.com/puny/install | bash -s -- --dir "$HOME/.local/bin"
```

Build from source (requires Zig 0.16.0+):

```bash
git clone https://github.com/christianhelle/puny.git
cd puny
zig build install-release
```

Usage:

```bash
puny                                          # Interactive chat (wizard on first run)
puny --provider opencode --api-key $KEY        # Hosted model
puny --provider copilot                       # GitHub Copilot device-flow
puny --prompt "List all source files" --oneshot
puny --prompt-file spec.md --oneshot
puny --prompt-file https://example.com/spec.md
puny --mock --model mock-model --prompt "search" --oneshot   # No backend
```

Puny also supports `--prompt-file` / `/file` for file-or-URL prompts (10 MiB, 30 s timeout), `--show-thinking` for reasoning output, and `--debug` / `--chat-log` for `puny_debug.log` / `puny_chat.log`.

## Distribution and miscellany

Like my other Zig tools, distribution is automatic: GitHub Actions builds `tar.gz` / `zip` artifacts for six targets and attaches them to releases. `install.sh` / `install.ps1` detect OS and architecture, handle `--dir`/`--prefix` and `VERSION` pinning, and `puny --upgrade` re-runs the same script.

A minimal Docker path is also included — `zig build -Ddocker -Doptimize=ReleaseSmall -Dtarget=x86_64-linux` plus a generated `Dockerfile` from `alpine:latest` with a non-root `puny` user. The encryption key lives in the container's writable layer (`/app/.local/share/puny/encryption.key`), so mount a volume if you want persisted keys.

Puny ships with a mock provider (`--mock` / `PUNY_MOCK=1`) that simulates tool calls from keywords (`read`→`read_file`, `search`→`grep_search`, `reasoning`→streamed thinking) so the whole loop is testable without a backend. A background update checker spawns a detached child after the welcome screen (skipped in `--oneshot`), writes a flag file, and surfaces an update notice on the next run.

```bash
zig build run -- --mock --model mock-model --prompt "respond with reasoning" --show-thinking --oneshot
puny --debug    # → puny_debug.log (pretty-printed SSE)
puny --chat-log # → puny_chat.log (full conversation, reasoning included)
```

## Conclusion

Puny is the tool I wanted but couldn't find — a tiny, natively compiled, single-binary coding agent that starts instantly, stays small, and only does what I actually need. Building it in Zig kept the binary at ~1 MB, the startup at ~1 ms, and the codebase honest about ownership and I/O. No runtime, no framework, no plugin ecosystem — just a tool that reads, edits, searches, and runs commands.

The source code is on GitHub at [https://github.com/christianhelle/puny](https://github.com/christianhelle/puny). Give it a try, file an issue, or send a PR. And if you want parallel work, run another instance.

This is part of my ongoing Zig journey. For more Zig tools, see [HTTP File Runner](/2025/06/http-file-runner-zig-tool.html), [chlogr](/2025/11/building-a-github-changelog-generator-in-zig.html), [clocz](/2026/02/building-clocz-zig-line-counter.html), [argiope](/2026/03/building-argiope-web-crawler-broken-link-detector.html), and [ZigFaker](/2026/03/zigfaker.html).
