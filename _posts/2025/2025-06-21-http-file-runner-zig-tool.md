---
layout: post
title: HTTP File Runner - A Command-Line Tool for Testing HTTP Requests Written in Zig
date: 2025-06-21
author: Christian Helle
tags:
- Zig
- HTTP Files
redirect_from:
- /2025/06/http-file-runner-zig-tool
- /2025/06/http-file-runner-zig-tool/
- /2025/http-file-runner-zig-tool/
- /2025/http-file-runner-zig-tool
- /http-file-runner-zig-tool/
- /http-file-runner-zig-tool
---

I'm excited to share my latest project: [HTTP File Runner](https://github.com/christianhelle/httprunner), a command-line tool written in Zig that parses `.http` files and executes HTTP requests. This tool provides colored output to indicate success or failure, making it easy to test APIs and web services directly from your terminal.

This project started as a learning exercise to explore the Zig programming language's capabilities. In a previous post, entitled [Generate .http files from OpenAPI specifications](/2023/11/http-file-generator.html), I wrote about [HTTP File Generator](https://github.com/christianhelle/httpgenerator), a tool that generates `.http` files from OpenAPI specifications. It felt natural to create a companion tool that could execute these generated files outside of an IDE environment. The combination of these two tools creates a complete workflow: generate HTTP files from your API specifications, then run them from the command line for testing and validation.

