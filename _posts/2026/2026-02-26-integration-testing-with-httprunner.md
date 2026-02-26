---
layout: post
title: Integration Testing REST APIs with .http Files and HTTP Runner
date: 2026-02-26
author: Christian Helle
tags:
  - Testing
  - Integration Testing
  - REST
  - HTTP
  - Rust
redirect_from: 
  - 2026/02/26/integration-testing-with-httprunner
  - 2026/02/26/integration-testing-with-httprunner/
  - 2026/02/integration-testing-with-httprunner
  - 2026/02/integration-testing-with-httprunner/
  - 2026/integration-testing-with-httprunner
  - 2026/integration-testing-with-httprunner/
  - integration-testing-with-httprunner
  - integration-testing-with-httprunner/
---

Integration testing REST APIs is a crucial part of ensuring the reliability of micro-services and web applications. While there are many tools available, using simple `.http` files offers a lightweight and version-controllable approach that I really love.

In this post, I'll explore how to use **HTTP File Runner** (or `httprunner`), a command-line tool I built in Rust, to execute advanced integration test scenarios using `.http` files. We'll cover everything from variable management to conditional execution and CI/CD integration.

## Getting Started

First, ensure you have `httprunner` installed. You can install it via a simple script or download a release from the [GitHub repository](https://github.com/christianhelle/httprunner).

```bash
# Linux/macOS
curl -fsSL https://christianhelle.com/httprunner/install | bash

# Windows
irm https://christianhelle.com/httprunner/install.ps1 | iex
```

If you're on Ubuntu then you can also install it using `snap`

```bash
snap install httprunner
```

Once installed, you can run any `.http` file:

```bash
httprunner tests.http
```

## Global Variables

You can define global variables at the top of your `.http` file using the `@` syntax. This is perfect for values that are reused across multiple requests, like a base URL.

{% raw %}
```http
@HostAddress = https://httpbin.org
@ContentType = application/json

GET {{HostAddress}}/get
Content-Type: {{ContentType}}
```
{% endraw %}

## Built-in Functions

`httprunner` provides several built-in functions for generating dynamic data, which is essential for testing scenarios like creating unique users or generating timestamps.

- `guid()` / `GUID()`: Generates a UUID.
- `string()` / `STRING()`: Generates a random alphanumeric string.
- `number()` / `NUMBER()`: Generates a random integer (0-100).
- `getdate()`: Current date (YYYY-MM-DD).
- `gettime()`: Current time (HH:MM:SS).
- `getdatetime()`: Current date and time.
- `base64_encode('value')`: Base64 encodes the string.

Functions are case-insensitive and can be used in headers, bodies, or URLs.

```http
POST https://api.example.com/users
Content-Type: application/json

{
  "id": "guid()",
  "username": "user_string()",
  "created_at": "getdatetime()"
}
```

## Environment Variables

For different environments (development, staging, production), you shouldn't hard code values. `httprunner` supports loading variables from a `http-client.env.json` file, compatible with the VS Code REST Client extension. In most cases, the environment file would contain secrets like API keys or tokens that you don't want to commit to version control.

Create a `http-client.env.json` file:

```json
{
  "dev": {
    "HostAddress": "https://dev-api.example.com",
    "ApiKey": "dev-secret"
  },
  "prod": {
    "HostAddress": "https://api.example.com",
    "ApiKey": "prod-secret"
  }
}
```

Reference these variables in your `.http` file:

{% raw %}
```http
GET {{HostAddress}}/users
Authorization: Bearer {{ApiKey}}
```
{% endraw %}

Then run `httprunner` with the `--env` flag:

```bash
httprunner tests.http --env dev
```

## Generating Environment Files

Manually managing `http-client.env.json` files can be tedious, especially when dealing with short-lived tokens or multiple environments. I recommend scripting the generation of this file.

Here is an example PowerShell script that fetches access tokens from Azure CLI and generates a comprehensive environment file for localhost, Docker, and development environments:

```powershell
$management_api_tokens = az account get-access-token `
  --scope app://api.example.net/dev/management_api/.default | ConvertFrom-Json

$simulator_tokens = az account get-access-token `
  --scope app://api.example.net/dev/simulator/.default | ConvertFrom-Json

$environment = @{
  localhost = @{
    authorization = "Bearer " + $management_api_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "http://localhost:8900"
    simulator = "http://localhost:8901"
    management_api = "http://localhost:8150"
  }
  docker = @{
    authorization = "Bearer " + $management_api_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "http://host.docker.internal:8900"
    simulator = "http://host.docker.internal:8901"
    management_api = "http://host.docker.internal:8150"
  }
  dev = @{
    authorization = "Bearer " + $management_api_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "https://ocpi.example.net"
    simulator = "https://ocpi-simulator.example.net"
    management_api = "https://csms-api.example.net"
  }
}

Set-Content -Path ./http-client.env.json -Value ($environment | ConvertTo-Json -Depth 10)
```

## Delays

Rate limiting is a common constraint when testing APIs. `httprunner` allows you to introduce delays either globally or per request. My personal use case would be eventually consistent systems where you want to wait for a certain state before proceeding.

To add a delay between every request in a run, use the CLI flag:

```bash
httprunner tests.http --delay 500
```

For more granular control, use comments in your `.http` file:

```http
# @pre-delay 1000
# @post-delay 500
GET https://httpbin.org/get
```

- `@pre-delay`: Wait before sending the request (in milliseconds).
- `@post-delay`: Wait after the request completes (in milliseconds).

## Timeouts

Network conditions can be unpredictable. You can configure timeouts to fail tests if an API is too slow. If you're testing against a local development server, you might want to set a shorter timeout than the default 30 seconds.

```http
# Wait up to 5 seconds for a response
# @timeout 5000 ms
GET https://api.example.net/delay/2

# Custom connection timeout (default is 30s)
# @connection-timeout 10 s
GET https://api.example.net/get
```

Supported units include `ms`, `s`, and `m`.

## Request Chaining

One of the most powerful features for integration testing is chaining requests—using data from a previous response in a subsequent request. `httprunner` supports extracting values from headers, JSON bodies, and even the original request data.

### Request Variable Syntax

The syntax for request variables is:

{% raw %}
```text
{{<request_name>.<source>.<part>.<path>}}
```
{% endraw %}

- **request_name**: The name defined via `# @name <name>`
- **source**: `request` or `response`
- **part**: `body` or `headers`
- **path**: The specific value to extract

### Extraction Patterns

- **JSON Bodies**: Use JSONPath syntax (e.g., `$.user.id`, `$.items[0].name`).
- **Headers**: Use the header name (e.g., `Content-Type`, `Location`).
- **Full Body**: Use `*` to extract the entire body.

### Example Scenario

{% raw %}
```http
# @name create_user
POST https://api.example.com/users
Content-Type: application/json

{
  "username": "test_user",
  "role": "admin"
}

###

# @name login
POST https://api.example.com/login
Content-Type: application/json

{
  "username": "{{create_user.request.body.$.username}}",
  "password": "default_password"
}

###

# Use the token from login response
GET https://api.example.com/admin/dashboard
Authorization: Bearer {{login.response.body.$.token}}
X-User-Role: {{create_user.request.body.$.role}}
```
{% endraw %}

## Conditional Execution

Complex test scenarios often require conditional logic. You might want to skip a cleanup step if the creation failed, or run specific tests only if a feature flag is enabled.

`httprunner` provides `@dependsOn` and `@if` directives.

{% raw %}
```http
# @name create_user
POST https://api.example.com/users
...

###

# Only run if create_user succeeded (HTTP 2xx)
# @dependsOn create_user
POST https://api.example.com/users/{{create_user.response.body.$.id}}/activate

###

# Only run if the user status is "active"
# @if create_user.response.body.$.status active
GET https://api.example.com/users/{{create_user.response.body.$.id}}
```
{% endraw %}

## Assertions

No test is complete without verification. `httprunner` allows you to assert response status, headers, and body content directly in your `.http` file.

### Basic Assertions

```http
GET https://httpbin.org/json
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
EXPECTED_RESPONSE_BODY "slideshow"
```

### Variable Substitution in Assertions

Crucially, **variables are fully supported in assertions**, allowing you to validate that a response matches input parameters or previous request data.

{% raw %}
```http
@expected_status=200
@user_id=123

# @name create_user
POST https://api.example.com/users
{ "id": "{{user_id}}" }

###

GET https://api.example.com/users/{{user_id}}

EXPECTED_RESPONSE_STATUS {{expected_status}}
EXPECTED_RESPONSE_BODY "{{user_id}}"
EXPECTED_RESPONSE_HEADERS "Location: /users/{{user_id}}"
```
{% endraw %}

This makes dynamic testing significantly easier, as you don't need to hardcode expected values.

If any assertion fails, `httprunner` will report the test as failed and exit with a non-zero status code, which is essential for CI/CD pipelines.

Note: For requests that have assertions that expect failed responses (e.g., testing error handling), you can use `EXPECTED_RESPONSE_STATUS` to specify the expected failure status code (like 400 or 404). The "failed" request will no longer be flagged as failed, but instead will be validated against the expected status code and assertions.

## Verbose and Discovery Mode

When developing or debugging, you often need more insight into what `httprunner` is doing.

### Verbose Mode

Use the `--verbose` flag to see detailed request and response information, including full headers and bodies. Add `--pretty-json` to format JSON payloads for readability.

```bash
httprunner tests.http --verbose --pretty-json
```

### Discovery Mode

If you have tests scattered across multiple directories, use `--discover` to recursively find and execute all `.http` files.

```bash
httprunner --discover --verbose
```

## Report Generation and Logging

For long-running tests or CI pipelines, console output isn't enough. `httprunner` can generate structured reports and detailed logs.

### Reports

Generate summary reports in Markdown (default) or HTML using the `--report` flag. HTML reports include responsive styling and dark mode support.

```bash
# Generate Markdown report
httprunner tests.http --report

# Generate HTML report
httprunner tests.http --report html
```

### Logging

Use `--log` to save all console output to a file. This is useful for auditing and post-execution analysis.

```bash
httprunner tests.http --log execution.log
```

## Export Mode

Sometimes you need the raw HTTP data for documentation or manual replay. The `--export` flag saves every request and response to individual, timestamped files.

```bash
httprunner tests.http --export
```

This will create files like `GET_users_request_1738016400.log` and `GET_users_response_1738016400.log` containing the exact payload sent and received.

## Insecure HTTPS

When testing against local development environments or staging servers with self-signed certificates, SSL validation can be a blocker. Use `--insecure` to bypass these checks.

```bash
httprunner https://localhost:5001/api/tests.http --insecure
```

**Note:** Only use this in trusted environments!

## Telemetry

`httprunner` collects anonymous usage data to help improve the tool. If you prefer to opt-out, you can disable it via a flag or environment variable.

```bash
# Disable via CLI
httprunner tests.http --no-telemetry

# Disable via Environment Variable
export HTTPRUNNER_TELEMETRY_OPTOUT=1
# or
export DO_NOT_TRACK=1
```

## CI/CD Integration

Automating these tests in a CI/CD pipeline is straightforward. Since `httprunner` is available as a Docker image and a standalone binary, it fits easily into GitHub Actions or GitLab CI.

Here is an example **GitHub Actions** workflow:

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Install HTTP Runner
        run: snap install httprunner

      - name: Run API Tests
        run: tests/api.http --env staging --report markdown
```

Or, if you prefer installing the binary:

```yaml
- name: Install HTTP Runner
  run: curl -fsSL https://christianhelle.com/httprunner/install | bash

- name: Run Tests
  run: httprunner tests/*.http --env staging --report html
```

This setup ensures that every change is validated against your API, providing fast feedback on regressions.

## Conclusion

By combining the simplicity of `.http` files with the advanced features of `httprunner`, you can build a robust integration testing suite that lives right alongside your code. It's version-controlled, easy to read, and powerful enough for complex scenarios involving authentication, chaining, and conditional logic.

Give it a try and let me know what you think!
