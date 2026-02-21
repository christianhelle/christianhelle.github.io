---
layout: post
title: "From Zig to Rust: Building httprunner"
date: 2026-02-21
author: Christian Helle
tags: [Rust, HTTP, CLI, TUI, GUI, Open Source, DevOps]
---

I recently released `httprunner`, a comprehensive HTTP client toolset that includes a powerful Command Line Interface (CLI), a Terminal User Interface (TUI), and a native Graphical User Interface (GUI). The project parses and executes standard `.http` files, providing a developer-friendly way to test and interact with APIs.

What’s interesting about `httprunner` isn't just what it does, but how it came to be. It started as a small coding exercise in [Zig](https://ziglang.org/)—a language I was keen to learn. My goal was simple: build a basic parser for the `.http` file format popularized by IntelliJ IDEA and VS Code. As I got the basics working, I found myself wanting more robust libraries and tooling, which led me to port the codebase to [Rust](https://www.rust-lang.org/).

The transition to Rust was a turning point. The rich ecosystem of crates, the strict compiler guarantees, and the excellent tooling allowed me to expand the project rapidly. What began as a weekend learning experiment has now evolved into a tool I use daily for building, testing, and debugging backend systems. It’s become an integral part of my workflow, replacing ad-hoc `curl` scripts and heavy UI-based clients.

Here is a deep dive into what `httprunner` can do.

## The .http File Format

At its core, `httprunner` is built around the `.http` file format. This format is simple, text-based, and version-controllable, making it ideal for storing API requests alongside your code.

You can define multiple requests in a single file by separating them with `###`. Comments start with `#`.

```http
# A simple GET request
GET https://httpbin.org/get
User-Agent: httprunner/1.0

###

# A POST request with a JSON body
POST https://httpbin.org/post
Content-Type: application/json

{
    "name": "httprunner",
    "language": "Rust"
}
```

## Built-in Functions

One of the most powerful features is the ability to inline dynamic values using built-in functions. This is essential for avoiding static data in your tests and simulating real-world scenarios.

You can use functions directly in your request body or headers:

```http
POST https://api.example.com/users
Content-Type: application/json

{
    "id": "{{guid}}",
    "email": "{{email}}",
    "name": "{{name}}",
    "created_at": "{{getdatetime}}",
    "nonce": "{{random_int(1000, 9999)}}"
}
```

`httprunner` supports a wide array of functions:

*   **Generators**: `guid`, `string`, `number`, `boolean`
*   **Transformation**: `base64_encode`, `upper`, `lower`
*   **Fake Data**: `name`, `first_name`, `last_name`, `email`, `address`, `job_title`, `lorem_ipsum`
*   **Date & Time**: `date`, `time`, `getdatetime`, `getutcdatetime`

## Variable Support

Hardcoding values is a bad practice. `httprunner` offers robust variable support to keep your requests flexible and reusable.

### Inline Variables
You can define variables directly in the `.http` file using the `@name = value` syntax:

```http
@hostname = api.example.com
@port = 8080
@protocol = https

GET {{protocol}}://{{hostname}}:{{port}}/health
```

### Environment Files
For managing different environments (e.g., local, dev, prod), `httprunner` supports `http-client.env.json` files. This allows you to define sets of variables that can be selected at runtime.

**http-client.env.json:**
```json
{
  "dev": {
    "hostname": "localhost",
    "port": 8080
  },
  "prod": {
    "hostname": "api.example.com",
    "port": 443
  }
}
```

## Response Assertion

Automated testing is built right in. You can assert the status code, headers, and body content of a response. If an assertion fails, `httprunner` will report it as a failure (red output).

```http
GET https://httpbin.org/json

# Assert the status code is 200
EXPECTED_RESPONSE_STATUS 200

# Assert a specific header is present
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"

# Assert the body contains a specific string
EXPECTED_RESPONSE_BODY "slideshow"

# Assert a JSON field value using JSONPath
EXPECTED_RESPONSE_BODY $.slideshow.author "Yours Truly"
```