As developers, we often find ourselves testing REST APIs manually through various tools like Scalar, SwaggerUI, Postman, or Insomnia. While these tools are excellent for interactive testing, they can become cumbersome when you need to run the same tests repeatedly, integrate them into automated workflows, or share them with team members in a version-controlled manner. This is where [HTTP File Runner](https://github.com/christianhelle/httprunner) shines, it bridges the gap between manual testing and automation by bringing the simplicity of `.http` files to the command line.

## What is HTTP File Runner?

[HTTP File Runner](https://github.com/christianhelle/httprunner) is a simple yet powerful tool that reads `.http` files (the same format used by popular IDEs like Visual Studio Code, JetBrains IDEs, and Visual Studio 2022) and executes the HTTP requests defined within them. It's designed to be fast, reliable, and developer-friendly.

As a learning exercise in Zig, this project allowed me to explore systems programming concepts while building something genuinely useful. The choice to create an HTTP file runner was driven by practical needs: having already developed [HTTP File Generator](https://github.com/christianhelle/httpgenerator) to create `.http` files from OpenAPI specifications, I needed a reliable way to execute these files in automated environments without relying on IDE extensions or GUI tools.

The beauty of this approach lies in its simplicity. `.http` files are plain text files that can be generated, version-controlled, shared between team members, and executed across different environments. These files are human-readable and can be edited with any text editor. This makes them perfect for documentation, onboarding new team members, and ensuring consistency across development environments.

What sets [HTTP File Runner](https://github.com/christianhelle/httprunner) apart from other command-line HTTP tools is its focus on batch processing and developer experience. While tools like `curl` are excellent for single requests, HTTP File Runner excels at running multiple related requests, providing comprehensive reporting, and offering features specifically designed for API testing workflows.

## Key Features

The tool comes packed with features that make API testing a breeze, each designed to address common pain points in API development and testing workflows:

- **Parse and execute HTTP requests** from `.http` files - The core functionality that reads the standard HTTP file format and executes requests sequentially
- **Support for multiple files** - Run several `.http` files in a single command, perfect for organizing tests by feature or service
- **Discovery mode** - Recursively find and run all `.http` files in a directory tree, ideal for comprehensive test suites
- **Verbose mode** for detailed request and response information - See exactly what's being sent and received, invaluable for debugging
- **Logging mode** to save all output to a file for analysis and reporting - Essential for CI/CD pipelines and audit trails
- **Color-coded output** (green for success, red for failure) - Immediate visual feedback on test results
- **Summary statistics** showing success/failure counts per file and overall - Quick overview of test suite health
- **Support for various HTTP methods** (GET, POST, PUT, DELETE, PATCH) - Covers all standard REST operations
- **Variables support** with substitution in URLs, headers, and request bodies - Enables dynamic and reusable test scenarios
- **Response assertions** for status codes, body content, and headers - Automated validation of API responses

These features work together to create a comprehensive testing solution that scales from simple smoke tests to complex integration test suites. The combination of batch processing, detailed reporting, and assertion capabilities makes it suitable for both development-time testing and production monitoring.

## Why Zig?

Choosing Zig for this project was intentional. This started as a learning exercise to explore the Zig programming language. In fact, choosing Zig came before deciding what to build. I primarily work with higher-level languages like C# in my day job, and I wanted to understand what Zig brings to the table. The decision proved to be an excellent choice for several compelling reasons:

### Performance

Zig compiles to highly optimized native code without the overhead of a runtime or garbage collector. This means HTTP File Runner starts instantly and processes requests with minimal memory footprint, crucial for CI/CD environments where every second counts.

The resulting binary is small (under 2MB), starts instantly, and handles network operations efficiently. The cross-platform support means teams can use the same tool regardless of their development environment, reducing friction and improving consistency.

### Memory management

Zig gives you explicit control over memory allocation and deallocation (similar to C/C++), but with helpful features to reduce errors. One standout is Zig's `defer` statement, which ensures resources are released when a scope exits—making it easy to prevent memory leaks and resource leaks. While Zig does not provide full memory safety guarantees, its compile-time checks, clear ownership model, and `defer` help you write reliable low-level code more safely than traditional C.

### Cross-platform

Zig's excellent cross-compilation support made it trivial to build binaries for Linux, macOS, and Windows from a single codebase. The build system handles platform-specific details seamlessly, which is essential for a tool that needs to work everywhere developers do.

### Simple syntax

Zig's philosophy of "no hidden control flow" means the code does exactly what it appears to do. This makes the codebase easier to understand, debug, and maintain. Coming from higher-level languages, I appreciated how Zig forces you to be explicit about your intentions.

### No runtime

Zero-cost abstractions and no garbage collector mean predictable performance characteristics. This is particularly important for a command-line tool that might be called thousands of times in automated testing scenarios.

### Great editor support

Zig's Language Server Protocol (LSP) implementation provides excellent code completion, diagnostics, and navigation features in editors like Visual Studio Code or Neovim. This made development smooth and productive, with real-time feedback and powerful refactoring tools available out of the box.

### Learning value

As someone whose day job is working in managed languages, exploring manual memory management and system-level programming concepts in Zig provided valuable insights into how software works at a lower level.

Compared to Rust, I found Zig's learning curve to be less steep, and more fun to write. Zig offers a straightforward syntax and fewer abstractions, which made it easier to grasp the core concepts quickly. While both languages provide low-level control and strong performance, Zig's simplicity and explicitness helped me become productive faster, making it an appealing choice for systems programming projects like this one.

### The Zig Zen

Zig's development philosophy, known as "The Zen of Zig," states the following:

- Communicate intent precisely.
- Edge cases matter.
- Favor reading code over writing code.
- Only one obvious way to do things.
- Runtime crashes are better than bugs.
- Compile errors are better than runtime crashes.
- Incremental improvements.
- Avoid local maximums.
- Reduce the amount one must remember.
- Focus on code rather than style.
- Resource allocation may fail; resource deallocation must succeed.
- Memory is a resource.
- Together we serve the users.

## Installation

Getting started is incredibly easy. The tool provides multiple installation options to suit different preferences and environments. I've focused on making the installation process as frictionless as possible, recognizing that developers often need to get tools up and running quickly.

### Quick Install (Recommended)

The fastest way to get started is using the automated install scripts. These scripts handle platform detection, architecture identification, and PATH configuration automatically:

**Linux/macOS:**

```bash
curl -fsSL https://christianhelle.com/httprunner/install | bash
```

**Windows (PowerShell):**

```powershell
irm https://christianhelle.com/httprunner/install.ps1 | iex
```

These scripts will automatically:

- Detect your operating system and CPU architecture
- Download the appropriate binary from the latest GitHub release
- Install it to a standard location (`/usr/local/bin` on Unix-like systems, `$HOME/.local/bin` as fallback)
- Optionally add the installation directory to your PATH
- Verify the installation was successful

### Other Installation Methods

For users who prefer different installation approaches or have specific requirements:

- **Snap Store**: `snap install httprunner` - Great for Ubuntu and other snap-enabled distributions, provides automatic updates
- **Manual Download**: Download from [GitHub Releases](https://github.com/christianhelle/httprunner/releases/latest) - Full control over installation location and process
- **Docker**: `docker pull christianhelle/httprunner` - Perfect for containerized environments or when you don't want to install binaries locally
- **Build from source**: Clone the repo and run `zig build` - For developers who want to customize the build or contribute to the project

Each method has its advantages: Snap provides automatic updates, manual download gives you full control, Docker ensures isolation, and building from source allows customization. Choose the method that best fits your workflow and security requirements.

## Usage Examples

Here are some common usage patterns that demonstrate the tool's flexibility and power. These examples progress from simple single-file execution to complex batch processing scenarios:

```bash
# Run a single .http file
httprunner api-tests.http

# Run with verbose output
httprunner api-tests.http --verbose

# Run multiple files
httprunner auth.http users.http posts.http

# Discover and run all .http files recursively
httprunner --discover

# Save output to a log file
httprunner api-tests.http --log results.txt

# Combine verbose mode with logging
httprunner --discover --verbose --log full-test-report.log
```

### Real-World Scenarios

**Development Workflow**: During development, you might run `httprunner --discover --verbose` to execute all tests in your project and see detailed output for debugging.

**CI/CD Integration**: In your build pipeline, use `httprunner --discover --log test-results.log` to run all tests and capture results for build reports.

**Environment Testing**: When deploying to a new environment, run `httprunner health-checks.http --env production --log deployment-validation.log` to verify everything is working correctly.

**Performance Monitoring**: Set up automated runs with `httprunner monitoring.http --log performance.log` to track API performance over time.

## HTTP File Format

The tool supports the standard `.http` file format:

```http
# Comments start with #

# Basic GET request
GET https://api.github.com/users/octocat

# Request with headers
GET https://httpbin.org/headers
User-Agent: HttpRunner/1.0
Accept: application/json

# POST request with body
POST https://httpbin.org/post
Content-Type: application/json

{
  "name": "test",
  "value": 123
}
```

## Variables and Environment Support

One of the most powerful features is the comprehensive variable support system, which enables you to create flexible, reusable test suites that work across different environments. This feature addresses one of the biggest pain points in API testing: managing different configurations for development, staging, and production environments.

### Basic Variable Usage

Variables are defined using the `@` syntax and referenced with double curly braces:

```http
@hostname=localhost
@port=8080
@baseUrl=https://{{hostname}}:{{port}}

GET {{baseUrl}}/api/users
Authorization: Bearer {{token}}
```

### Advanced Variable Composition

Variables can be composed from other variables, allowing you to build complex configurations incrementally:

```http
@protocol=https
@hostname=api.example.com
@port=443
@version=v1
@baseUrl={{protocol}}://{{hostname}}:{{port}}/{{version}}

# Now you can use the composed URL
GET {{baseUrl}}/users
GET {{baseUrl}}/posts
GET {{baseUrl}}/comments
```

### Environment Configuration Files

For managing different environments, you can create an `http-client.env.json` file with environment-specific values:

```json
{
  "dev": {
    "HostAddress": "https://localhost:44320",
    "ApiKey": "dev-api-key-123",
    "Environment": "development"
  },
  "staging": {
    "HostAddress": "https://staging.example.com",
    "ApiKey": "staging-api-key-456",
    "Environment": "staging"
  },
  "prod": {
    "HostAddress": "https://api.example.com",
    "ApiKey": "prod-api-key-789",
    "Environment": "production"
  }
}
```

Then specify the environment when running tests:

```bash
httprunner api-tests.http --env dev
```

This approach allows you to maintain a single set of test files while easily switching between different environments. The variable override behavior is intelligent: environment variables are loaded first, then any variables defined in the `.http` file override them, giving you both flexibility and control.

## Response Assertions

The response assertion system is one of HTTP File Runner's most powerful features, transforming it from a simple request executor into a comprehensive API testing framework. This system enables you to validate not just that your API responds, but that it responds correctly with the expected data, status codes, and headers. By incorporating assertions into your `.http` files, you can create robust test suites that catch regressions, validate API contracts, and ensure your services behave correctly across different environments.

### Understanding Assertion Philosophy

HTTP File Runner's assertion system is designed around the principle of explicit validation. Rather than assuming that any response is acceptable, assertions force you to define what constitutes a successful interaction with your API. This approach catches subtle bugs that might otherwise go unnoticed, such as:

- APIs returning 200 status codes with error messages in the body
- Correct data with unexpected content types or encoding
- Missing or incorrect security headers
- Performance regressions indicated by unexpected response patterns

### Types of Assertions

#### Status Code Assertions

Status code assertions are the foundation of API testing, ensuring your endpoints return the correct HTTP status codes for different scenarios:

```http
# Test successful resource retrieval
GET https://httpbin.org/status/200
EXPECTED_RESPONSE_STATUS 200

# Test resource not found scenario
GET https://httpbin.org/status/404
EXPECTED_RESPONSE_STATUS 404
```

Status code assertions are particularly valuable for testing error conditions and edge cases. You can verify that your API correctly returns 400 for bad requests, 401 for unauthorized access, 403 for forbidden operations, and 404 for missing resources.

#### Response Body Assertions

Response body assertions validate the actual content returned by your API. These assertions use substring matching, making them flexible enough to work with various response formats while being specific enough to catch content errors:

```http
# Test JSON response content
GET https://httpbin.org/json
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "slideshow"
EXPECTED_RESPONSE_BODY "Sample Slide Show"

# Test API response structure
GET https://jsonplaceholder.typicode.com/users/1
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "Leanne Graham"
EXPECTED_RESPONSE_BODY "@hildegard.org"
```

Body assertions are case-sensitive and look for exact substring matches. This approach allows you to validate specific field values in JSON responses, error messages, HTML content, XML elements, and plain text responses.

#### Response Header Assertions

Header assertions validate the metadata returned with your API responses. These are crucial for testing security headers, content types, caching directives, and custom application headers:

```http
# Test content type and security headers
GET https://httpbin.org/json
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"

# Test custom application headers
POST https://httpbin.org/post
Content-Type: application/json

{"test": "data"}

EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
EXPECTED_RESPONSE_HEADERS "Server: gunicorn"
```

Header assertions use substring matching on the full header line (including both name and value). This flexibility allows you to validate exact header values, check for header presence, test multiple values for the same header, and verify security and compliance headers.

### Advanced Assertion Patterns

#### Comprehensive API Endpoint Testing

Here's an example of thoroughly testing a user management API endpoint:

```http
# Test user creation with comprehensive validation
POST https://api.example.com/users
Content-Type: application/json
Authorization: Bearer {{auth_token}}

{
  "name": "Christian Helle",
  "email": "christian.helle@example.com",
  "role": "user"
}

EXPECTED_RESPONSE_STATUS 201
EXPECTED_RESPONSE_BODY "Christian Helle"
EXPECTED_RESPONSE_BODY "christian.helle@example.com"
EXPECTED_RESPONSE_BODY "\"id\":"
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
EXPECTED_RESPONSE_HEADERS "Location: /users/"

###

# Verify user is deleted properly
DELETE https://api.example.com/users/{{user_id}}
Authorization: Bearer {{auth_token}}

EXPECTED_RESPONSE_STATUS 204

###

# Confirm user no longer exists
GET https://api.example.com/users/{{user_id}}
Authorization: Bearer {{auth_token}}

EXPECTED_RESPONSE_STATUS 404
EXPECTED_RESPONSE_BODY "User not found"
```

#### Error Condition Testing

Testing error conditions is just as important as testing success scenarios:

```http
# Test validation errors
POST https://api.example.com/users
Content-Type: application/json

{
  "name": "",
  "email": "invalid-email"
}

EXPECTED_RESPONSE_STATUS 400
EXPECTED_RESPONSE_BODY "validation error"
EXPECTED_RESPONSE_BODY "name is required"
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"

###

# Test authentication errors
GET https://api.example.com/users
Authorization: Bearer invalid-token

EXPECTED_RESPONSE_STATUS 401
EXPECTED_RESPONSE_BODY "unauthorized"
EXPECTED_RESPONSE_HEADERS "WWW-Authenticate: Bearer"
```

### Assertion Execution and Behavior

#### Processing Order and Logic

When HTTP File Runner encounters assertions in a `.http` file, it follows a specific execution pattern:

1. **Request Execution**: The HTTP request is sent normally, following all redirects and handling authentication
2. **Response Capture**: The complete response is captured, including status code, headers, and body
3. **Assertion Evaluation**: Each assertion is evaluated in the order it appears in the file
4. **Result Aggregation**: All assertion results are collected and reported
5. **Request Status Determination**: The request is marked as successful only if ALL assertions pass

#### Enhanced Logging and Reporting

When assertions are present, HTTP File Runner automatically enables enhanced logging, even in non-verbose mode:

```text
🚀 HTTP File Runner - Processing file: .\tests\user-api.http
==================================================
Found 3 HTTP request(s)

✅ POST https://api.example.com/users - Status: 201 - 245ms
   ✅ Status assertion passed: 201
   ✅ Body assertion passed: "Christian Helle"
   ✅ Body assertion passed: "christian.helle@example.com"
   ✅ Header assertion passed: "Content-Type: application/json"
   ✅ Header assertion passed: "Location: /users/"

❌ GET https://api.example.com/users/invalid - Status: 404 - 123ms
   ✅ Status assertion passed: 404
   ❌ Body assertion failed: Expected "user not found" but got "Resource not found"
   ✅ Header assertion passed: "Content-Type: application/json"

✅ DELETE https://api.example.com/users/123 - Status: 204 - 156ms
   ✅ Status assertion passed: 204
   ✅ Header assertion passed: "X-Deleted-At:"

==================================================
File Summary: 2/3 requests succeeded
Total Assertions: 9 passed, 1 failed
```

#### Verbose Mode Enhancement

In verbose mode, HTTP File Runner provides even more detailed information about assertion evaluation:

```text
🚀 HTTP File Runner - Processing file: .\tests\detailed-test.http
==================================================

📤 Request: POST https://api.example.com/users
Headers:
  Content-Type: application/json
  Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...

Body:
{
  "name": "Christian Helle",
  "email": "christian.helle@example.com"
}

📥 Response: 201 Created (245ms)
Headers:
  Content-Type: application/json; charset=utf-8
  Location: /users/12345
  X-RateLimit-Remaining: 99
  Content-Length: 156

Body:
{
  "id": 12345,
  "name": "Christian Helle",
  "email": "christian.helle@example.com",
  "created_at": "2025-06-21T10:30:00Z",
  "role": "user"
}

🔍 Assertion Results:
   ✅ EXPECTED_RESPONSE_STATUS 201
      Expected: 201, Actual: 201 ✓

   ✅ EXPECTED_RESPONSE_BODY "Christian Helle"
      Found substring "Christian Helle" in response body ✓

   ✅ EXPECTED_RESPONSE_BODY "christian.helle@example.com"
      Found substring "christian.helle@example.com" in response body ✓

   ✅ EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
      Found header match "Content-Type: application/json" ✓

   ✅ EXPECTED_RESPONSE_HEADERS "Location: /users/"
      Found header match "Location: /users/" ✓

✅ Request completed successfully - All assertions passed
```

### Best Practices for Assertion Design

#### Start Simple, Build Complex

Begin with basic status code assertions and gradually add more specific validations:

```http
# Level 1: Basic connectivity
GET https://api.example.com/health
EXPECTED_RESPONSE_STATUS 200

# Level 2: Add content validation
GET https://api.example.com/health
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "healthy"

# Level 3: Full contract validation
GET https://api.example.com/health
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "healthy"
EXPECTED_RESPONSE_BODY "\"uptime\":"
EXPECTED_RESPONSE_BODY "\"version\":"
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
```

#### Use Specific but Resilient Assertions

Make your assertions specific enough to catch real issues, but resilient enough to not break on minor changes:

```http
# Good: Validates presence of key fields
EXPECTED_RESPONSE_BODY "\"id\":"
EXPECTED_RESPONSE_BODY "\"name\":"
EXPECTED_RESPONSE_BODY "\"email\":"

# Avoid: Too specific, breaks on formatting changes
EXPECTED_RESPONSE_BODY "  \"id\": 123,"
```

#### Test Both Success and Failure Scenarios

Comprehensive testing includes validating that your API fails correctly:

```http
# Test success case
POST https://api.example.com/login
Content-Type: application/json

{"username": "valid@example.com", "password": "correct"}

EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "\"token\":"

###

# Test failure case
POST https://api.example.com/login
Content-Type: application/json

{"username": "invalid@example.com", "password": "wrong"}

EXPECTED_RESPONSE_STATUS 401
EXPECTED_RESPONSE_BODY "invalid credentials"
```

### Integration with Development Workflows

#### Pre-commit Testing

Use assertions in pre-commit hooks to catch API regressions before they reach version control:

```bash
# In your pre-commit script
httprunner tests/api-contracts.http --verbose
if [ $? -ne 0 ]; then
    echo "API contract tests failed. Commit blocked."
    exit 1
fi
```

#### Environment Validation

After deployments, run assertion-heavy test suites to validate the new environment:

```bash
# Validate staging deployment
httprunner tests/smoke-tests.http --env staging --log staging-validation.log

# Validate production deployment
httprunner tests/critical-paths.http --env production --log prod-validation.log
```

#### Performance and Regression Testing

Combine assertions with regular execution to catch both functional and performance regressions:

```http
# This request should complete quickly and return valid data
GET https://api.example.com/users?page=1&limit=10
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "\"users\":"
EXPECTED_RESPONSE_BODY "\"total\":"
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"

# The timing information in the output helps track performance trends
```

This comprehensive assertion system ensures that your API tests are thorough, reliable, and provide meaningful feedback when things go wrong. By incorporating detailed assertions into your HTTP files, you create living documentation of your API contracts while building robust test suites that catch issues early in the development cycle.

## Logging and CI/CD Integration

The logging feature makes this tool perfect for CI/CD pipelines and automated testing scenarios. Modern software development relies heavily on automation, and HTTP File Runner's logging capabilities are designed to integrate seamlessly into these workflows.

### Comprehensive Logging Options

The `--log` flag provides several options for capturing test results:

```bash
# Generate test reports for build systems
httprunner --discover --log test_report_$(date +%Y%m%d_%H%M%S).log

# Daily API health checks
httprunner health-checks.http --verbose --log daily_health_check.log
```

### Integration Scenarios

**Build Pipeline Integration**: Configure your CI/CD system to run HTTP File Runner as part of the build process. The tool's exit codes and log files provide everything needed for build status determination and result reporting.

**Deployment Validation**: After deploying to a new environment, automatically run a suite of health check requests to verify that all services are responding correctly.

**Monitoring and Alerting**: Set up scheduled runs of critical API tests, with log files feeding into monitoring systems that can alert on failures or performance degradation.

**Documentation and Reporting**: The verbose logging mode captures complete request/response cycles, making it easy to generate API documentation or troubleshooting guides from actual test runs.

### Log File Benefits

Log files preserve:

- Complete terminal output including colors and emojis
- Detailed HTTP request and response information (when using `--verbose`)
- Success/failure indicators and summary statistics
- Error messages and network diagnostics
- Execution timestamps and duration metrics

This comprehensive logging makes HTTP File Runner suitable not just for testing, but for API monitoring, documentation generation, and troubleshooting production issues.

## Output Examples

The tool provides beautiful, color-coded output:

```text
🚀 HTTP File Runner - Processing file: .\examples\simple.http
==================================================
Found 4 HTTP request(s)

✅ GET https://httpbin.org/status/200 - Status: 200 - 557ms
❌ GET https://httpbin.org/status/404 - Status: 404 - 542ms
✅ GET https://api.github.com/zen - Status: 200 - 85ms
✅ GET https://jsonplaceholder.typicode.com/users/1 - Status: 200 - 91ms

==================================================
File Summary: 3/4 requests succeeded
```

## What's Next?

I'm continuously improving the tool based on user feedback and real-world usage patterns. The roadmap includes several exciting enhancements that will further enhance its capabilities:

### Planned Enhancements

- **Request timeout configuration** - Configurable timeouts per request or globally, essential for testing APIs with varying response times or unreliable networks.

- **JSON response formatting** - Pretty-printing and syntax highlighting for JSON responses, making it easier to read and debug API responses.

- **Export results to different formats** - JSON, XML, CSV, and HTML reports for integration with various reporting and analysis tools.

- **Parallel execution** - Option to run multiple requests concurrently for faster test suite execution.

## Conclusion

[HTTP File Runner](https://github.com/christianhelle/httprunner) represents my exploration into newer programming languages with Zig while solving a real-world problem that affects developers daily. What started as a learning exercise to understand Zig evolved into a genuinely useful tool that complements my existing [HTTP File Generator](https://github.com/christianhelle/httpgenerator) project.

Building [HTTP File Runner](https://github.com/christianhelle/httprunner) in Zig has been an exercise in balancing performance with usability.

The modular code structure makes the project maintainable and extensible. Each component, from HTTP parsing to response validation, is cleanly separated, making it easy to add new features or modify existing behavior.

Give it a try and let me know what you think! If you find it useful, consider starring the repository or sharing it with fellow developers.
