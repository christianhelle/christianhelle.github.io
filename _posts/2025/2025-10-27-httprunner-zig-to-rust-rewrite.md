---
layout: post
title: Rewriting HTTP File Runner in Rust (from Zig)
date: 2025-10-27
author: Christian Helle
tags:
  - Rust
  - Zig
  - HTTP
  - Migration
redirect_from:
  - /2025/10/httprunner-zig-to-rust
  - /2025/10/httprunner-zig-to-rust/
  - /2025/httprunner-zig-to-rust/
  - /2025/httprunner-zig-to-rust
---

A few months ago, I wrote about [HTTP File Runner](/2025/06/http-file-runner-zig-tool), a command-line tool I built in Zig to execute `.http` files from the terminal. The project was a successful learning exercise and a genuinely useful tool. However, I recently completed a full rewrite of the project from Zig to Rust. This wasn't a decision made lightly or based on preferences—it was a **technical necessity**.

## The Critical Problem: HTTPS Certificate Validation

The primary driver for this migration was a **blocking technical limitation** in Zig's standard library. The issue was simple but insurmountable: Zig's HTTP client (`std.http`) cannot be configured to bypass certificate validation. This makes testing against development environments with self-signed certificates, a fundamental requirement for any serious HTTP testing tool, needlessly difficult, or impossible.

### The Failed Solution

I explored integrating libcurl to work around this limitation, but the cross-platform compilation complexity proved prohibitive. Zig's excellent cross-compilation support ironically became a liability when trying to integrate C libraries with complex build requirements across multiple platforms.

This wasn't about preferring one language over another—the Zig implementation simply couldn't meet basic requirements for development environment testing.

## Migration Overview

The migration was fully AI assisted, comprehensive, touching every aspect of the project:

- **54 commits** across the migration branch
- **3,419 lines added, 2,634 lines removed**
- **54 files changed**
- **12 core modules** successfully ported
- **100% feature parity** maintained

