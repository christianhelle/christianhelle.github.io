---
layout: post
title: Porting HTTP File Generator from a .NET Tool to a Rust CLI
date: 2026-05-13
author: Christian Helle
tags:
  - Rust
  - .NET
  - OpenAPI
  - Migration
redirect_from:
  - /2026/05/porting-http-generator-to-rust
  - /2026/05/porting-http-generator-to-rust/
  - /2026/porting-http-generator-to-rust/
  - /2026/porting-http-generator-to-rust
---

A few years ago, I wrote about [HTTP File Generator](/2023/11/http-file-generator), a tool I built to generate `.http` files from OpenAPI specifications. It started as a [.NET Tool](https://github.com/christianhelle/httpgenerator) distributed via NuGet, and it served me well. Over time, I've been increasingly drawn to Rust for command-line tools — first with [HTTP File Runner](/2025/06/azure-devops-cli.html), then with [Azure DevOps CLI](/2025/06/azure-devops-cli.html), and more recently I [rewrote HTTP File Runner from Zig to Rust](/2025/10/httprunner-zig-to-rust-rewrite.html).

The pattern became clear: for CLI tools, Rust offers the best combination of performance, zero dependencies, and a mature ecosystem. So I decided to port HTTP File Generator to Rust as well.

## Why Port?

The original .NET tool works fine, but it comes with implicit costs:

- **Runtime dependency**: Users need .NET 8.0 installed to run the tool
- **Startup time**: The .NET runtime adds noticeable cold-start latency
- **Binary size**: The self-contained publish produces a ~70MB folder with the runtime
- **Distribution friction**: `dotnet tool install --global` is reliable but not as universally convenient as a single binary

The Rust version addresses all of these. The resulting binary is under 5MB, starts instantly, and requires zero runtime. But this wasn't about abandoning .NET — it's about giving users the best option for a CLI tool. The .NET version remains available, but Rust is now the primary implementation going forward.

## Project Structure

The Rust version is organized as a Cargo workspace with two crates:

```
src/
├── dotnet/          # Legacy .NET tool (still maintained)
│   ├── HttpGenerator/       # CLI app (Spectre.Console.Cli)
│   ├── HttpGenerator.Core/  # Core generator library
│   └── HttpGenerator.VSIX/  # Visual Studio extension
└── rust/            # New Rust CLI
    ├── cli/               # CLI binary (clap)
    └── core/              # Core library (openapiv3 + openapiv3_1)
```

The workspace root `Cargo.toml` ties them together:

```toml
[workspace]
members = [
  "src/rust/core",
  "src/rust/cli",
]
resolver = "2"

[workspace.package]
authors = ["Christian Helle"]
edition = "2024"
rust-version = "1.95"
version = "0.1.0"

[workspace.dependencies]
azure_core = "0.35.0"
azure_identity = "0.35.0"
clap = { version = "4.6.1", features = ["derive"] }
httpgenerator-core = { version = "0.1.0", path = "src/rust/core" }
openapiv3 = "2.2.0"
openapiv3_1 = "0.1.5"
reqwest = { version = "0.12.28", default-features = false, features = [
  "blocking",
  "gzip",
  "rustls-tls",
] }
serde = { version = "1.0.228", features = ["derive"] }
serde_json = { version = "1.0.145", features = ["preserve_order"] }
```

## CLI Framework: Spectre.Console.Cli vs clap

The original .NET tool uses [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console) for CLI argument parsing and rich console output. The Rust version uses [clap](https://clap.rs/) with derive macros.

### .NET Command Settings

The .NET version defines CLI options using attributes on a `CommandSettings` class:

```csharp
public class Settings : CommandSettings
{
    [CommandArgument(0, "[URL or input file]")]
    public string OpenApiPath { get; set; } = null!;

    [CommandOption("-o|--output <OUTPUT>")]
    [DefaultValue("./")]
    public string OutputFolder { get; set; } = "./";

    [CommandOption("--skip-validation")]
    [DefaultValue(false)]
    public bool SkipValidation { get; set; }

    [CommandOption("--authorization-header <HEADER>")]
    public string? AuthorizationHeader { get; set; }

    [CommandOption("--load-authorization-header-from-environment")]
    public bool AuthorizationHeaderFromEnvironmentVariable { get; set; }

    [CommandOption("--authorization-header-variable-name <VARIABLE-NAME>")]
    [DefaultValue("authorization")]
    public string AuthorizationHeaderVariableName { get; set; } = "authorization";

    [CommandOption("--content-type <CONTENT-TYPE>")]
    [DefaultValue("application/json")]
    public string ContentType { get; set; } = "application/json";

    [CommandOption("--base-url <BASE-URL>")]
    public string? BaseUrl { get; set; }

    [CommandOption("--output-type <OUTPUT-TYPE>")]
    [DefaultValue(OutputType.OneRequestPerFile)]
    public OutputType OutputType { get; set; } = OutputType.OneRequestPerFile;

    [CommandOption("--azure-scope <SCOPE>")]
    public string? AzureScope { get; set; }

    [CommandOption("--azure-tenant-id <TENANT-ID>")]
    public string? AzureTenantId { get; set; }

    [CommandOption("--timeout <SECONDS>")]
    [DefaultValue(120)]
    public int Timeout { get; set; } = 120;

    [CommandOption("--generate-intellij-tests")]
    public bool GenerateIntelliJTests { get; set; }

    [CommandOption("--custom-header")]
    public string[]? CustomHeaders { get; set; }

    [CommandOption("--skip-headers")]
    public bool SkipHeaders { get; set; }
}
```

### Rust CLI Args with clap

The Rust equivalent uses derive macros, which is significantly more concise:

```rust
#[derive(Debug, Clone, Parser, PartialEq, Eq)]
#[command(
    name = "httpgenerator",
    bin_name = "httpgenerator",
    version,
    about = "Generate .http files from OpenAPI specifications",
    disable_help_flag = true,
    disable_version_flag = true
)]
pub struct CliArgs {
    #[arg(
        value_name = "URL or input file",
        help = "URL or file path to OpenAPI Specification file"
    )]
    pub open_api_path: Option<String>,

    #[arg(
        short = 'o',
        long = "output",
        value_name = "OUTPUT",
        default_value = "./",
        help = "Output directory"
    )]
    pub output_folder: String,

    #[arg(
        long = "skip-validation",
        default_value_t = false,
        help = "Skip validation of OpenAPI Specification file"
    )]
    pub skip_validation: bool,

    #[arg(
        long = "authorization-header",
        value_name = "HEADER",
        help = "Authorization header to use for all requests"
    )]
    pub authorization_header: Option<String>,

    #[arg(
        long = "content-type",
        value_name = "CONTENT-TYPE",
        default_value = "application/json",
        help = "Default Content-Type header to use for all requests"
    )]
    pub content_type: String,

    #[arg(
        long = "base-url",
        value_name = "BASE-URL",
        help = "Default Base URL to use for all requests"
    )]
    pub base_url: Option<String>,

    #[arg(
        long = "output-type",
        value_name = "OUTPUT-TYPE",
        default_value_t = OutputTypeArg::OneRequestPerFile,
        ignore_case = true,
        help = "OneRequestPerFile generates one .http file per request. OneFile generates a single .http file for all requests."
    )]
    pub output_type: OutputTypeArg,

    #[arg(
        long = "azure-scope",
        value_name = "SCOPE",
        help = "Azure Entra ID Scope to use for retrieving Access Token"
    )]
    pub azure_scope: Option<String>,

    #[arg(
        long = "azure-tenant-id",
        value_name = "TENANT-ID",
        help = "Azure Entra ID Tenant ID to use for retrieving Access Token"
    )]
    pub azure_tenant_id: Option<String>,

    #[arg(
        long = "generate-intellij-tests",
        default_value_t = false,
        help = "Generate IntelliJ tests that assert whether the response status code is 200"
    )]
    pub generate_intellij_tests: bool,

    #[arg(
        long = "custom-header",
        value_name = "HEADER",
        help = "Add custom HTTP headers to the generated request"
    )]
    pub custom_headers: Vec<String>,
}
```

The clap derive approach is notably more compact. The help text is auto-generated from the `#[arg(help = "...")]` attributes, and default values are specified inline with `default_value` or `default_value_t`.

## Console Output: Spectre.Console vs Manual Rendering

The .NET version uses Spectre.Console for rich console output with colored panels, tables, and rules. The Rust version renders console output manually with conditional rich/plain modes based on terminal detection.

### .NET Rich Console Output

```csharp
private static void DisplayHeader(Settings settings)
{
    var version = typeof(GenerateCommand).Assembly.GetName().Version!;

    var panel = new Panel(new Markup($"[bold blue]🚀 HTTP File Generator[/] [dim]v{version}[/]"))
    {
        Border = BoxBorder.Rounded,
        BorderStyle = new Style(Color.Blue),
        Padding = new Padding(1, 0, 1, 0)
    };

    AnsiConsole.Write(panel);

    var supportKey = settings.NoLogging
        ? "[yellow]⚠️  Unavailable when logging is disabled[/]"
        : $"[green]🔑 {SupportInformation.GetSupportKey()}[/]";

    AnsiConsole.MarkupLine($"Support key: {supportKey}");
    AnsiConsole.WriteLine();
}

private static void DisplayOpenApiStatistics(OpenApiStats statistics)
{
    var table = new Table()
    {
        Border = TableBorder.Rounded,
        BorderStyle = new Style(Color.Green)
    };

    table.AddColumn(new TableColumn("[bold]📊 OpenAPI Statistics[/]").LeftAligned());
    table.AddColumn(new TableColumn("[bold]Count[/]").LeftAligned());

    table.AddRow("📝 Path Items", $"[cyan]{statistics.PathItemCount}[/]");
    table.AddRow("⚡ Operations", $"[cyan]{statistics.OperationCount}[/]");
    table.AddRow("📝 Parameters", $"[cyan]{statistics.ParameterCount}[/]");
    table.AddRow("📤 Request Bodies", $"[cyan]{statistics.RequestBodyCount}[/]");
    table.AddRow("📥 Responses", $"[cyan]{statistics.ResponseCount}[/]");
    table.AddRow("🔗 Links", $"[cyan]{statistics.LinkCount}[/]");
    table.AddRow("📞 Callbacks", $"[cyan]{statistics.CallbackCount}[/]");
    table.AddRow("📋 Schemas", $"[cyan]{statistics.SchemaCount}[/]");

    AnsiConsole.Write(table);
}
```

### Rust Conditional Rich/Plain Output

The Rust version detects whether stdout/stderr are terminals and switches between rich (Unicode box-drawing characters, emojis) and plain (ASCII-safe) modes:

```rust
pub(crate) struct CliPresenter {
    stdout_mode: PresentationMode,
    stderr_mode: PresentationMode,
}

impl CliPresenter {
    pub(crate) fn detect() -> Self {
        Self::new(
            mode_from_terminal(io::stdout().is_terminal()),
            mode_from_terminal(io::stderr().is_terminal()),
        )
    }

    pub(crate) fn print_header(&mut self, no_logging: bool) {
        self.write_stdout(&render_header(self.stdout_mode, no_logging));
    }

    pub(crate) fn print_success(&mut self, duration: Duration) {
        self.write_stdout(&render_success(self.stdout_mode, duration));
    }

    pub(crate) fn print_error(&mut self, error: &CliError) {
        self.write_stderr(&render_error(self.stderr_mode, error));
    }
}
```

The rich mode uses Unicode characters like `╭`, `─`, `╮` for panel borders, while plain mode falls back to simple ASCII. This ensures the tool works correctly when piped to files or used in CI/CD environments where Unicode might not be supported.

## OpenAPI Parsing: Microsoft.OpenApi vs openapiv3 crates

The most significant architectural change is how OpenAPI documents are parsed. The .NET version uses Microsoft's official `Microsoft.OpenApi` library, while the Rust version uses two crates: `openapiv3` for OpenAPI 3.0.x and `openapiv3_1` for OpenAPI 3.1.x.

### .NET OpenAPI Document Creation

```csharp
public static async Task<GeneratorResult> Generate(GeneratorSettings settings)
{
    var document = await OpenApiDocumentFactory.CreateAsync(settings.OpenApiPath);
    var operationNameGenerator = new OperationNameGenerator();

    var serverUrl = document.Servers?.FirstOrDefault()?.Url ?? string.Empty;
    var baseUrl = settings.BaseUrl ?? string.Empty;

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = serverUrl;
    }
    else
    {
        if (!Uri.IsWellFormedUriString(serverUrl, UriKind.Absolute))
            baseUrl += serverUrl;
    }

    return settings.OutputType switch
    {
        OutputType.OneRequestPerFile => GenerateMultipleFiles(
            settings, document, baseUrl, operationNameGenerator),
        OutputType.OneFile => GenerateSingleFile(
            settings, document, operationNameGenerator, baseUrl),
        OutputType.OneFilePerTag => GenerateFilePerTag(
            settings, document, baseUrl, operationNameGenerator),
        _ => throw new ArgumentOutOfRangeException(nameof(settings.OutputType))
    };
}
```

### Rust OpenAPI Document Loading and Normalization

The Rust version separates document loading from normalization, which is a more modular approach:

```rust
pub fn generate_http_files(
    settings: &GeneratorSettings,
    document: &NormalizedOpenApiDocument,
) -> GeneratorResult {
    let server_url = document
        .servers
        .first()
        .map(|server| server.url.as_str())
        .unwrap_or_default();

    let base_url = resolve_base_url(
        &settings.open_api_path,
        Some(server_url),
        settings.base_url.as_deref(),
    );

    match settings.output_type {
        OutputType::OneRequestPerFile => generate_multiple_files(settings, document, &base_url),
        OutputType::OneFile => generate_single_file(settings, document, &base_url),
        OutputType::OneFilePerTag => generate_file_per_tag(settings, document, &base_url),
    }
}
```

The `NormalizedOpenApiDocument` type is the result of a normalization pipeline that handles both OpenAPI 3.0 and 3.1 differences, `$ref` resolution, and schema composition (`allOf`, `oneOf`, `anyOf`). This normalization layer is one of the most valuable parts of the Rust implementation — it provides a unified API for the generator regardless of which OpenAPI version was used as input.

## HTTP Request Rendering

The core of the tool is generating `.http` files. The logic is essentially the same in both implementations, but the Rust version uses a more composable approach with separate modules for rendering, sampling, and text handling.

### .NET Request Generation

```csharp
private static string GenerateRequest(
    OpenApiDocument document,
    KeyValuePair<string, IOpenApiPathItem> operationPath,
    IOperationNameGenerator operationNameGenerator,
    GeneratorSettings settings,
    string verb,
    OpenApiOperation operation)
{
    var code = new StringBuilder();
    AppendSummary(verb, operationPath, operation, code);

    var parameterNameMap = AppendParameters(
        document, operationPath, settings, operation, verb,
        operationNameGenerator, code);

    var url = operationPath.Key.Replace("{", "{{").Replace("}", "}}");

    var pathParams = new List<KeyValuePair<string, string>>();
    var queryParams = new List<KeyValuePair<string, string>>();

    foreach (var param in parameterNameMap)
    {
        if (operationPath.Key.Contains($"{{{param.Key}}}"))
            pathParams.Add(param);
        else
            queryParams.Add(param);
    }

    foreach (var pathParam in pathParams)
    {
        url = url.Replace($"{{{{{pathParam.Key}}}}}", $"{{{{{pathParam.Value}}}}}");
    }

    if (queryParams.Count > 0)
    {
        url += "?" + string.Join("&", queryParams.Select(p => $"{p.Key}={{{{{p.Value}}}}}"));
    }

    code.AppendLine($"{verb.ToUpperInvariant()} {{{{baseUrl}}}}{url}");
    code.AppendLine("Content-Type: {{contentType}}");

    // ... authorization and custom headers ...

    var contentType = operation.RequestBody?.Content?.Keys
        ?.FirstOrDefault(c => c.Contains(settings.ContentType));

    code.AppendLine();
    if (operation.RequestBody?.Content is null || contentType is null)
        return code.ToString();

    var requestBody = operation.RequestBody;
    var requestBodySchema = requestBody.Content[contentType].Schema;
    var requestBodyJson = GenerateSampleJson(requestBodySchema) ?? string.Empty;

    code.AppendLine(requestBodyJson);
    GenerateIntelliJTest(settings, code);

    return code.ToString();
}
```

### Rust Request Rendering

```rust
pub(super) fn render_request(
    settings: &GeneratorSettings,
    operation: &NormalizedOperation,
) -> String {
    let mut content = String::new();
    append_summary(operation, &mut content);
    let parameter_name_map = append_parameters(settings, operation, &mut content);

    let mut url = operation.path.replace('{', "{{").replace('}', "}}");
    let mut query_parameters = Vec::new();

    for (original_name, generated_name) in &parameter_name_map {
        if operation.path.contains(&format!("{{{original_name}}}")) {
            url = url.replace(
                &format!("{{{{{original_name}}}}}"),
                &format!("{{{{{generated_name}}}}}"),
            );
        } else {
            query_parameters.push((original_name, generated_name));
        }
    }

    if !query_parameters.is_empty() {
        url.push('?');
        url.push_str(
            &query_parameters
                .iter()
                .map(|(name, generated)| format!("{name}={{{{{generated}}}}}"))
                .collect::<Vec<_>>()
                .join("&"),
        );
    }

    push_line(&mut content, &format!(
        "{} {{{{baseUrl}}}}{url}",
        operation.method.as_str().to_ascii_uppercase()
    ));
    push_line(&mut content, "Content-Type: {{contentType}}");

    // ... authorization and custom headers ...

    let request_body = match &operation.request_body {
        Some(NormalizedRequestBody::Inline(request_body)) => request_body,
        _ => {
            generate_intellij_test(settings, &mut content);
            return content;
        }
    };

    let Some(media_type) = request_body.content.iter().find(|c| c.content_type.contains(&settings.content_type))
    else {
        generate_intellij_test(settings, &mut content);
        return content;
    };

    let Some(schema) = media_type.schema.as_ref() else {
        generate_intellij_test(settings, &mut content);
        return content;
    };

    content.push_str(&generate_sample_json(schema));
    push_line(&mut content, "");
    generate_intellij_test(settings, &mut content);
    content
}
```

The logic is functionally identical. The Rust version uses a `push_line` helper instead of `StringBuilder.AppendLine`, and `String` directly instead of `StringBuilder`, which is more idiomatic Rust. The early returns in the Rust version make the happy path clearer.

## Azure Authentication

Both versions support Azure Entra ID authentication for generating `.http` files with proper authorization headers. The .NET version uses `Microsoft.Azure.Identity` while the Rust version uses `azure_core` and `azure_identity` crates.

### .NET Azure Authentication

```csharp
private static async Task AcquireAzureEntraIdToken(Settings settings)
{
    if (!string.IsNullOrWhiteSpace(settings.AuthorizationHeader) ||
        (string.IsNullOrWhiteSpace(settings.AzureScope) &&
         string.IsNullOrWhiteSpace(settings.AzureTenantId)))
        return;

    AnsiConsole.MarkupLine($"[cyan]🔐 Acquiring authorization header from Azure Entra ID...[/]");
    using var listener = AzureEventSourceListener.CreateConsoleLogger();
    var token = await AzureEntraID
        .TryGetAccessTokenAsync(
            settings.AzureTenantId!,
            settings.AzureScope!,
            CancellationToken.None);

    if (!string.IsNullOrWhiteSpace(token))
    {
        settings.AuthorizationHeader = $"Bearer {token}";
        AnsiConsole.MarkupLine($"[green]✅ Successfully acquired access token[/]{Crlf}");
    }
}
```

### Rust Azure Authentication

The Rust version tries multiple credential providers in sequence, which is a nice improvement:

```rust
pub fn try_get_access_token(
    tenant_id: Option<&str>,
    scope: &str,
) -> Result<Option<String>, String> {
    let scope = scope.trim();
    if scope.is_empty() {
        return Ok(None);
    }

    let mut errors = Vec::new();

    match get_token_with_azure_cli(tenant_id, scope) {
        Ok(token) => return Ok(Some(token)),
        Err(error) => errors.push(error),
    }

    match get_token_with_azure_developer_cli(tenant_id, scope) {
        Ok(token) => return Ok(Some(token)),
        Err(error) => errors.push(error),
    }

    Err(errors.join("\n"))
}
```

This is a meaningful improvement over the .NET version. Instead of relying on a single credential provider, the Rust version tries both Azure CLI and Azure Developer CLI credentials, falling back to the next if one fails. This means the tool works regardless of which Azure tooling the user has installed.

## Build System

### .NET Build

```bash
dotnet build --configuration Debug src/dotnet/HttpGenerator.slnx
dotnet build --configuration Release src/dotnet/HttpGenerator.slnx
dotnet test src/dotnet/HttpGenerator.slnx
dotnet pack src/dotnet/HttpGenerator.Core/HttpGenerator.Core.csproj
dotnet pack src/dotnet/HttpGenerator/HttpGenerator.csproj
```

### Rust Build

```bash
cargo build --workspace
cargo build --release --workspace
cargo test --workspace
cargo publish --dry-run --allow-dirty
```

### Unified Makefile

Both build systems are unified in a single Makefile:

```makefile
all: build

build:
	dotnet build --configuration Debug src/dotnet/HttpGenerator.slnx
	cargo build --workspace

release:
	dotnet build --configuration Release src/dotnet/HttpGenerator.slnx
	cargo build --release --workspace

test:
	dotnet test --configuration Debug src/dotnet/HttpGenerator.slnx
	cargo test --workspace

publish:
	dotnet pack --no-build src/dotnet/HttpGenerator.Core/HttpGenerator.Core.csproj
	dotnet pack --no-build src/dotnet/HttpGenerator/HttpGenerator.csproj
	cargo publish --dry-run --allow-dirty

clean:
	dotnet clean src/dotnet/HttpGenerator.slnx
	cargo clean
```

## Distribution

### .NET Tool Installation

```bash
dotnet tool install --global httpgenerator
dotnet tool update --global httpgenerator
```

### Rust CLI Installation

```bash
# From crates.io (requires Rust)
cargo install httpgenerator

# From GitHub Releases (no dependencies)
# Download the appropriate binary for your platform

# From Snap Store
snap install httpgenerator
```

The Rust version adds `cargo install` as a new installation option, which is particularly convenient for Rust developers. The GitHub Releases and Snap installation methods provide zero-dependency alternatives that work everywhere.

## Feature Parity

All features from the .NET version are implemented in the Rust version:

| Feature | .NET | Rust |
| --- | --- | --- |
| Parse local OpenAPI files | ✅ | ✅ |
| Fetch OpenAPI from URL | ✅ | ✅ |
| OpenAPI 3.0.x support | ✅ | ✅ |
| OpenAPI 3.1.x support | ✅ | ✅ |
| Swagger 2.0 (auto-convert) | ✅ | ✅ |
| Output types (one file, per request, per tag) | ✅ | ✅ |
| Azure Entra ID authentication | ✅ | ✅ |
| Custom authorization headers | ✅ | ✅ |
| Environment variable auth loading | ✅ | ✅ |
| Custom headers support | ✅ | ✅ |
| Skip headers generation | ✅ | ✅ |
| IntelliJ test generation | ✅ | ✅ |
| OpenAPI validation | ✅ | ✅ |
| Skip validation | ✅ | ✅ |
| Base URL override | ✅ | ✅ |
| Content-Type customization | ✅ | ✅ |
| Timeout configuration | ✅ | ✅ |
| Support key generation | ✅ | ✅ |
| Colored terminal output | ✅ | ✅ |
| Rich/plain mode detection | ❌ | ✅ |
| Telemetry | ✅ | ✅ |

## Rust-Specific Improvements

### Rich/Plain Mode Detection

The Rust version automatically detects whether output is going to a terminal or being piped to a file, and switches between rich Unicode rendering and plain ASCII. This is something the .NET version doesn't do — it always uses Spectre.Console's rich rendering, which can produce garbled output when piped.

```rust
pub(crate) fn detect() -> Self {
    Self::new(
        mode_from_terminal(io::stdout().is_terminal()),
        mode_from_terminal(io::stderr().is_terminal()),
    )
}
```

### OpenAPI 3.1 Support

The Rust version uses `openapiv3_1` for OpenAPI 3.1.x documents, which handles the format differences from 3.0.x properly. The normalization layer in the core crate unifies both formats into a single internal representation.

### Swagger 2.0 Support

The Rust version includes automatic Swagger 2.0 (OpenAPI 2.0) to OpenAPI 3.x conversion, which is a nice convenience for users who still work with older API specifications.

### Multiple Azure Credential Providers

As mentioned earlier, the Rust version tries both Azure CLI and Azure Developer CLI for authentication, whereas the .NET version relies on `DefaultAzureCredential` which has its own chain but is less transparent about what's being tried.

## What Stays in .NET

The Visual Studio extension (`HttpGenerator.VSIX`) remains a .NET project. This makes sense because VS extensions are inherently tied to the .NET/Visual Studio ecosystem. The VSIX project provides the same functionality as the CLI but integrated directly into Visual Studio's menu system.

## Lessons Learned

### Workspace Structure

Organizing the Rust code as a workspace with separate `cli` and `core` crates was a good decision. The `core` crate is a pure library with no CLI dependencies, making it testable and reusable. The `cli` crate depends on `core` and adds the CLI-specific concerns (argument parsing, console output, telemetry).

### Error Handling

Rust's `Result<T, E>` type and the `?` operator make error handling more explicit than .NET's exception-based approach. In the .NET version, errors flow through try/catch blocks in `GenerateCommand.ExecuteAsync`. In the Rust version, errors propagate through the call chain with context attached at each layer.

### No Hidden Dependencies

One of the most satisfying aspects of the Rust version is that there are no hidden dependencies. When you run `cargo build`, you know exactly what's being compiled. The .NET version, while having a well-defined project file, still pulls in the entire .NET runtime at publish time, which can be surprising for users who expect a single binary.

### The .NET Legacy

The .NET version isn't going anywhere — it's still a perfectly functional tool. But for a CLI tool that users run from the terminal, the Rust version offers a better experience: faster startup, smaller download, no runtime installation required. The .NET version remains available for users who prefer it or who need the Visual Studio extension.

## Moving Forward

The Rust implementation is now the primary version of HTTP File Generator. Future work includes:

- **Enhanced OpenAPI normalization**: Better handling of edge cases in both 3.0 and 3.1 specs
- **Improved sample JSON generation**: More realistic sample data based on schema constraints
- **Better error messages**: More specific error messages for common OpenAPI issues
- **Performance improvements**: Faster processing for large OpenAPI documents

If you're currently using the .NET tool, I'd encourage you to try the Rust version:

```bash
cargo install httpgenerator
```

Or download a pre-built binary from [GitHub Releases](https://github.com/christianhelle/httpgenerator/releases). The CLI interface is identical, so your existing scripts and workflows will work without modification.

For more on my Rust and Zig tools, see [HTTP File Runner](/2025/06/http-file-runner.html), the [Zig to Rust rewrite](/2025/10/httprunner-zig-to-rust-rewrite.html), [Azure DevOps CLI](/2025/06/azure-devops-cli.html), [chlogr](/2025/11/building-a-github-changelog-generator-in-zig.html), and [clocz](/2026/02/building-clocz-zig-line-counter.html).
