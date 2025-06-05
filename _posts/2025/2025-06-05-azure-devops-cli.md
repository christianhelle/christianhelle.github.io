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
- **Simple authentication**: Supports both Personal Access Tokens (PAT) and Azure CLI authentication.
- **Rich command set**: Manage pipelines, builds, releases, work items, and more.
- **Easy scripting**: Designed for automation and CI/CD workflows.

## Getting Started

1. Download the latest release from the [GitHub releases page](https://github.com/christianhelle/azdocli/releases).
2. Make the binary executable (`chmod +x azdocli` on Unix).
3. Authenticate using your Azure DevOps PAT or via the Azure CLI.
4. Start running commands! For example:

    ```sh
    azdocli pipelines list --organization https://dev.azure.com/your-org --project YourProject
    ```

## Example Use Cases

- Quickly list and trigger pipelines
- Query and update work items
- Download build artifacts
- Automate release management

## Roadmap

I'm actively working on adding more features and improving the user experience. Contributions and feedback are welcome! Check out the [GitHub repository](https://github.com/christianhelle/azdocli) for more details, documentation, and to report issues.

---

If you're looking for a fast, reliable, and modern Azure DevOps CLI, give azdocli a try!