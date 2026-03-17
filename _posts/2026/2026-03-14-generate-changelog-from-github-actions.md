---
layout: post
title: Generate a Changelog from GitHub Actions
date: 2026-03-14
author: Christian Helle
tags:
  - GitHub Actions
  - Changelog
  - CI/CD
  - Tools
  - Automation
redirect_from:
  - /2026/03/17/generate-changelog-from-github-actions
  - /2026/03/17/generate-changelog-from-github-actions/
  - /2026/03/generate-changelog-from-github-actions
  - /2026/03/generate-changelog-from-github-actions/
  - /2026/generate-changelog-from-github-actions
  - /2026/generate-changelog-from-github-actions/
  - /generate-changelog-from-github-actions
  - /generate-changelog-from-github-actions/
---

I recently built a GitHub Action that automatically generates changelogs from repository releases and merged pull requests. The action wraps [chlogr](https://github.com/christianhelle/chlogr), a fast changelog generator written in Zig that I created to simplify keeping changelogs up-to-date in my projects.

The [Changelog Generator Action](https://github.com/marketplace/actions/generate-changelog-with-chlogr) is published to the GitHub Marketplace and can generate changelogs for any repository, not just the one where the workflow runs. This makes it useful for monorepos, documentation sites, or projects that need to aggregate changelogs from multiple sources.

I built this because manually maintaining `CHANGELOG.md` files is tedious and error-prone. Tools that generate changelogs from commit messages often produce noisy output with implementation details rather than user-facing changes. By generating changelogs from GitHub releases and merged pull requests, the tool produces cleaner, more meaningful changelogs that focus on what matters to users.

## What is chlogr?

[chlogr](https://github.com/christianhelle/chlogr) is the underlying CLI tool that powers the GitHub Action. It's written in Zig, compiles to a single native binary with zero dependencies, and is designed to be fast and simple.

The tool fetches GitHub releases and merged pull requests from the GitHub API, categorizes them by labels, and generates a Markdown changelog. It supports filtering by tag ranges, excluding specific labels, and handles GitHub authentication through multiple methods.

Key features include:

- **Fast and lightweight**: Native binary, pure Zig standard library, no external dependencies
- **Smart categorization**: Groups changes by labels (Features, Bug Fixes, Other)
- **Automatic linking**: Generates links to PRs, issues, and contributors
- **Flexible authentication**: Supports `--token` flag, environment variables, or `gh` CLI
- **Tag range filtering**: Generate changelogs for specific release windows
- **Cross-platform**: Works on Linux, macOS, and Windows

The tool is available on GitHub at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

## What is the Changelog Generator Action?

The [Changelog Generator Action](https://github.com/marketplace/actions/generate-changelog-with-chlogr) is a reusable GitHub Action that wraps chlogr for use in CI/CD workflows. It handles downloading the appropriate chlogr binary for the runner platform, passing credentials, and generating the changelog file.

The action provides a clean interface with inputs for repository selection, output file path, tag filtering, label exclusion, and version pinning. It outputs the path to the generated file and a flag indicating whether the content changed, making it easy to conditionally commit the changelog.

Since it's published to the GitHub Marketplace, you can reference it directly in your workflows without downloading or compiling anything.

## Basic Usage

The simplest use case is generating a `CHANGELOG.md` file for the current repository. This workflow runs on every push to the main branch and on manual dispatch:

```yaml
name: Changelog

on:
  workflow_dispatch:
  push:
    branches:
      - main

permissions:
  contents: write
  pull-requests: read
  issues: read

jobs:
  changelog:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Generate changelog
        id: changelog
        uses: christianhelle/changelog-generator-action@v1
        with:
          output: CHANGELOG.md

      - name: Commit changelog
        if: steps.changelog.outputs.changed == 'true'
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
          git add CHANGELOG.md
          git commit -m "Update changelog [skip ci]"
          git push
```

This workflow demonstrates the complete lifecycle: checkout, generate, and commit. The action uses the default `${{ github.token }}` for authentication and the current repository (`${{ github.repository }}`) as the target.

### Understanding the Permissions

The `permissions` block is critical. The workflow needs:

- **`contents: write`** - To commit the generated changelog back to the repository
- **`pull-requests: read`** - To fetch merged pull request data from the GitHub API
- **`issues: read`** - To access issue metadata referenced in pull requests

If you're only generating the changelog without committing it (for example, uploading it as an artifact), you can remove the `write` permission.

### Conditional Commit with the `changed` Output

The action outputs a `changed` flag that indicates whether the generated changelog differs from the existing file. This prevents empty commits when nothing has changed:

```yaml
- name: Commit changelog
  if: steps.changelog.outputs.changed == 'true'
  run: |
    git add CHANGELOG.md
    git commit -m "Update changelog [skip ci]"
    git push
```

The `[skip ci]` suffix in the commit message prevents triggering the workflow again, avoiding an infinite loop.

## Advanced Usage Examples

The action supports several advanced scenarios through its inputs. Let me walk through the most useful patterns.

### Generating Changelog for a Different Repository

You can generate changelogs for any repository, not just the one where the workflow runs. This is useful for documentation sites or aggregation workflows:

```yaml
- name: Generate changelog for Refitter
  uses: christianhelle/changelog-generator-action@v1
  with:
    repo: christianhelle/refitter
    output: artifacts/REFITTER_CHANGELOG.md
```

If the target repository is private, you'll need to provide a Personal Access Token (PAT) with appropriate permissions:

```yaml
- name: Generate changelog for private repo
  uses: christianhelle/changelog-generator-action@v1
  with:
    repo: myorg/private-repo
    github-token: ${{ secrets.PAT_WITH_REPO_ACCESS }}
    output: CHANGELOG.md
```

### Filtering by Tag Range

You can limit the changelog to a specific release window using `since-tag` and `until-tag`:

```yaml
- name: Generate changelog for v1.10.0 to v1.11.0
  uses: christianhelle/changelog-generator-action@v1
  with:
    since-tag: v1.10.0
    until-tag: v1.11.0
    output: CHANGELOG-1.11.0.md
```

This is particularly useful when generating release-specific changelogs or when you want to document changes between two specific versions.

### Excluding Labels

Some pull requests shouldn't appear in the changelog—duplicates, won't-fix issues, or internal refactoring. Exclude them by label:

```yaml
- name: Generate changelog without noise
  uses: christianhelle/changelog-generator-action@v1
  with:
    exclude-labels: duplicate,wontfix,internal,dependencies
    output: CHANGELOG.md
```

Any pull request tagged with these labels will be filtered out of the generated changelog.

### Pinning chlogr Version

For deterministic builds, pin the chlogr version rather than using `latest`:

```yaml
- name: Generate changelog with pinned version
  uses: christianhelle/changelog-generator-action@v1
  with:
    chlogr-version: 0.1.2
    output: CHANGELOG.md
```

This ensures the changelog format and behavior remain consistent across workflow runs, even when new chlogr versions are released.

### Multiple Changelogs in One Workflow

You can generate multiple changelogs in a single workflow by calling the action multiple times:

```yaml
- name: Generate main project changelog
  uses: christianhelle/changelog-generator-action@v1
  with:
    output: CHANGELOG.md

- name: Generate changelog for last release only
  uses: christianhelle/changelog-generator-action@v1
  with:
    since-tag: v1.0.0
    output: CHANGELOG-LATEST.md
```

This is useful for creating both a complete historical changelog and a focused release-specific version.

## Real-World Example: Argiope

I use this action in my [Argiope](https://github.com/christianhelle/argiope) project, a web crawler for broken link detection. The workflow is simple but effective:

```yaml
name: Changelog

on:
  workflow_dispatch:
  push:
    branches:
      - main

permissions:
  contents: write
  pull-requests: read
  issues: read

jobs:
  changelog:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Generate changelog
        id: changelog
        uses: christianhelle/changelog-generator-action@v1
        with:
          output: CHANGELOG.md

      - name: Commit changelog
        if: steps.changelog.outputs.changed == 'true'
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
          git add CHANGELOG.md
          git commit -m "Update changelog [skip ci]"
          git push
```

This workflow runs automatically on every push to main and can be manually triggered via `workflow_dispatch`. It keeps the changelog synchronized with the latest releases without any manual intervention.

The benefits for Argiope include:

- **No manual maintenance**: Changelog updates automatically when I create releases
- **Clean git history**: The changelog commit uses the GitHub Actions bot identity
- **No infinite loops**: The `[skip ci]` commit message prevents re-triggering
- **Always accurate**: The changelog reflects actual releases and merged PRs, not commit messages

## Integration Patterns

Here are some common patterns for integrating changelog generation into your CI/CD workflows.

### Generate Changelog on Release

Trigger changelog generation when you create a new release:

```yaml
name: Release Changelog

on:
  release:
    types: [published]

permissions:
  contents: write
  pull-requests: read
  issues: read

jobs:
  changelog:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Generate changelog
        uses: christianhelle/changelog-generator-action@v1
        with:
          output: CHANGELOG.md

      - name: Commit and push
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
          git add CHANGELOG.md
          git commit -m "Update changelog for ${{ github.event.release.tag_name }} [skip ci]"
          git push
```

This ensures the changelog is updated immediately after every release.

### Upload Changelog as Release Asset

Instead of committing the changelog, attach it to the release:

```yaml
- name: Generate changelog
  uses: christianhelle/changelog-generator-action@v1
  with:
    output: CHANGELOG.md

- name: Upload changelog to release
  uses: softprops/action-gh-release@v1
  with:
    files: CHANGELOG.md
```

This makes the changelog available as a downloadable artifact on the release page.

### Scheduled Changelog Updates

Run changelog generation on a schedule to keep it up-to-date even without explicit releases:

```yaml
on:
  schedule:
    - cron: "0 0 * * 0" # Weekly on Sunday at midnight
  workflow_dispatch:

jobs:
  changelog:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Generate changelog
        uses: christianhelle/changelog-generator-action@v1
        with:
          output: CHANGELOG.md

      - name: Create pull request
        uses: peter-evans/create-pull-request@v5
        with:
          commit-message: Update changelog
          title: "chore: update changelog"
          branch: changelog-update
```

This creates a pull request with the updated changelog rather than committing directly, allowing you to review changes before merging.

### Multi-Platform Matrix Build

The action supports all major platforms and architectures:

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, macos-latest, windows-latest]

runs-on: ${{ matrix.os }}

steps:
  - uses: actions/checkout@v4

  - name: Generate changelog
    uses: christianhelle/changelog-generator-action@v1
    with:
      output: CHANGELOG.md
```

The action automatically detects the runner platform and downloads the appropriate chlogr binary (Linux x64/arm64, macOS x64/arm64, Windows x64).

## Features and Capabilities

Let me dive deeper into how the action categorizes and formats changelog entries.

### Label Categorization

The action groups changelog entries into three categories based on pull request labels:

- **Features**: PRs with labels like `enhancement`, `feature`, or `new-feature`
- **Bug Fixes**: PRs with labels like `bug`, `bugfix`, or `fix`
- **Other**: Everything else

This produces clean, scannable changelog sections:

```markdown
# Changelog

## [v1.2.0](https://github.com/owner/repo/releases/tag/v1.2.0) - 2024-01-15

### Features

- Add new feature X (#123) (@alice)

### Bug Fixes

- Fix critical bug (#124) (@bob)

### Other

- Update documentation (#125) (@charlie)
```

The categorization happens automatically based on the labels assigned to pull requests. For best results, establish a labeling convention in your repository.

### Link Generation

The generated changelog includes links to:

- **Release tags**: Each version header links to the GitHub release page
- **Pull requests**: PR numbers link to the PR page
- **Contributors**: Author usernames link to their GitHub profiles

This makes the changelog a navigable reference rather than just a text document.

### Token Resolution Strategies

The action handles GitHub authentication through a flexible fallback chain. When you don't specify `github-token`, it uses the default workflow token (`${{ github.token }}`). When generating changelogs for the current repository, this is usually sufficient.

For cross-repository changelog generation, the action respects the token you provide:

```yaml
- name: Generate changelog
  uses: christianhelle/changelog-generator-action@v1
  with:
    repo: otherorg/otherrepo
    github-token: ${{ secrets.CROSS_REPO_PAT }}
```

The underlying chlogr tool has even more fallback options (environment variables, `gh` CLI), but the action normalizes this by passing the token explicitly.

### Output File Handling

The action handles both absolute and relative paths for the output file. Relative paths are resolved from the `GITHUB_WORKSPACE` directory:

```yaml
# Writes to $GITHUB_WORKSPACE/CHANGELOG.md
- uses: christianhelle/changelog-generator-action@v1
  with:
    output: CHANGELOG.md

# Writes to $GITHUB_WORKSPACE/docs/CHANGELOG.md
- uses: christianhelle/changelog-generator-action@v1
  with:
    output: docs/CHANGELOG.md

# Writes to an absolute path
- uses: christianhelle/changelog-generator-action@v1
  with:
    output: /tmp/CHANGELOG.md
```

The action outputs the absolute path via `changelog-path`, which you can reference in subsequent steps:

```yaml
- name: Generate changelog
  id: gen
  uses: christianhelle/changelog-generator-action@v1

- name: Print path
  run: echo "Changelog written to ${{ steps.gen.outputs.changelog-path }}"
```

## Troubleshooting and Tips

Here are some common issues and solutions when using the action.

### Token Authentication Errors

If you see `401 Unauthorized` or `403 Forbidden` errors, check your permissions:

```yaml
permissions:
  contents: read
  pull-requests: read
  issues: read
```

For private repositories or cross-repository access, use a PAT:

```yaml
- uses: christianhelle/changelog-generator-action@v1
  with:
    github-token: ${{ secrets.PAT_WITH_REPO_SCOPE }}
```

The PAT needs the `repo` scope for private repositories or `public_repo` for public ones.

### Rate Limiting

The GitHub API has rate limits (5,000 requests/hour for authenticated requests, 60/hour for unauthenticated). The action uses authenticated requests via the workflow token, so rate limiting is rarely an issue.

If you do hit rate limits (typically in very large repositories or when running many workflows simultaneously), you'll see a `429 Too Many Requests` error. The solution is to wait and retry, or reduce the frequency of changelog generation.

### Empty or Missing Changelogs

If the generated changelog is empty or missing expected entries, verify:

1. **The repository has GitHub releases**: The tool fetches releases via the GitHub Releases API, not just tags
2. **Pull requests are merged**: Only merged PRs appear in the changelog
3. **Tag filtering is correct**: If using `since-tag` or `until-tag`, ensure the tags exist

You can test the underlying chlogr tool locally to debug issues:

```bash
# Download chlogr
curl -L https://github.com/christianhelle/chlogr/releases/latest/download/chlogr-linux-x86_64 -o chlogr
chmod +x chlogr

# Generate changelog
./chlogr --repo owner/repo --token $GITHUB_TOKEN --output CHANGELOG.md
```

### Platform and Architecture Support

The action supports:

- **Linux**: x64, arm64
- **macOS**: x64, arm64
- **Windows**: x64

If you use a runner platform that doesn't match these, the action will fail with a clear error message. Self-hosted runners should use one of these supported platforms.

### Permissions for Committing

If your workflow commits the changelog and you see errors like `unable to access`, ensure:

1. The workflow has `contents: write` permission
2. Your repository settings allow GitHub Actions to create and approve pull requests (Settings → Actions → General → Workflow permissions)

For repositories with branch protection rules, you may need to use a PAT with elevated permissions or create a pull request instead of committing directly.

### Version Pinning Best Practices

For production workflows, pin both the action version and the chlogr version:

```yaml
- uses: christianhelle/changelog-generator-action@v1.0.3
  with:
    chlogr-version: 0.1.2
```

This prevents unexpected changes when either the action or chlogr is updated. Use Dependabot or Renovate to keep versions up-to-date.

## Conclusion

Automating changelog generation from GitHub Actions eliminates manual maintenance while producing cleaner, more meaningful changelogs than commit-based approaches. The Changelog Generator Action makes this easy to integrate into any workflow, whether you're generating changelogs for the current repository, multiple repositories, or specific release windows.

The action is published to the GitHub Marketplace at [https://github.com/marketplace/actions/generate-changelog-with-chlogr](https://github.com/marketplace/actions/generate-changelog-with-chlogr), and the underlying chlogr tool is available at [https://github.com/christianhelle/chlogr](https://github.com/christianhelle/chlogr).

If you're looking for a simple way to keep your changelogs up-to-date, give it a try. It takes just a few lines of YAML to set up and runs in seconds.
