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

## The Core Loop: Creating and Managing Pull Requests

This is the skill's flagship scenario and the one most directly comparable to the GitHub experience. On GitHub you say "create a PR" and `gh` derives the repo and branch from git. With the azdocli skill, the agent does the equivalent for Azure DevOps.

### The GitHub-like Flow

When you are on a feature branch and say "create a pull request", the agent will:

1. Run `git remote get-url origin` and `git branch --show-current` to derive the repo and source branch
2. Compose `azdocli repos pr create` with sensible defaults

```bash
# What the agent actually runs for you
REPO=$(git remote get-url origin | sed 's/.*\/\(.*\)\.git/\1/')
BRANCH=$(git branch --show-current)

azdocli repos pr create \
  --repo "$REPO" \
  --source "$BRANCH" \
  --title "feat: add dark mode" \
  --description "Implements dark mode toggle"
```

Key defaults the agent knows about:

| Flag | Default | Notes |
|------|---------|-------|
| `--target` | `main` | Auto-prefixed with `refs/heads/` |
| `--title` | `Pull Request` | The agent will generate a meaningful title from your changes if you don't provide one |
| `--description` | *(none)* | The agent can derive this from commit messages or a description file |

**Example prompts:**

```text
Create a pull request for this branch
```

This is the canonical GitHub-like prompt. The agent derives `REPO` and `BRANCH` from git, targets `main`, and generates a title from your recent commits.

```text
Create a PR from feature/payment-gateway targeting develop with title "Add Stripe integration"
```

Explicit source, target, and title — the agent passes `--source feature/payment-gateway --target develop --title "Add Stripe integration"`.

```text
Open a pull request for my current branch and use the content of PR_DESCRIPTION.md as the description
```

The agent knows about `--description-file` via `azdocli repos pr update` and will create the PR then update it with the file content.

### Browsing and Inspecting PRs

After creation, the natural next step is to review what is out there. The skill covers the view side of the PR lifecycle:

```bash
# List PRs in a repo
azdocli repos pr list --repo MyRepo

# Show PR details
azdocli repos pr show --repo MyRepo --id 123

# Show commits in a PR
azdocli repos pr commits --repo MyRepo --id 123

# Update title or description
azdocli repos pr update --repo MyRepo --id 123 --title "New title"
azdocli repos pr update --repo MyRepo --id 123 --description-file ./description.md
```

**Example prompts:**

```text
Show me all open pull requests in the Backend repo
```

```text
What's the status of PR 204 in MyRepo? Who created it and what's the source branch?
```

```text
List the commits in PR 123 for the WebApp repo
```

```text
Update the title of PR 87 in MyRepo to "fix: handle null token in auth middleware"
```

```text
Replace the description of PR 87 with the contents of ./docs/pr-body.md
```

> Approval and merge still happen in the Azure DevOps web UI — azdocli is view and create only. The agent will tell you this rather than failing silently if you ask it to approve or merge.

## Repositories: Beyond Pull Requests

Pull requests get the spotlight, but the skill also handles everyday repo operations.

```bash
azdocli repos list                        # list all repos in the default project
azdocli repos show --id MyRepo            # repo details
azdocli repos create --name NewRepo       # create a repo
azdocli repos delete --id MyRepo          # delete (add --hard --yes for permanent)
azdocli repos clone                       # clone all repos from the default project
azdocli repos clone --target-dir ./repos --yes --parallel --concurrency 8
```

**Example prompts:**

```text
List all repos in my project
```

```text
Show me details for the Infrastructure repo — size, remote URL, default branch
```

```text
Create a new repo called Playground and show me the clone URL
```

```text
Clone all repos from my project in parallel into ./repos — skip confirmation
```

The bulk clone workflow is the one where `azdocli` really shines over the browser. When I got a new machine and needed to restore hundreds of repos, the difference between sequential browser cloning and `azdocli repos clone --parallel --concurrency 32 --yes` was the difference between 20 minutes of babysitting and a few seconds of unattended work. With the skill, you just say "clone everything in parallel" and the agent picks the right flags.

```text
Delete the temp-experiment repo but confirm before doing it
```

```text
Hard-delete the old-prototype repo without asking
```

The agent knows `--hard` means permanent deletion and `--yes` skips the confirmation prompt. It will not combine them unless you explicitly ask for destructive action.

## Pipelines: The CI/CD Pulse Check

This is where the GitHub Actions analogy is strongest. Just as you might say "show me the failed runs for this repo" with `gh`, you can now do the same for Azure Pipelines:

