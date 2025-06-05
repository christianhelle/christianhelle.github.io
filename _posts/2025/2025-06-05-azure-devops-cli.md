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

I'm excited to announce the release of my latest project: a blazing fast Azure DevOps CLI tool written in Rust! After years of using the official Azure DevOps CLI, I wanted something faster, more efficient, and easier to use. That's why I built [azdocli](https://github.com/christianhelle/azdocli).

## Why Rust?

To be honest it's mostly for the learning process, and also because I stumbled upon the an [Azure DevOps Rust API](https://github.com/microsoft/azure-devops-rust-api) which was code generated from OpenAPI specifications. Rust is known for its performance and safety. By leveraging Rust, [azdocli](https://github.com/christianhelle/azdocli) delivers near-instant command execution, low memory usage, and a single static binary with zero dependencies. This makes installation and updates a breeze, and ensures the tool runs reliably across platforms.

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
```    ```sh
    ado pipelines list --organization https://dev.azure.com/your-org --project YourProject
    ```

## Detailed Command Examples

### Default Project Management

One of the most convenient features is the ability to set a default project:

```sh
# Set a default project
ado project MyDefaultProject

# View the current default project
ado project

# All commands will now use the default project if --project is not specified
ado repos list                  # Uses default project
ado pipelines list              # Uses default project
ado repos list --project Other  # Overrides default with "Other"
```

### Repository Management

#### List and View Repositories

```sh
# List all repositories
ado repos list

# Show detailed repository information
ado repos show --id MyRepository
```

#### Clone Repositories

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

## Advanced Features

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

### Error Handling

The CLI provides comprehensive error handling with:

- Clear feedback when repositories, pipelines, or work items are not found
- Helpful suggestions when authentication fails
- Validation of required parameters before making API calls
- User-friendly formatting with emoji icons for better readability

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

## Roadmap

I'm actively working on adding more features and improving the user experience. The project follows modern development practices with:

- Continuous integration and security auditing
- Comprehensive test coverage including integration tests
- Clear contribution guidelines for community involvement
- Regular updates and feature enhancements

Contributions and feedback are welcome! Check out the [GitHub repository](https://github.com/christianhelle/azdocli) for more details, documentation, and to report issues.

---

If you're looking for a fast, reliable, and modern Azure DevOps CLI that can significantly speed up your development workflow, give [azdocli](https://github.com/christianhelle/azdocli) a try! The combination of Rust's performance, comprehensive feature set, and modern CLI design makes it a powerful tool for any Azure DevOps user.