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

Integration testing REST APIs is a crucial part of ensuring the reliability of microservices and web applications. While there are many tools available, using simple `.http` files offers a lightweight and version-controllable approach that developers love.

In this post, I'll explore how to use **HTTP Runner** (or `httprunner`), a command-line tool I built in Rust, to execute advanced integration test scenarios directly from `.http` files. We'll cover everything from variable management to conditional execution and CI/CD integration.

## Getting Started

First, ensure you have `httprunner` installed. You can install it via a simple script or download a release from the [GitHub repository](https://github.com/christianhelle/httprunner).

```bash
# Linux/macOS
curl -fsSL https://christianhelle.com/httprunner/install | bash

# Windows
irm https://christianhelle.com/httprunner/install.ps1 | iex
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

## Environment Variables

For different environments (development, staging, production), you shouldn't hardcode values. `httprunner` supports loading variables from a `http-client.env.json` file, compatible with the VS Code REST Client extension.

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
Write-Host "Getting access token"
$csms_tokens = az account get-access-token --scope app://api.example.net/dev/csms/.default | ConvertFrom-Json
$simulator_tokens = az account get-access-token --scope app://api.example.net/dev/simulator/.default | ConvertFrom-Json

Write-Host "Creating environment file"
$environment = @{
  localhost = @{
    authorization = "Bearer " + $csms_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "http://localhost:8900"
    simulator = "http://localhost:8901"
    csms = "http://localhost:8150"
    tenant_id = "00000000-0000-0000-0000-000000000000"
  }
  docker = @{
    authorization = "Bearer " + $csms_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "http://host.docker.internal:8900"
    simulator = "http://host.docker.internal:8901"
    csms = "http://host.docker.internal:8150"
    tenant_id = "00000000-0000-0000-0000-000000000000"
  }
  dev = @{
    authorization = "Bearer " + $csms_tokens.accessToken
    simulator_authorization = "Bearer " + $simulator_tokens.accessToken
    cpo = "https://ocpi.dev001.example.net"
    simulator = "https://ocpi-simulator.dev001.example.net"
    csms = "https://csms-api.dev001.example.net"
    tenant_id = "10000000-0000-0000-0000-000000000000"
  }
}
Set-Content -Path ./http-client.env.json -Value ($environment | ConvertTo-Json -Depth 10)
```

## Delays

Rate limiting is a common constraint when testing APIs. `httprunner` allows you to introduce delays either globally or per request.

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

Network conditions can be unpredictable. You can configure timeouts to fail tests if an API is too slow.

```http
# Wait up to 5 seconds for a response
# @timeout 5000 ms
GET https://httpbin.org/delay/2

# Custom connection timeout (default is 30s)
# @connection-timeout 10 s
GET https://httpbin.org/get
```

Supported units include `ms`, `s`, and `m`.

## Request Chaining

One of the most powerful features for integration testing is chaining requests—using data from a previous response in a subsequent request.

1. **Name** the request using `# @name <requestName>`.
2. **Reference** its response in later requests using `{{requestName.response.body.path}}`.

{% raw %}

```http
# @name login
POST https://api.example.com/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}

###

# Use the token from the login response
GET https://api.example.com/admin/dashboard
Authorization: Bearer {{login.response.body.$.token}}
```

{% endraw %}

You can access headers (`.headers.Header-Name`) and body properties (using JSONPath syntax like `$.user.id`).

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

No test is complete without verification. You can assert the response status, headers, and body content directly in the `.http` file.

```http
GET https://httpbin.org/json

# Assert status code
EXPECTED_RESPONSE_STATUS 200

# Assert a header value
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"

# Assert body content (contains string)
EXPECTED_RESPONSE_BODY "slideshow"
```

If any assertion fails, `httprunner` will report the test as failed and exit with a non-zero status code, which is essential for CI/CD pipelines.

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

      - name: Run API Tests
        uses: docker://christianhelle/httprunner:latest
        with:
          args: tests/api.http --env staging --report markdown
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