```bash
azdocli pipelines list                    # list all pipelines
azdocli pipelines runs --id 42            # show all runs for pipeline 42
azdocli pipelines show --id 42 --build-id 123  # details for a specific build
azdocli pipelines run --id 42             # trigger a new run (experimental)
```

**Example prompts:**

```text
List all pipelines in my project
```

```text
Show me the runs for pipeline 42 — any failures?
```

```text
What happened in build 123 of pipeline 42? Show me the details
```

```text
Trigger pipeline 42 for me
```

The agent knows `pipelines runs --id` shows the history for a single pipeline definition, while `pipelines show --id --build-id` drills into one specific execution. If you phrase it ambiguously like "show me pipeline 42", it will default to listing runs and ask if you wanted a specific build. The `--project` flag is optional if you have a default set, just like with repos.

A morning standup variant that mirrors my personal workflow:

```text
Before standup, check pipelines — list all pipelines and then show runs for the main build pipeline
```

The agent will chain `azdocli pipelines list` to identify the main pipeline, then `azdocli pipelines runs --id <n>` to show recent activity, all without extra prompting.

```text
Rerun the latest failed pipeline build
```

The agent will list runs, identify the failed one, and trigger it via `azdocli pipelines run --id`. It will warn you that `pipelines run` is currently registered but not yet fully implemented in some builds, so you know to confirm in the web UI if needed.

## Boards: Work Items as a Conversation

Azure Boards is where Azure DevOps diverges most from GitHub Issues, so the skill puts extra care into mapping natural language to the right work item type and state transitions.

```bash
azdocli boards work-item list                          # my work items (default 50)
azdocli boards work-item list --state Active --work-item-type Bug --limit 20
azdocli boards work-item show --id 123                 # show details
azdocli boards work-item show --id 123 --web           # open in browser
azdocli boards work-item create bug --title "Fix login"             # subcommand is the type
azdocli boards work-item update --id 123 --state Active --priority 2
azdocli boards work-item delete --id 123               # permanent
azdocli boards work-item delete --id 123 --soft-delete # state -> Removed
```

Supported create subcommands are `bug`, `task`, `user-story`, `feature`, and `epic`. The agent knows to map your natural language type to the correct subcommand.

**Example prompts:**

```text
Show my work items
```

Simplest standup prompt. The agent runs `azdocli boards work-item list` and summarizes what is assigned to you. It defaults to 50 results; you can narrow it.

```text
List active bugs assigned to me, max 20 results
```

The agent composes `azdocli boards work-item list --state Active --work-item-type Bug --limit 20`.

```text
Show me work item 456
```

```text
Open work item 456 in the browser
```

The `--web` flag opens the web UI directly. The agent knows to use it when you say "open in browser" versus just showing details inline.

```text
Create a bug called "Login fails after password change" 
```

```text
Create a user story "As a shopper I want to filter by size so that I can find my fit"
```

The agent maps "bug" → `azdocli boards work-item create bug` and "user story" → `azdocli boards work-item create user-story`. The `--title` flag is required; the agent will use your quoted text as the title.

```text
Update work item 123 — set it to Active and priority 1
```

```text
Close work item 123
```

Priority is 1–4, state values like `Active`, `Resolved`, `Closed` are passed through to `--state` and `--priority`.

```text
Delete work item 999 but soft-delete it so it can be recovered
```

The agent knows `--soft-delete` changes state to `Removed` instead of permanent deletion, and will prefer it unless you say "permanently" or "hard delete".

A combined workflow example:

```text
After my standup, create a task "Write integration tests for checkout" then list my active work items to confirm
```

The agent will run `azdocli boards work-item create task --title "Write integration tests for checkout"` followed by `azdocli boards work-item list --state Active`, chaining two commands in one turn.

## Projects, Users, and Wiki: The Rest of the Surface Area

The skill does not stop at repos, pipelines, and boards. It covers the broader Azure DevOps surface that you might otherwise reach only through the web portal.

### Projects

```bash
azdocli projects list                              # list team projects in the org
azdocli projects show --project MyProject           # show a project (or --open in browser)
azdocli projects create --name NewProject --description "My new project"
azdocli projects delete --id <project-guid> --yes   # GUID required — get via show
```

**Example prompts:**

```text
List all team projects in the organization
```

```text
Show me the details for the Platform project
```

```text
Create a new project called Sandbox with description "Experiments"
```