## Request Chaining

Complex workflows often require chaining requests—using the output of one request as the input for another. `httprunner` makes this easy with named requests and response referencing.

```http
# @name login
POST https://api.example.com/login
Content-Type: application/json

{
    "username": "user",
    "password": "password"
}

###

# Use the token from the login response
GET https://api.example.com/profile
Authorization: Bearer {{login.response.body.$.token}}
X-User-Id: {{login.response.body.$.user.id}}
```

You can reference:
*   `{{request.response.body}}` (full body)
*   `{{request.response.body.$.path}}` (JSONPath)
*   `{{request.response.headers.Header-Name}}`
*   `{{request.response.status}}`

## Conditional Request Execution

You can control the execution flow of your requests using `@dependsOn` and conditional checks. This is perfect for integration tests where a cleanup step should only run if the creation step succeeded.

```http
# @name create_user
POST https://api.example.com/users
...

# Only run if create_user succeeded (returned 2xx)
# @dependsOn create_user
GET https://api.example.com/users/{{create_user.response.body.$.id}}

###

# Only run if the user role is admin
# @if login.response.body.$.role == "admin"
DELETE https://api.example.com/users/{{create_user.response.body.$.id}}
```

## Request Delays and Timeouts

To simulate user behavior or respect API rate limits, you can introduce delays and configure timeouts.

```http
# Wait 500ms before this request
# @pre-delay 500
GET https://httpbin.org/delay/2

# Wait 1000ms after this request completes
# @post-delay 1000
GET https://httpbin.org/get

# Set a custom timeout for this request
# @timeout 30s
GET https://httpbin.org/delay/10
```

## Reporting, Logging, and Telemetry

`httprunner` is designed to be observable and CI/CD friendly.

*   **Verbose Mode**: The `--verbose` flag prints detailed request and response information, including headers and bodies. The `--pretty-json` flag makes JSON output readable.
*   **Logging**: Use `--log` to stream all output to a file for archival or analysis.
*   **Report Generation**: The `--report` flag generates a summary of the run in Markdown (default) or HTML format, perfect for attaching to build artifacts.
*   **Telemetry**: The tool collects anonymous usage data via Azure Application Insights to help improve the tool. Privacy is paramount—no request content, URLs, file paths, or environment values are ever collected. You can opt-out by setting `DO_NOT_TRACK=1` or `HTTPRUNNER_TELEMETRY_OPTOUT=1`.

## GUI and TUI Apps

While I love the CLI, sometimes a visual interface is better. `httprunner` includes two additional modes:

1.  **TUI (Terminal User Interface)**: A text-based UI that runs in your terminal. It's great for interactive testing on remote servers or when you want a visual dashboard without leaving your shell.
2.  **GUI (Graphical User Interface)**: A native application that provides a rich desktop experience, including request history, environment management, and a modern look and feel.

## Publishing and Installation

I've made `httprunner` available through multiple channels to ensure it's easy to install on any platform.

*   **Crates.io**: `cargo install httprunner`
*   **Snap Store**: `sudo snap install httprunner`
*   **Binaries**: Pre-built binaries for Linux, macOS, and Windows are available on [GitHub Releases](https://github.com/christianhelle/httprunner/releases).
*   **Install Scripts**:
    *   Linux/macOS: `curl -fsSL https://christianhelle.com/httprunner/install | bash`
    *   Windows: `irm https://christianhelle.com/httprunner/install.ps1 | iex`

## AI Assistance

This project has been a collaborative effort between me and AI. I utilized GitHub Copilot extensively throughout the development process. It helped with:
*   **PR Reviews**: Catching potential issues and suggesting idiomatic Rust patterns.
*   **Housekeeping**: Automating routine tasks and documentation updates.
*   **Library Discovery**: Finding the best crates for the TUI (like `ratatui`) and CLI argument parsing (`clap`).

Building `httprunner` has been a rewarding journey. It's not just a tool I built; it's a tool I use. I hope you find it as useful as I do!