You can see the complete details in [Pull Request #43](https://github.com/christianhelle/httprunner/pull/43).

## Architecture: From Zig to Rust

### Module Structure

The Rust implementation maintains a clean, modular architecture:

```
src/
├── main.rs              # Application entry point
├── cli.rs               # Command-line interface with clap
├── types.rs             # Core data structures
├── colors.rs            # Terminal color utilities
├── parser.rs            # HTTP file parsing
├── environment.rs       # Environment file loading
├── runner.rs            # HTTP request execution with reqwest
├── assertions.rs        # Response assertion validation
├── request_variables.rs # Request variable substitution
├── processor.rs         # Request processing pipeline
├── discovery.rs         # File discovery with walkdir
├── log.rs              # Logging functionality
└── upgrade.rs          # Self-update feature
```

### Key Dependencies

The Rust ecosystem provided mature, battle-tested libraries for every requirement:

- **clap**: Modern command-line argument parsing with declarative macros
- **reqwest**: Full-featured HTTP client with TLS control
- **colored**: Cross-platform terminal colors
- **serde_json**: JSON parsing for environment files and chaining requests variables
- **walkdir**: Efficient directory traversal
- **anyhow**: Ergonomic error handling with context chaining

## Feature Parity Verification

All features from the Zig implementation are fully supported in the Rust version:

| Feature                                      | Status  |
| -------------------------------------------- | ------- |
| HTTP methods (GET, POST, PUT, DELETE, PATCH) | ✅ 100% |
| Variable substitution                        | ✅ 100% |
| Request variables & chaining                 | ✅ 100% |
| Response assertions (status, body, headers)  | ✅ 100% |
| Environment files (http-client.env.json)     | ✅ 100% |
| File discovery mode (--discover)             | ✅ 100% |
| Verbose mode (--verbose)                     | ✅ 100% |
| Logging to file (--log)                      | ✅ 100% |
| Version information (--version)              | ✅ 100% |
| Self-upgrade (--upgrade)                     | ✅ 100% |
| Colored output with emojis                   | ✅ 100% |
| Custom headers                               | ✅ 100% |
| Request body support                         | ✅ 100% |
| Multiple file processing                     | ✅ 100% |

## Technical Improvements

### Better Error Handling

**Zig**: Manual error unions and explicit error propagation

```zig
const result = try parseHttpFile(allocator, file_path);
defer result.deinit();
```

**Rust**: `anyhow::Result` with context chaining and detailed error messages

```rust
let result = parse_http_file(file_path)
    .context("Failed to parse HTTP file")?;
```

### Type Safety and Memory Management

**Zig**: Explicit memory management with `defer` statements

```zig
const allocator = std.heap.page_allocator;
var list = try std.ArrayList(u8).initCapacity(allocator, 1024);
defer list.deinit();
```

**Rust**: Ownership system with compile-time guarantees

```rust
let mut list = Vec::with_capacity(1024);
// Automatically dropped when out of scope
```

### HTTP Client Capabilities

**Zig**: Limited `std.http` with no TLS configuration options

**Rust**: Full TLS control, connection pooling, timeout management, and certificate validation options

```rust
let client = Client::builder()
    .danger_accept_invalid_certs(allow_insecure)
    .timeout(Duration::from_secs(30))
    .build()?;
```

This is the critical improvement that drove the entire migration. The `reqwest` library provides the flexibility needed for real-world testing scenarios.

### CLI Parsing

**Zig**: Manual argument parsing with custom logic

**Rust**: Declarative `clap` with automatic help generation

```rust
#[derive(Parser)]
#[command(name = "httprunner")]
#[command(about = "Run .http files from the command line")]
struct Cli {
    #[arg(help = "HTTP files to process")]
    files: Vec<PathBuf>,

    #[arg(short, long, help = "Enable verbose output")]
    verbose: bool,
}
```

## Migration Process

The migration followed a systematic, phased approach:

### Phase 1: Core Infrastructure

- Set up Rust project structure with Cargo.toml
- Implemented build script for version generation
- Created core type definitions
- Added color utilities
- Built HTTP file parser

### Phase 2: HTTP Execution

- Integrated `reqwest` for HTTP operations
- Implemented response assertions
- Added request variable substitution with JSONPath support
- Built environment file loader

### Phase 3: CLI & Features

- Implemented `clap`-based CLI interface
- Added file discovery mode
- Implemented logging functionality
- Added self-update feature
- Created comprehensive request processor

### Phase 4: Infrastructure

- Updated GitHub Actions workflows for Rust
- Migrated dev container to Rust toolchain
- Updated Docker configuration
- Modified release workflows for Rust binaries
- Added Snap packaging for Rust version

### Phase 5: Cleanup & Documentation

- Removed Zig implementation from main branch
- Updated all documentation for Rust
- Added migration guides
- Updated README with Rust instructions
- Added Cargo/Crates.io installation instructions

## Build System Comparison

| Task          | Zig (Legacy)                       | Rust (Current)          |
| ------------- | ---------------------------------- | ----------------------- |
| Debug build   | `zig build`                        | `cargo build`           |
| Release build | `zig build -Doptimize=ReleaseFast` | `cargo build --release` |
| Run tests     | `zig build test`                   | `cargo test`            |
| Format code   | `zig fmt .`                        | `cargo fmt`             |
| Lint code     | N/A                                | `cargo clippy`          |
| Clean         | `rm -rf zig-out zig-cache`         | `cargo clean`           |

The Rust tooling ecosystem provides a more comprehensive development experience with integrated testing, formatting, linting, and dependency management.

## Installation: Now Even Easier

The Rust version adds a new installation method via Cargo:

```bash
# Install from crates.io
cargo install httprunner
```

All previous installation methods remain supported:

- Automated installation scripts (Linux/macOS/Windows)
- Snap Store: `snap install httprunner`
- Manual download from GitHub Releases
- Docker: `docker pull christianhelle/httprunner`
- Build from source: `cargo build --release`

## The Zig Legacy

The original Zig implementation has been preserved in a separate repository: [christianhelle/httprunner-zig](https://github.com/christianhelle/httprunner-zig). While it's no longer actively maintained, it remains available as a historical reference and testament to Zig's capabilities within its limitations.

The Zig version was an excellent learning experience, and I genuinely enjoyed working with the language. Zig's simplicity, zero-cost abstractions, and explicit nature made it a pleasure to write. The limitation that forced this migration wasn't a reflection on Zig as a language—it was simply a missing feature in the standard library's HTTP implementation.

## Lessons Learned

### When to Rewrite

This migration taught me important lessons about when a rewrite is justified:

✅ **Good reasons to rewrite:**

- Blocking technical limitations that prevent core functionality
- Ecosystem maturity issues affecting long-term maintainability
- Fundamental architectural problems that can't be incrementally improved

❌ **Bad reasons to rewrite:**

- Language preference or "grass is greener" syndrome
- Minor inconveniences that can be worked around
- Wanting to try new technologies without clear benefits

### Language Selection Matters

While both Zig and Rust are excellent systems programming languages, their ecosystems have different maturity levels:

**Zig Strengths:**

- Simpler syntax and learning curve
- Excellent cross-compilation support
- No hidden control flow
- Explicit and predictable behavior

**Rust Strengths:**

- Mature ecosystem with battle-tested libraries
- Comprehensive standard library and crate ecosystem
- Strong compile-time guarantees via ownership system
- Extensive tooling (cargo, clippy, rustfmt)

For a production tool that needs to work reliably across various environments and scenarios, Rust's ecosystem maturity proved decisive.

## Performance Comparison

Both implementations are fast, but with different characteristics:

**Binary Size:**

- Zig: ~700KB (optimized for binary size)
- Rust (release): ~1.7MB (with all release build optimization and optimized for size)

**Startup Time:**

- Both: Instant (< 10ms)

**Memory Usage:**

- Both: Minimal (< 10MB for typical workloads)

**HTTP Performance:**

- Zig: Fast, but limited by `std.http` capabilities
- Rust: Fast with more features (connection pooling, better TLS)

The performance differences are negligible for this use case. The real benefits are in functionality and maintainability.

## Moving Forward

The Rust version of HTTP File Runner is now the primary implementation and receives all active development. Future enhancements include:

- **Request timeout configuration**: Per-request and global timeout settings
- **Response body filtering**: JSONPath queries and XML parsing
- **Parallel execution**: Concurrent processing of non-chained requests for faster test suites
- **Enhanced reporting**: JSON, XML, and HTML output formats

## Conclusion

Rewriting HTTP File Runner from Zig to Rust was driven by pragmatic necessity rather than preference. The inability to configure TLS certificate validation in Zig's standard library was a blocking issue for a serious HTTP testing tool. While I enjoyed working with Zig and appreciated its design philosophy, the project needed the functionality and ecosystem maturity that Rust provides.

The migration maintained 100% feature parity while adding the critical capability to work with self-signed certificates in development environments. The Rust ecosystem's mature libraries for HTTP, CLI parsing, and error handling made the rewrite straightforward and resulted in more maintainable code.

If you're using the Zig version, I encourage you to try the Rust version:

```bash
cargo install httprunner
```

The tool remains fast, small, and cross-platform—but now it actually works in all the scenarios it needs to support. You can read more about the original Zig implementation in my [previous post](/2025/06/http-file-runner).

For more details on the migration, check out:

- [Pull Request #43](https://github.com/christianhelle/httprunner/pull/43) - Complete migration details
- [HTTP File Runner repository](https://github.com/christianhelle/httprunner) - Rust implementation
- [HTTP File Runner (Zig)](https://github.com/christianhelle/httprunner-zig) - Original implementation
- [Documentation](https://christianhelle.com/httprunner/) - Full user guide

The project continues to evolve, and I'm excited about the possibilities that Rust's ecosystem enables for future enhancements.