```text
Delete project <guid> — I already confirmed, skip the prompt
```

The agent knows `projects delete` requires a GUID, not a name, and will call `projects show --project <name>` first to resolve it if you provide a name.

### Users

```bash
azdocli user list
azdocli user show --email user@company.com
azdocli user show --id <uuid>
azdocli user add --email user@company.com --license express
azdocli user remove --email user@company.com
azdocli user update --email user@company.com --license stakeholder
```

Licenses are `none`, `earlyAdopter`, `express`, `professional`, `advanced`, and `stakeholder`.

**Example prompts:**

```text
List all users in the org
```

```text
Show me user@company.com
```

```text
Add user@company.com with an express license
```

```text
Change user@company.com to stakeholder
```

```text
Remove user@company.com from the organization
```

The agent enforces the mutual exclusivity of `--id` vs `--email` and validates the license values before calling the CLI.

### Wiki

```bash
azdocli wiki list                                   # list wikis in a project
azdocli wiki show [MyWiki]                          # details (auto-resolves if only one)
azdocli wiki page list [/Home] --wiki MyWiki        # list pages from a root path
azdocli wiki page show /Getting-Started             # show page content
azdocli wiki page show /Getting-Started --web       # open in browser
azdocli wiki page download /Getting-Started --dir ./docs
azdocli wiki page search "API key" --show-contents --limit 10
azdocli wiki page move /Old-Name /New-Name          # rename/move
```

**Example prompts:**

```text
List all wikis in my project
```

```text
Show me the wiki page at /Getting-Started
```

```text
Search the wiki for "deploy" and show content snippets, limit 10
```

```text
Download the wiki page /Onboarding to ./docs
```

```text
Rename the wiki page from /Old-Name to /New-Name
```

The agent knows wikis auto-resolve when only one exists, so it will omit `--wiki MyWiki` unless disambiguation is needed, and it knows `--dir` and `--overwrite` for downloads.

## Advanced Scenario: Cross-Tenant Migration

This is the skill's most advanced area and one that has no real GitHub equivalent: migrating an entire team project from one Azure DevOps organization to another. Because `azdocli migrate` touches so many resources, the skill is careful to lean on safety flags.

The migration feature requires **named credential profiles**. The agent will have already set these up via `azdocli login --profile <name>` during the authentication step, but if you start with migration it will prompt for them.

Available migration phases (which the agent can include or skip selectively) are: `project`, `process`, `areas`, `iterations`, `teams_create`, `teams_configure`, `repos`, `wikis`, `variable_groups`, `service_connections`, `work_items`, `wi_links`, `wi_attachments`, `wi_comments`, `prs`, `pipelines_yaml`, `pipelines_classic`, `test_plans`, `dashboards`.

### Single-Project Migration

```bash
azdocli migrate project \
  --source-profile src \
  --target-profile dst \
  --source "OldProject" \
  --target "NewProject" \
  --create-target \
  --dry-run

# Resume after interruption
azdocli migrate project --source-profile src --target-profile dst --source OldProject --resume

# Narrow to specific phases
azdocli migrate project --source-profile src --target-profile dst --source OldProject --phases repos,wikis,work_items
```

**Example prompts:**

```text
Migrate the OldProject team project from tenant A to tenant B — create the target if it doesn't exist, but do a dry run first
```

The agent will compose `azdocli migrate project --source-profile src --target-profile dst --source "OldProject" --create-target --dry-run` and show you what would be migrated without writing anything.

```text
Run the real migration for OldProject from source profile "src" to target profile "dst", but only migrate repos, wikis, and work items
```

→ `--phases repos,wikis,work_items`.

```text
The last migration failed halfway through — resume it from the state file
```

→ adds `--resume`.

```text
Skip the test_plans and dashboards phases and use 8 concurrent API calls
```

→ `--skip-phases test_plans,dashboards --concurrency 8`.

### Batch Migration

For multiple projects the agent expects a JSON manifest:

```bash
azdocli migrate batch --config manifest.json --dry-run
azdocli migrate batch --config manifest.json --resume --fail-fast
```

**Example prompts:**

```text
Migrate everything in manifest.json as a batch — dry run and stop on first error
```

```text
Resume the batch migration from manifest.json without failing fast
```

In all migration prompts the agent knows about `--fail-fast`, `--resume`, `--state-file`, `--output-dir`, and `-y/--yes` for skipping confirmations. It defaults to `--dry-run` unless you say "real migration" or "actually migrate".
