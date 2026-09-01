---
layout: post
title: Bringing GitHub-like Agentic Workflows to Azure DevOps with the azdocli Skill
date: 2026-09-01
author: Christian Helle
tags:
  - Azure DevOps
  - Rust
  - AI
  - Agents
  - Copilot
  - CLI
redirect_from:
  - /2026/09/01/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill
  - /2026/09/01/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill/
  - /2026/09/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill
  - /2026/09/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill/
  - /2026/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill
  - /2026/bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill/
  - /bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill
  - /bringing-github-like-agent-experience-to-azure-devops-with-azdocli-skill/
---

If you have used GitHub with an AI coding agent, you know how natural it feels. You can just say "create a pull request for this branch" or "show me the failed workflow runs" and the agent handles the rest. No context switching, no copying URLs, no remembering CLI flags. The agent knows the repo, knows the branch, and does the right `gh` command for you.

I wanted that same experience for Azure DevOps.

For years Azure DevOps has had powerful APIs and a capable web UI, but the command-line story has always felt fragmented. The official `az devops` extension through the Azure CLI works, but it is Python-based, slow to start, heavy on dependencies, and not designed for agentic workflows. When I built [azdocli](https://github.com/christianhelle/azdocli) — a fast, native Azure DevOps CLI written in Rust — I focused on speed, single-binary distribution, and scripting ergonomics. But speed alone does not give you the GitHub-like "just tell the agent what you want" experience.

That last mile is what the [azdocli skill](../skills/azdocli) solves. It teaches your AI agent how to use `azdocli` the way the GitHub skill teaches it to use `gh`. Once installed, you can talk to your agent in plain English and it will translate your intent into the right `azdocli` commands, with the right flags, defaults, and safety checks.

In this post I will walk you through the entire experience in detail: what the skill is, how to install it, how it decides when to trigger, and how to use it across every major Azure DevOps area — repos, pull requests, pipelines, boards, projects, wikis, and even cross-tenant migrations — with example prompts you can copy-paste and adapt.

## What is azdocli?

[azdocli](https://github.com/christianhelle/azdocli) is a fast, cross-platform CLI for Azure DevOps written in Rust. It compiles to a single static binary with zero runtime dependencies and is designed for both interactive use and scripting.

Think of it as the `gh` equivalent for Azure DevOps. It covers:

- **Repos** — list, create, delete, clone, and pull request management
- **Pipelines** — list pipelines, inspect runs, trigger builds
- **Boards** — create, list, show, update, and delete work items (bugs, tasks, user stories, features, epics)
- **Projects** — list, show, create, and delete team projects
- **Users** — manage organization users and licenses
- **Wiki** — browse, search, and manage wiki pages
- **Migration** — experimental cross-tenant project migration

The CLI is built on top of the [Azure DevOps Rust API](https://github.com/microsoft/azure-devops-rust-api), which itself is auto-generated from OpenAPI specifications. This gives us strong typing and fast iteration when Azure DevOps APIs evolve.

Why Rust? Two reasons that compound for agentic use: startup time and distribution. Commands execute in 50–200 milliseconds — faster than you can blink — so an agent can chain several calls without the user noticing latency. And there is nothing to install beyond a single binary. No Python, no Node, no `az extension add` dance. That matters a lot when the agent is the one doing the installing.

If you have not used azdocli before, the [announcement post](/2025/06/azure-devops-cli.html) covers the origin story, key features, and design decisions in detail.

## What is the azdocli Skill?

The [azdocli skill](../skills/azdocli) lives in `skills/azdocli` and consists of two files:

- **`SKILL.md`** — the compact instruction set the agent loads into context. It contains installation steps, authentication setup, default project configuration, and the core pull request workflow with quick-reference tables for every other command group.
- **`REFERENCE.md`** — the full command catalog with every flag, default, and example for every subcommand. The agent consults this when it needs precise syntax.

This two-file split is intentional. `SKILL.md` stays lean so the agent can keep it resident without wasting tokens, while `REFERENCE.md` is loaded on demand when the user asks for something that needs exact flags.

The skill triggers whenever the user intent matches Azure DevOps work or when they explicitly say "create a pull request". Once triggered, the agent knows to:

1. Check if `azdocli` is installed and install it if not, using the platform-appropriate one-liner
2. Verify authentication (`azdocli login` and `azdocli project` defaults)
3. Derive repo and branch names from the local git state when possible
4. Execute the right `azdocli` subcommand with sensible defaults

That is the same shape as the GitHub skill: intent in, correct CLI invocation out, with the agent handling the glue.

## Installation and Authentication

The first thing the agent does in any Azure DevOps session is make sure `azdocli` is available and authenticated. You do not need to remember the install commands — the agent does that for you — but it helps to know what happens under the hood.

### Installing azdocli

If `azdocli` is not installed, the agent will pick the right one-liner for the current platform:

| Platform | Command the agent runs |
|----------|------------------------|
| Linux/macOS | `curl -fsSL https://christianhelle.com/azdocli/install \| bash` |
| Windows (PowerShell) | `iwr -useb https://christianhelle.com/azdocli/install.ps1 \| iex` |
| Cargo (any) | `cargo install azdocli` |
| Linux (Snap) | `snap install azdocli` |

It then verifies with `azdocli --version`.

**Example prompts:**

```text
Install azdocli for me
```

```text
Check if azdocli is installed and set it up if not
```

```text
I'm on Windows — set up azdocli so we can work with Azure DevOps
```

In each case the agent detects the OS, runs the correct installer, and confirms the binary is on the PATH. You can also be explicit about the method: "install azdocli via cargo" will make the agent prefer `cargo install azdocli` even on Linux where the curl installer would otherwise be the default.

### Logging in with a PAT

Azure DevOps authenticates via Personal Access Tokens (PATs). The agent will prompt you when needed:

```bash
azdocli login
# prompts for: organization URL/name + PAT

# Named profile for cross-tenant scenarios
azdocli login --profile my-org
```

You create the PAT in the web UI under User Settings → Personal Access Tokens. For `azdocli` you typically need **Code** (Read & Write), **Build** (Read & Execute), **Work Items** (Read & Write), and **Project and Team** (Read) scopes.

**Example prompts:**

```text
Log me into Azure DevOps — my org is contoso
```

```text
Set up auth for two tenants, one as source and one as target
```

```text
I'm getting an auth error — re-run login for my profile and check the config
```

The agent knows that `azdocli login` is interactive and will walk you through the PAT prompt. It also knows about `--profile` for named credential profiles, which becomes important in the migration scenario later.

To clear credentials the agent can run `azdocli logout`, which removes stored credentials and config.

## Setting a Default Project

One of the most satisfying quality-of-life features in azdocli came from pure frustration during development: constantly typing `--project MyProject` for every command. The fix is a single setup step:

```bash
azdocli project "MyProject"   # set default
azdocli project               # view current default
```

Once set, every `--project` flag becomes optional. The agent knows this and will offer to configure it for you. If you do not set one, it will explicitly pass `--project YourProject` on each command. If you do, it omits the flag and the CLI uses the stored default.

**Example prompts:**

```text
Set my default project to ECommercePlatform
```

```text
What project am I currently defaulting to?
```

```text
List my repos — I'm defaulting to MyProject so you shouldn't need the flag
```

```text
List pipelines in the OtherProject project specifically, not my default
```

This mirrors how `gh` remembers your repo via the git remote. The agent handles both cases: deriving context from git where it can, and remembering project scope where you told it to.

