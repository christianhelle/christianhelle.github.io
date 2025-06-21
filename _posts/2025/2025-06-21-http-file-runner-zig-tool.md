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

I'm excited to share my latest project: [HTTP File Runner](https://github.com/christianhelle/httprunner), a command-line tool written in Zig that parses `.http` files and executes HTTP requests. This tool provides colored output with emojis to indicate success or failure, making it easy to test APIs and web services directly from your terminal.

## What is HTTP File Runner?

HTTP File Runner is a simple yet powerful tool that reads `.http` files (the same format used by popular IDEs like Visual Studio Code and JetBrains) and executes the HTTP requests defined within them. It's designed to be fast, reliable, and developer-friendly.

## Key Features

The tool comes packed with features that make API testing a breeze:

🚀 **Parse and execute HTTP requests** from `.http` files  
📁 **Support for multiple files** - run several `.http` files in a single command  
🔍 **Discovery mode** - recursively find and run all `.http` files in a directory  
📝 **Verbose mode** for detailed request and response information  
📋 **Logging mode** to save all output to a file for analysis and reporting  
✅ **Color-coded output** (green for success, red for failure)  
📊 **Summary statistics** showing success/failure counts per file and overall  
🌐 **Support for various HTTP methods** (GET, POST, PUT, DELETE, PATCH)  
🔧 **Variables support** with substitution in URLs, headers, and request bodies  
🔍 **Response assertions** for status codes, body content, and headers  
🛡️ **Robust error handling** for network issues  

## Why Zig?

I chose Zig for this project for several reasons:

- **Performance**: Zig compiles to highly optimized native code
- **Memory safety**: Manual memory management with compile-time safety checks
- **Cross-platform**: Easy to build for multiple platforms
- **Simple syntax**: Clean, readable code without hidden complexity
- **No runtime**: Zero-cost abstractions and no garbage collector

## Installation

Getting started is incredibly easy. The tool provides multiple installation options:

### Quick Install (Recommended)

**Linux/macOS:**

```bash
curl -fsSL https://christianhelle.com/httprunner/install | bash
```

**Windows (PowerShell):**

```powershell
irm https://christianhelle.com/httprunner/install.ps1 | iex
```

### Other Installation Methods

- **Snap Store**: `sudo snap install httprunner`
- **Manual Download**: Download from [GitHub Releases](https://github.com/christianhelle/httprunner/releases/latest)
- **Docker**: `docker pull christianhelle/httprunner`
- **Build from source**: Clone the repo and run `zig build`

## Usage Examples

Here are some common usage patterns:

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

One of the most powerful features is variable support:

```http
@hostname=localhost
@port=8080
@baseUrl=https://{{hostname}}:{{port}}

GET {{baseUrl}}/api/users
Authorization: Bearer {{token}}
```

You can also use environment files (`http-client.env.json`) for different configurations:

```json
{
  "dev": {
    "HostAddress": "https://localhost:44320",
    "ApiKey": "dev-api-key-123"
  },
  "prod": {
    "HostAddress": "https://api.production.com",
    "ApiKey": "prod-api-key-789"
  }
}
```

Then specify the environment:
```bash
httprunner api-tests.http --env dev
```

## Response Assertions

The tool supports assertions to validate responses:

```http
# Status code assertion
GET https://httpbin.org/status/200
EXPECTED_RESPONSE_STATUS 200

# Response body assertion
GET https://httpbin.org/json
EXPECTED_RESPONSE_STATUS 200
EXPECTED_RESPONSE_BODY "slideshow"
EXPECTED_RESPONSE_HEADERS "Content-Type: application/json"
```

## Logging and CI/CD Integration

The logging feature makes this tool perfect for CI/CD pipelines:

```bash
# Generate test reports for build systems
httprunner --discover --log test_report_$(date +%Y%m%d_%H%M%S).log

# Daily API health checks
httprunner health-checks.http --verbose --log daily_health_check.log
```

## Output Examples

The tool provides beautiful, emoji-rich output:

```text
🚀 HTTP File Runner - Processing file: examples/simple.http
==================================================
Found 4 HTTP request(s)

✅ GET https://httpbin.org/status/200 - Status: 200
❌ GET https://httpbin.org/status/404 - Status: 404
✅ GET https://api.github.com/zen - Status: 200
✅ GET https://jsonplaceholder.typicode.com/users/1 - Status: 200

==================================================
Summary: 3/4 requests succeeded
```

## What's Next?

I'm continuously improving the tool. Some planned enhancements include:

- Full custom headers support (currently parsed but not fully applied)
- Advanced authentication methods (Basic, Bearer tokens)
- Request timeout configuration
- JSON response formatting
- Export results to different formats (JSON, XML, CSV)

## Conclusion

HTTP File Runner represents my exploration into systems programming with Zig while solving a real-world problem. It's designed to be fast, reliable, and developer-friendly. Whether you're testing APIs, running health checks, or integrating into CI/CD pipelines, this tool aims to make HTTP testing as simple as possible.

The project is open source and available under the MIT License. I welcome contributions, feedback, and feature requests!

**Links:**

- [GitHub Repository](https://github.com/christianhelle/httprunner)
- [Download Latest Release](https://github.com/christianhelle/httprunner/releases/latest)
- [Docker Hub](https://hub.docker.com/r/christianhelle/httprunner)

Give it a try and let me know what you think! If you find it useful, consider starring the repository or sharing it with fellow developers.
