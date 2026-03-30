---
layout: post
title: From AI-Assisted Code Completion to Agentic Software Engineering
date: 2026-03-17
author: Christian Helle
tags:
  - AI
  - Copilot
  - Agents
  - Refitter
redirect_from:
  - /2026/03/09/from-ai-assisted-code-completion-to-agentic-engineering
  - /2026/03/09/from-ai-assisted-code-completion-to-agentic-engineering/
  - /2026/03/from-ai-assisted-code-completion-to-agentic-engineering
  - /2026/03/from-ai-assisted-code-completion-to-agentic-engineering/
  - /2026/from-ai-assisted-code-completion-to-agentic-engineering
  - /2026/from-ai-assisted-code-completion-to-agentic-engineering/
  - from-ai-assisted-code-completion-to-agentic-engineering
  - from-ai-assisted-code-completion-to-agentic-engineering/
---

Around four years ago, when [GitHub Copilot](https://github.com/features/copilot) was still in the early beta days, I tried AI-assisted code completion for the first time. Like most developers, I was equal parts skeptical and curious. The experience felt a little surreal. I would start writing a function, pause for a second, and the editor would continue the thought for me.

At first, it felt like a novelty. Then it became useful. Then it became part of my workflow. Somewhere along the way, Copilot stopped being something I occasionally experimented with and became a tool I use every single day. Today it is one of those tools I genuinely do not want to work without.

This post is about that journey: from AI-assisted code completion, to what now is a completely different mode of working altogether - agentic software engineering.

## From novelty to daily habit

The first value I got from Copilot was obvious: it removed repetition. It filled in boilerplate, guessed tests, completed common patterns, and saved me from typing the same kinds of code over and over again. That alone was enough to make it useful.

But what made it stick was not just speed. It was momentum.

When the tool is good, you stay in flow longer. You spend less time context switching, less time writing scaffolding, and more time thinking about the parts that actually matter. Over time, Copilot started helping me with much more than method bodies. It became useful for tests, documentation, scripts, CI workflows, refactorings, and all the little pieces of engineering work that surround the code itself.

That is when it stopped feeling like autocomplete and started feeling like a real development companion.

## Why I hated chat-driven development

There was, however, an awkward phase in the middle of all this: chat-driven development.

To be blunt, I hated it.

I thought it was an utter waste of time. The context switching was awful. I had to stop what I was doing, leave the editor mentally, explain the problem in a chat box, wait for a wall of text, copy pieces back into my code, discover that the result only half-fit the codebase, and then repair the damage manually. It felt like taking the most direct activity in software development - writing and changing code - and routing it through the slowest interface imaginable.

The results were often worse than the process. A lot of it felt verbose, vague, and overconfident. Chat-driven development had a habit of producing answers that looked finished from a distance and fell apart the moment they touched a real codebase. It was like programming by office memo.

At its worst, it felt like a workflow designed for people who wanted to describe software rather than build it. Too much prompting, too much pasting, too much ceremony, and far too little engineering. Instead of accelerating flow, it shattered flow. Instead of reducing cognitive load, it added another layer of it.

That is why I never saw chat-driven development as the future. For me, it was a clumsy transitional phase between code completion and something much more useful. The breakthrough was not "chat more." The breakthrough was getting to a point where the system could actually do meaningful work with the right context, constraints, and autonomy.

## Copilot Agent Mode with Claude Sonnet changed the game

For me, one of the biggest shifts happened when Claude Sonnet entered the picture. At first it was OK good, but far from impressive. Then it got better. Then it got much better. The jump from Claude Sonnet 3.5 to 4.0 was especially significant. It was both impressive and cost-effective. Suddenly, I can delegate large complex tasks to coding agents working in the background while I focus on something else. It can go and work by itself until the task is completed, no matter how long it took, and it would only cost me a single premium request.

The jump was hard to ignore. Prompts that previously produced a rough draft started producing something much closer to what I actually wanted. Multi-file edits became more coherent. Refactorings held together better. The model was much better at preserving intent across a larger body of work, which meant I could trust it with tasks that were broader than just completing the next few lines.

That was the moment where everything started improving rapidly.

## A tiny prompt can still go a long way

One of my favorite things about this new way of working is that even very small prompts can produce surprisingly meaningful results.

For example, I gave a very simple instruction to improve the output of [Refitter](https://github.com/christianhelle/refitter):

```text
Make the output of Refitter look more fancy
```

That is not a detailed specification. It is barely even a design brief. But it was enough to move the tool from a simple output format to something much more polished.

**Before:**

![Refitter simple output before the prompt](/assets/images/refitter-simple-output.png)

**After:**

![Refitter fancy output after the prompt](/assets/images/refitter-fancy-output.png)

This is a good example of why I no longer think about these tools only in terms of productivity. The real value is experimentation and leverage. A short prompt can unlock a useful improvement that would otherwise require a lot of manual iteration, especially when the agent has enough context to understand the existing codebase and the kind of result you are aiming for. It takes no time for the agent to come up with the solution and if I don't like it, I'll just throw it away. I can also go more specific into asking it to try it out multiple approaches to a problem using different models.

## From assistant to agent

AI-assisted development was already helpful, but it still often felt like a smart pair programmer that needed a lot of steering.
With Sonnet or Opus, the output started feeling more deliberate. The reasoning got better. The edits became more consistent. The amount of useful work I could delegate increased noticeably.

That change matters because it pushed the experience beyond code completion. Once the model became strong enough to reason through larger changes, the natural next step was to let it take on broader responsibilities.

That broader responsibility is what I now think of as agentic engineering.

Traditional AI-assisted coding starts with the line, the method, or maybe the file. Agentic engineering starts with intent. You describe the outcome you want, give the system enough context and constraints, and let it execute a meaningful chunk of work with some level of autonomy.

This is where GitHub Copilot feels fundamentally different today than it did in the early beta days. It is still great at helping me write code, but it can also help me plan work, break it down, coordinate it, and execute it across a much larger surface area.

## Running agents in parallel with /fleet

One of the features that makes this practical is [`/fleet` in GitHub Copilot CLI](https://docs.github.com/en/copilot/concepts/agents/copilot-cli/fleet).

`/fleet` takes a larger request or implementation plan, breaks it into smaller subtasks, and lets Copilot execute those subtasks in parallel when possible. The main agent acts as the orchestrator, manages dependencies, and delegates work to subagents. Each subagent gets its own context window, separate from the main agent and from the other subagents.

That separation is a big deal.

One giant chat thread is not a team. A team needs boundaries, focus, and clear handoffs. Parallel subagents with isolated context get much closer to that model than a single long-running conversation ever could.

Not every task benefits from parallelism, of course. Some work is inherently sequential. But when a problem can be decomposed into independent streams of work, `/fleet` is one of the most compelling tools in the current GitHub Copilot experience.

## Building a persistent team with Squad

That brings me to [Squad](https://github.com/christianhelle/blog/tree/master/.squad).

Squad gives you an AI development team through GitHub Copilot. You describe what you are building, and you get a team of specialists - frontend, backend, tester, lead - that live in your repo as files. They persist across sessions, learn your codebase, share decisions, and get better the more you use them.

It is not a chatbot wearing hats.

Each team member runs in its own context, reads only its own knowledge, and writes back what it learned. In practice, that means the team can have a roster, routing rules, agent charters, and shared decision logs inside the repository. The repo becomes the memory. The agents do not just respond in the moment - they accumulate working knowledge over time.

That is what makes the experience feel like a team rather than a clever prompt.

If I want a setup like this to work well, I have found that a few things matter:

1. Give each agent a clear specialty and boundaries.
2. Store team knowledge in the repository, not only in the chat transcript.
3. Use a lead or coordinator to route work and enforce handoffs.
4. Start downstream work early when it can happen independently.
5. Use `/fleet` when the plan can genuinely be parallelized.

When those pieces are in place, the interaction model changes completely. You stop thinking in terms of "help me write this file" and start thinking in terms of "here is the problem, here are the constraints, now go work as a team."

## Fixing Refitter issues hands-free with YOLO mode

The biggest example of this shift for me was when I put my agent team to work on [Refitter](https://github.com/christianhelle/refitter).

Refitter has gained quite a bit of traction over the past couple of years, and with that came a growing issue backlog. Like many maintainers, I had collected more issues than I realistically had time to work through myself. So instead of picking them off one by one, I gave the team a larger objective and let it run.

This was the exact prompt:

```text
Team, this project has gotten quite some traction over these past 2 years.
I have collected quite the number of issues that I simply don't have time to fix myself.
My issue list is publicly available on the Github repo: christianhelle/refitter.
Fix all the issues.
Apply fixes in individual feature branches and don't do all the work in a single branch
```

That one prompt resulted in a 29-hour session where 24 issues were resolved completely hands-free with the help of [`autopilot`](https://docs.github.com/en/copilot/concepts/agents/copilot-cli/autopilot) mode.

![Refitter agent fleet session resolving issues](/assets/images/refitter-agent-fleet.png)

What I find especially interesting is not just the number of issues resolved, but the operating model behind it. The instruction to use individual feature branches forced the work to be broken apart instead of collapsing into one giant change. Combined with parallel agent execution, that starts to look much less like code completion and much more like coordinated engineering work.

That would have been almost impossible to imagine when I first tried Copilot in the beta days. Back then, the magic was that it could finish a line or sketch out a function. Now I can hand a team of agents a backlog and constraints, let them work through it for more than a day, and come back to real progress.

## Where I am now

I still care deeply about writing code by hand. Code is the best and clearest way for me to express myself. I still love code completion and it is still the fastest interface between an idea in my head and code in my editor. GitHub Copilot remains a core part of my daily workflow for exactly that reason.

But the bigger story is what happened next.

The evolution from code completion to stronger reasoning, the arrival of models like Claude Opus, the ability to run work in parallel with `/fleet`, and the emergence of persistent repo-native agent teams like Squad have all pushed software development into a very different place.

I no longer think about AI as just something that helps me write code faster. I think about it as a system for delegating engineering work, coordinating specialists, and scaling execution without losing context.

That is why this feels like more than better autocomplete.

It feels like an entirely new form of software engineering. Agentic software engineering.

This shift has impacted how I build tools like [Refitter](/2023/03/refitter.html), [HTTP File Generator](/2023/11/http-file-generator.html), and the various [Zig](/2026/02/building-clocz-zig-line-counter.html) and [Rust](/2025/06/azure-devops-cli.html) projects on my [projects page](/projects/).
