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
