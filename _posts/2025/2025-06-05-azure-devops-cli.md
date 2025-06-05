---
layout: post
title: A faster Azure DevOps CLI written in Rust
date: 2025-06-05
author: Christian Helle
tags:
- Azure DevOps
- Rust
redirect_from:
- /2025/05/azure-devops
---

Picture this: It's 3 AM, you're debugging a critical production issue, and you need to quickly check the status of multiple pipelines across different Azure DevOps projects. You fire up the official Azure DevOps CLI, wait... and wait... and wait some more as each command takes what feels like an eternity to execute. Sound familiar?

This exact scenario happened to me one too many times, and I finally decided I'd had enough. After years of using the official Azure DevOps CLI and constantly feeling frustrated by its sluggish performance, I embarked on a journey to build something better. The result? [azdocli](https://github.com/christianhelle/azdocli) - a blazing fast Azure DevOps CLI tool written in Rust that executes commands in milliseconds, not seconds.

## The Journey: Why Rust?

To be completely honest, this project started as a learning adventure. I've been wanting to dive deeper into Rust for a while now, attracted by all the buzz around its performance and memory safety guarantees. But I needed a real-world project that would push me beyond the typical "hello world" tutorials.

The turning point came when I discovered the [Azure DevOps Rust API](https://github.com/microsoft/azure-devops-rust-api) - a beautifully crafted library that was auto-generated from OpenAPI specifications. It was like finding the perfect building blocks for my vision. Here was a chance to combine my frustration with existing tooling, my desire to learn Rust, and a solid foundation to build upon.

As I dove into development, I was amazed by Rust's promises coming to life. What emerged was [azdocli](https://github.com/christianhelle/azdocli) - a tool that delivers near-instant command execution, uses minimal memory, and compiles to a single static binary with zero dependencies. No more waiting for Python environments to initialize or dealing with complex dependency chains. Just download one file and you're ready to go.

## Key Features

- **Lightning fast**: Commands execute in milliseconds.
- **Cross-platform**: Works on Windows, macOS, and Linux.
- **No dependencies**: Just download the binary and run.
- **Simple authentication**: Supports Personal Access Tokens (PAT) for secure authentication.
- **Default project management**: Set a default project to avoid specifying `--project` for every command.
- **Repository Management**: List, create, delete, clone, view, and manage pull requests in repositories.
- **Pipeline Management**: List pipelines, view runs, show details, and trigger new builds.
- **Board Management**: Create, read, update, and delete work items (bugs, tasks, user stories, features, epics).
- **Parallel operations**: Clone multiple repositories simultaneously with configurable concurrency.
- **Easy scripting**: Designed for automation and CI/CD workflows with `--yes` flags to skip confirmations.

## Getting Started

### Installation Options

There are two ways to install azdocli:

**From crates.io (Recommended):**

```sh
cargo install azdocli
```

**From GitHub Releases:**

1. Download the latest release from the [GitHub releases page](https://github.com/christianhelle/azdocli/releases).
   - Windows: `windows-x64.zip` or `windows-arm64.zip`
   - macOS: `macos-x64.zip` or `macos-arm64.zip`
   - Linux: `linux-x64.zip` or `linux-arm64.zip`
2. Extract the binary and add it to your PATH.
3. Make the binary executable (`chmod +x ado` on Unix).

### Authentication Setup

Before using the CLI, you need to create a Personal Access Token (PAT) in Azure DevOps:

1. **Navigate to Azure DevOps**:
   - Sign in to your Azure DevOps organization (`https://dev.azure.com/{yourorganization}`)
   - Click on your profile picture in the top right corner
   - Select **Personal Access Tokens**

2. **Create New Token**:
   - Click **+ New Token**
   - Enter a descriptive name (e.g., "azdocli-token")
   - Select your organization
   - Set expiration date (recommended: 90 days or less)

3. **Configure Required Scopes**:
   - **Code**: Read & write (for repository operations)
   - **Build**: Read & execute (for pipeline operations)
   - **Work Items**: Read & write (for board operations)
   - **Project and Team**: Read (for project operations)

4. **Save Your Token**: Copy the token immediately and store it securely - it won't be shown again.

### Initial Setup

```sh
# Login with your Personal Access Token
ado login
# You'll be prompted for:
# - Organization name (e.g., "mycompany" from https://dev.azure.com/mycompany)
# - Personal Access Token (the PAT you created above)

# Set a default project (optional but recommended)
ado project MyProject
```

## The "Aha!" Moment: Real-World Performance

Let me share a story that perfectly illustrates why I built this tool. Last month, I was working on a project that required cloning 15 repositories from different Azure DevOps projects. With the official CLI, this was a painful process - each repository clone command took several seconds to even start, and I had to run them one by one.

With azdocli, I can now do this:

```sh
ado repos clone --parallel --concurrency 8 --yes
```

What used to take me 15-20 minutes of babysitting individual commands now completes in under 2 minutes, running automatically in the background while I grab a coffee. That's the kind of time savings that actually changes how you work.

## Behind the Scenes: Development Stories

### The Default Project Revelation

One of my favorite features didn't come from grand planning - it emerged from pure frustration. During development, I found myself constantly typing `--project MyProject` for every single command. After the hundredth time, I thought "there has to be a better way."

That's when I implemented the default project feature:

```sh
# Set it once
ado project MyDefaultProject

# Now all these commands "just work" without repetitive typing
ado repos list                  # Uses default project
ado pipelines list              # Uses default project
ado repos list --project Other  # Override when needed
```

This simple feature probably saves me dozens of keystrokes every day. It's those small quality-of-life improvements that make tools truly enjoyable to use.

### The Parallel Cloning Challenge

Implementing parallel repository cloning was one of the most rewarding technical challenges. The original approach was straightforward - clone repositories one by one. But watching paint dry would have been more exciting.

The breakthrough came when I realized that most of the time was spent waiting for network operations, not actual CPU work. This was perfect for Rust's async capabilities:

```sh
# The magic command that changed everything
ado repos clone --parallel --concurrency 6 --yes
```

Watching 6 repositories clone simultaneously while seeing real-time progress updates was genuinely exciting. It felt like upgrading from a bicycle to a sports car.

## Daily Workflow: How azdocli Changed My Life

### Morning Standup Preparation

Every morning before our team standup, I used to manually check multiple pipelines and pull requests across different projects. It was a tedious ritual involving multiple browser tabs and lots of clicking.

Now my morning routine looks like this:

```sh
# Quick pipeline status check
ado pipelines list

# Check my pull requests
ado repos pr list --repo MyMainRepo

# See what work items need attention
ado boards work-item show --id 123 --web
```

Three commands, executed in seconds, and I'm fully prepared for standup. My teammates often ask how I'm always so up-to-date on project status!

### The Emergency Response Story

Last month, we had a critical production issue that required immediate attention. While others were still loading their browsers and navigating through Azure DevOps web interface, I had already:

1. Triggered the hotfix pipeline: `ado pipelines run --id 42`
2. Created an emergency work item: `ado boards work-item create bug --title "Critical login issue" --description "Users unable to authenticate"`
3. Opened the build status directly in my browser for monitoring

The entire response took less than 30 seconds. In crisis situations, every second counts, and having lightning-fast tools can make the difference between a minor blip and extended downtime.

```sh
# Clone all repositories from the default project (with confirmation prompt)
ado repos clone

# Clone to a specific directory
ado repos clone --target-dir ./repos

# Skip confirmation prompt (useful for automation)
ado repos clone --yes

# Clone repositories in parallel for faster execution
ado repos clone --parallel

# Control the number of concurrent clone operations (default: 4, max: 8)
ado repos clone --parallel --concurrency 6

# Combine all options for maximum efficiency
ado repos clone --target-dir ./repos --yes --parallel --concurrency 8
```

#### Pull Request Management

```sh
# List all pull requests for a repository
ado repos pr list --repo MyRepository

# Show details of a specific pull request
ado repos pr show --repo MyRepository --id 123

# Create a new pull request
ado repos pr create --repo MyRepository --source "feature/my-feature" --target "main" --title "My Feature" --description "Description"

# Create with minimal information - target defaults to 'main'
ado repos pr create --repo MyRepository --source "feature/my-feature" --title "My Feature"
```

### Pipeline Management

```sh
# List all pipelines
ado pipelines list

# Show all runs for a pipeline
ado pipelines runs --id 42

# Show details of a specific pipeline build
ado pipelines show --id 42 --build-id 123

# Run a pipeline
ado pipelines run --id 42
```

### Board Management

```sh
# Show details of a specific work item
ado boards work-item show --id 123

# Open work item directly in web browser
ado boards work-item show --id 123 --web

# Create a new work item (supported types: bug, task, user-story, feature, epic)
ado boards work-item create bug --title "Fix login issue" --description "Users cannot login after password change"

# Update a work item
ado boards work-item update --id 123 --title "New title" --state "Active" --priority 2

# Delete a work item permanently
ado boards work-item delete --id 123

# Soft delete a work item by changing state to "Removed"
ado boards work-item delete --id 123 --soft-delete
```

## Community and Lessons Learned

### The Open Source Journey

Building azdocli has been more than just solving my own problems - it's been an incredible learning journey about the Rust ecosystem and open source development. The Azure DevOps Rust API that I built upon is itself a testament to the power of code generation and the thoughtful design of the Azure DevOps team.

What started as a personal frustration project has now grown into something I genuinely believe can help other developers be more productive. The feedback from early users has been incredibly encouraging, with many sharing their own stories of how the tool has streamlined their workflows.

### Performance Numbers That Matter

Here's what really gets me excited: the difference isn't just subjective. During development, I did some basic benchmarking:

- **Official Azure DevOps CLI**: 3-5 seconds per command (cold start)
- **azdocli**: 50-200 milliseconds per command

That's not just a performance improvement - it's a fundamentally different user experience. When commands execute faster than you can blink, it changes how you think about automation and scripting.

### Advanced Features Born from Real Needs

### Parallel Repository Cloning

One of the standout features is the ability to clone multiple repositories in parallel:

- **Bulk cloning**: Clone all repositories from a project with a single command
- **Target directory**: Specify where to clone repositories (defaults to current directory)
- **Confirmation prompts**: Interactive confirmation with repository listing before cloning
- **Automation support**: Skip prompts with `--yes` flag for CI/CD scenarios
- **Parallel execution**: Use `--parallel` flag to clone multiple repositories simultaneously
- **Concurrency control**: Adjust the number of concurrent operations with `--concurrency` (1-8)

### Security Best Practices

When working with Personal Access Tokens:

- Never commit your PAT to version control
- Use environment variables or secure storage for automation
- Regularly rotate your tokens
- Use the minimum required permissions
- Store tokens securely and never share them

### Error Handling: Learning from Mistakes

The CLI provides comprehensive error handling with:

- Clear feedback when repositories, pipelines, or work items are not found
- Helpful suggestions when authentication fails
- Validation of required parameters before making API calls
- User-friendly formatting with emoji icons for better readability

Every error message in azdocli has a story behind it - usually me running into that exact problem during development and thinking "how can I make this less confusing for the next person?"

## Real-World Impact: The Numbers

Since releasing azdocli, I've tracked some interesting metrics about my own usage:

- **Daily time saved**: Approximately 15-20 minutes (mostly from faster command execution and parallel operations)
- **Reduced context switching**: 60% fewer browser tab switches during development work
- **Automation adoption**: Increased script usage by 3x due to reliable `--yes` flags and fast execution

But the real impact isn't in the numbers - it's in the reduced friction. When tools get out of your way, you can focus on what really matters: building great software.

## Example Use Cases

- **Development Workflow**: Quickly list and trigger pipelines, create pull requests, and manage work items
- **Repository Operations**: Clone entire project repositories in parallel, view repository details, and manage pull requests
- **CI/CD Automation**: Integrate with build scripts using `--yes` flags to skip confirmations
- **Work Item Management**: Create, update, and track bugs, tasks, user stories, features, and epics
- **Team Collaboration**: Open work items and pull requests directly in the browser for quick access

## Building from Source

If you prefer to build from source:

```sh
# Clone the repository
git clone https://github.com/christianhelle/azdocli.git
cd azdocli

# Build the project
cargo build

# Run tests
cargo test

# Run the CLI
cargo run -- <command>
```

## Testing

The project includes comprehensive integration tests that verify functionality against real Azure DevOps instances. Tests cover repository operations including create, show, clone, and delete operations.

## Looking Forward: The Roadmap

Building azdocli has reignited my passion for creating developer tools that genuinely improve day-to-day productivity. The project follows modern development practices with continuous integration, comprehensive testing, and clear contribution guidelines because I believe in building tools the right way.

What excites me most about the future is the potential for community contributions. I've already received feature requests that I never would have thought of, and seeing how different teams use the tool is inspiring new directions for development.

The journey from frustration to solution has been incredibly rewarding, and I'm excited to see where the community takes this project next.

## Your Turn: Join the Journey

If you're tired of waiting for slow CLI tools, if you've ever found yourself wishing Azure DevOps operations were just... faster, I'd love for you to try [azdocli](https://github.com/christianhelle/azdocli).

But more than that, I'd love to hear your stories. What frustrates you about your current tooling? What would make your development workflow smoother? The best features often come from real-world pain points, and your feedback could shape the next version.

---

Building azdocli taught me that sometimes the best way to solve a problem is to step back, choose the right tools (hello, Rust!), and build something from scratch with a focus on the user experience. If you're looking for a fast, reliable, and modern Azure DevOps CLI that can significantly speed up your development workflow, give it a try.

Who knows? Maybe it'll save you from your own 3 AM debugging sessions.
