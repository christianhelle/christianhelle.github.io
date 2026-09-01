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

