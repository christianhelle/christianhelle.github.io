---
layout: post
title: Porting HTTP File Generator from a .NET Tool to a Rust CLI
date: 2026-05-12
author: Christian Helle
tags:
  - Rust
  - .NET
  - OpenAPI
  - Migration
redirect_from:
  - 2026/05/http-file-generator-dotnet-tool-to-rust-cli
  - 2026/05/http-file-generator-dotnet-tool-to-rust-cli/
  - 2026/http-file-generator-dotnet-tool-to-rust-cli
  - 2026/http-file-generator-dotnet-tool-to-rust-cli/
  - http-file-generator-dotnet-tool-to-rust-cli
  - http-file-generator-dotnet-tool-to-rust-cli/
---

Back in 2023, I wrote about [HTTP File Generator](/2023/11/generate-http-files-from-openapi-spec/), a command-line tool I built to generate `.http` files from OpenAPI specifications. At the time, it shipped as a [.NET Tool on NuGet](https://www.nuget.org/packages/HttpGenerator), and that version served me really well. I use this tool all the time when working with APIs because I much prefer staying inside my editor with `.http` files over bouncing back and forth to Swagger UI.

Over the past few weeks, I completed a substantial port of the project to Rust and promoted the Rust CLI to the primary release path. This was not a small rewrite and it was definitely not a flag day migration where I deleted the old implementation and hoped for the best. I kept the legacy .NET implementation in the repository, used it as a migration oracle, and moved the project forward in phases.

The result is a tool that is 15x faster, ships as a natively compiled single binary CLI app, easier to install across platforms, and much more explicit internally about how it parses OpenAPI documents and turns them into `.http` files.

In this post, I want to walk through what changed, why I did it, and show some of the before and after code.

## The migration in numbers

The Rust port landed across four main pull requests:

- [PR #374 - Experimental CLI app re-write in Rust](https://github.com/christianhelle/httpgenerator/pull/374): 55 commits, 13,384 additions, 1,698 deletions, 173 changed files
- [PR #380 - Publish Rust app to crates.io](https://github.com/christianhelle/httpgenerator/pull/380): 12 commits, 895 additions, 42 deletions, 35 changed files
- [PR #389 - Restructure Rust codebase](https://github.com/christianhelle/httpgenerator/pull/389): 16 commits, 13,408 additions, 5,121 deletions, 152 changed files
- [PR #390 - Add installer scripts and release assets](https://github.com/christianhelle/httpgenerator/pull/390): 7 commits, 704 additions, 11 deletions, 9 changed files

Taken together, these four pull requests alone account for **90 commits**, **28,391 additions**, and **6,872 deletions**. That is a lot of change for a CLI tool, but it was also the right scale of change. The port touched the command-line surface, the OpenAPI loading pipeline, the generation engine, packaging, release automation, and the installation story.

## Why rewrite a tool that already worked?

The original .NET Tool was useful, stable, and already integrated into how I work. So the motivation here was not "I wanted to rewrite something in Rust". The motivation was more pragmatic than that.

I wanted HTTP File Generator to behave like a native CLI first and foremost. I wanted it to install cleanly without assuming a .NET SDK or runtime setup. I wanted the distribution story to be first class on Linux, macOS, and Windows. I also wanted the internals to have a cleaner separation between parsing, normalization, rendering, and file output.

The old implementation evolved organically and carried a lot of responsibility inside one core generator class. The Rust rewrite gave me a chance to reorganize the project around explicit modules and clearer data flow while still preserving behavior.

Just as important, I did not want to lose the working .NET implementation. The repository still contains the legacy CLI and supporting code under `src/dotnet`, which made it possible to compare behavior while porting. That turned out to be one of the best decisions in the whole migration.

## From .NET Tool to native CLI distribution

The most visible change is how the tool is installed.

**Before**, the primary installation path was NuGet:

```bash
dotnet tool install --global httpgenerator
```

That was simple if you already lived in the .NET ecosystem, but it also made the tool feel like a .NET developer utility even though the actual output is useful to anyone working with HTTP APIs.

**After**, the Rust CLI has multiple native install paths:

```bash
cargo install httpgenerator
```

```bash
curl -fsSL https://christianhelle.com/httpgenerator/install | bash
```

```powershell
irm https://christianhelle.com/httpgenerator/install.ps1 | iex
```

That is a big difference in practice. The Rust CLI now ships via [crates.io](https://crates.io/crates/httpgenerator), as standalone archives on [GitHub Releases](https://github.com/christianhelle/httpgenerator/releases), and through platform-specific installer scripts. The .NET Tool still exists as a legacy compatibility path, but it is no longer the center of gravity.

The release workflow also reflects that shift. Instead of publishing one NuGet-delivered tool surface, the release pipeline now builds native artifacts for multiple targets:

```yaml
strategy:
  fail-fast: false
  matrix:
    include:
      - name: Rust CLI (Linux)
        os: ubuntu-latest
        target: x86_64-unknown-linux-gnu
        archive_suffix: linux-x64.tar.gz
      - name: Rust CLI (macOS x64)
        os: macos-15-intel
        target: x86_64-apple-darwin
        archive_suffix: darwin-x64.tar.gz
      - name: Rust CLI (macOS ARM64)
        os: macos-15
        target: aarch64-apple-darwin
        archive_suffix: darwin-arm64.tar.gz
      - name: Rust CLI (Windows)
        os: windows-latest
        target: x86_64-pc-windows-msvc
        archive_suffix: win-x64.zip
```

That is exactly the kind of distribution story I wanted for this tool. It is no longer "a .NET Tool that happens to generate `.http` files". It is now simply **HTTP File Generator**, with a native CLI as the main product.

## The CLI moved from Spectre.Console.Cli to clap

One of the nicest parts of the original .NET version was the CLI surface. I spent a fair bit of time on the help output and examples, and I wanted to preserve that.

In the .NET version, the entry point used `Spectre.Console.Cli` and configured the command app imperatively:

```csharp
CommandApp<GenerateCommand> app = new();
app.Configure(
    configuration =>
    {
        configuration
            .SetApplicationName("httpgenerator")
            .SetApplicationVersion(typeof(GenerateCommand).Assembly.GetName().Version!.ToString());

        configuration.AddExample(InputFilename);
        configuration.AddExample(InputFilename, "--output", "./");
        configuration.AddExample(InputFilename, "--output-type", "onefile");
        configuration.AddExample("https://petstore.swagger.io/v2/swagger.json");
    });

return app.Run(args);
```

The Rust version uses `clap` with a declarative argument model:

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
```

This is one of those changes that looks smaller than it really is. The old approach built up the command-line surface through a series of runtime configuration calls. The Rust approach makes the CLI contract live directly in the type system. Options, defaults, help text, and value parsing are all attached to the `CliArgs` struct.

That has a few very real benefits:

- It is easier to see the entire CLI contract in one place
- It is easier to test the parser in isolation
- It is easier to evolve flags without threading configuration through a separate builder API
- It keeps the generated help output tightly coupled to the actual argument model

The Rust CLI still preserves the familiar public command identity and most of the option surface, which was important. I wanted the port to feel like the same tool, not a brand new tool with a borrowed name.

## OpenAPI loading became much more explicit

One of the biggest internal differences is how the two implementations load and classify OpenAPI documents.

In the original .NET implementation, the loading layer was intentionally very thin:

```csharp
public static async Task<OpenApiDocument> CreateAsync(string openApiPath)
{
    var result = await OpenApiMultiFileReader.Read(openApiPath);
    return result.OpenApiDocument;
}
```

There is nothing wrong with that. It is short, clear, and it delegates the heavy lifting to the Microsoft OpenAPI stack. But it also hides a lot of nuance. Once you start dealing with Swagger 2.0, OpenAPI 3.0, OpenAPI 3.1, webhook-only specs, and edge cases in the wild, that simplicity becomes a little too opaque.

The Rust version makes the supported document shapes explicit up front:

```rust
pub enum LoadedOpenApiDocument {
    Swagger2 {
        raw: RawOpenApiDocument,
    },
    OpenApi30 {
        raw: RawOpenApiDocument,
        document: openapiv3::OpenAPI,
    },
    OpenApi31 {
        raw: RawOpenApiDocument,
        document: openapiv3_1::OpenApi,
    },
    OpenApi31Raw {
        raw: RawOpenApiDocument,
    },
}

pub fn load_document(input: &str) -> Result<LoadedOpenApiDocument, OpenApiDocumentLoadError> {
    load_document_with_options(input, false)
}
```

And the loader is deliberate about fallback behavior for OpenAPI 3.1 edge cases:

```rust
Err(TypedOpenApiParseError::Deserialize {
    version: OpenApiSpecificationVersion::OpenApi31,
    ..
}) if should_fallback_to_raw_openapi31(&raw, tolerate_invalid_openapi31) => {
    Ok(LoadedOpenApiDocument::OpenApi31Raw { raw })
}
```

That is a more robust design for a generator. Instead of pretending every document fits neatly into one object model, the Rust code distinguishes between what was loaded, what was strongly typed, and what had to be carried through as a raw 3.1 document. There are also targeted tests around these cases, including Swagger 2.0, OpenAPI 3.0, OpenAPI 3.1, webhook-only documents, and invalid-but-tolerated 3.1 documents.

This is one of the areas where the Rust version is not just a line-for-line port. It is a stronger model.

## Generation moved from a monolithic class to a clearer pipeline

The original `.NET` generator packed a lot of behavior into [`HttpFileGenerator`](https://github.com/christianhelle/httpgenerator/blob/main/src/dotnet/HttpGenerator.Core/HttpFileGenerator.cs). It loaded the document, resolved the base URL, switched output modes, generated request text, sampled request bodies, and assembled files.

The top-level branching looked like this:

{% raw %}
```csharp
return settings.OutputType switch
{
    OutputType.OneRequestPerFile => GenerateMultipleFiles(
        settings,
        document,
        baseUrl,
        operationNameGenerator),
    OutputType.OneFile => GenerateSingleFile(
        settings,
        document,
        operationNameGenerator,
        baseUrl),
    OutputType.OneFilePerTag => GenerateFilePerTag(
        settings,
        document,
        baseUrl,
        operationNameGenerator),
    _ => throw new ArgumentOutOfRangeException(
        nameof(settings.OutputType),
        $"Unknown output type: {settings.OutputType}")
};
```

And the request rendering path mixed URL construction, parameter emission, headers, request body generation, and IntelliJ test blocks in one place:

```csharp
code.AppendLine($"{verb.ToUpperInvariant()} {{{{baseUrl}}}}{url}");
code.AppendLine("Content-Type: {{contentType}}");

if (!string.IsNullOrWhiteSpace(settings.AuthorizationHeader) ||
    settings.AuthorizationHeaderFromEnvironmentVariable)
{
    code.AppendLine($"Authorization: {{{{{settings.AuthorizationHeaderVariableName}}}}}");
}
```
{% endraw %}

The Rust version splits these concerns more cleanly. At the top level, generation operates on a normalized model:

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

Then the actual request rendering happens in a dedicated renderer:

{% raw %}
```rust
push_line(
    &mut content,
    &format!(
        "{} {{{{baseUrl}}}}{url}",
        operation.method.as_str().to_ascii_uppercase()
    ),
);
push_line(&mut content, "Content-Type: {{contentType}}");

if settings.authorization_header_from_environment_variable
    || settings
        .authorization_header
        .as_deref()
        .is_some_and(|value| !value.trim().is_empty())
{
    push_line(
        &mut content,
        &format!(
            "Authorization: {{{{{}}}}}",
            settings.authorization_header_variable_name
        ),
    );
}
```
{% endraw %}

That might look superficially similar, and at the output level it should be similar, but the architecture is different in an important way. The Rust code is built around a normalized intermediate representation with types such as `NormalizedOpenApiDocument`, `NormalizedOperation`, `NormalizedParameter`, and `NormalizedSchema`. That gives the renderer a cleaner and more stable surface to work with.

This is exactly the kind of separation I wanted from the rewrite. The old code worked, but the Rust version is easier to reason about because normalization, output mode selection, rendering, and writing are not all tangled together.

## Writing files is now a first-class concern

The old Visual Studio extension wrote files with a `Task.WhenAll` over `File.WriteAllText`, and the original generator logic treated writing mostly as an afterthought once the in-memory files had been assembled.

The Rust CLI makes writing explicit, including timeout handling:

```rust
pub(crate) fn write_files(
    output_folder: &Path,
    files: Vec<HttpFile>,
    timeout_seconds: u64,
) -> Result<Vec<PathBuf>, CliError> {
    if !output_folder.exists() {
        fs::create_dir_all(output_folder).map_err(|error| CliError::CreateOutputDirectory {
            path: output_folder.to_path_buf(),
            reason: error.to_string(),
        })?;
    }

    let output_folder = output_folder.to_path_buf();
    let (sender, receiver) = mpsc::channel();

    thread::spawn({
        let output_folder = output_folder.clone();
        move || {
            let result = write_files_worker(&output_folder, files);
            let _ = sender.send(result);
        }
    });

    receiver
        .recv_timeout(Duration::from_secs(timeout_seconds))
```

This is a good example of the Rust port taking operational behavior seriously. Writing generated files to disk is part of the CLI contract, so it now has its own timeout switch and explicit error paths.

## Azure authentication became easier to consume from the CLI

One of the reasons I originally built HTTP File Generator was to make `.http` files fit better into my day-to-day Azure-heavy workflow. In the [2023 post](/2023/11/generate-http-files-from-openapi-spec/), I showed a PowerShell wrapper around Azure CLI token acquisition:

```powershell
az account get-access-token --scope [Some Application ID URI]/.default `
| ConvertFrom-Json `
| %{
    httpgenerator `
        https://localhost:5001/swagger/v1/swagger.json `
        --authorization-header ("Bearer " + $_.accessToken) `
        --base-url https://localhost:5001 `
        --output ./HttpFiles
}
```

That workflow still works, and it is still useful. But the Rust CLI now documents a much simpler path as a first-class feature:

```powershell
httpgenerator `
  https://api.example.com/swagger/v1/swagger.json `
  --azure-scope [Some Application ID URI]/.default `
  --base-url https://api.example.com `
  --output ./HttpFiles
```

Under the hood, the old .NET implementation used `Azure.Identity` with a chained credential:

```csharp
var request = new TokenRequestContext([scope], tenantId: tenantId);
var credentials = new ChainedTokenCredential(
    new AzureCliCredential(),
    new VisualStudioCredential(),
    new DefaultAzureCredential(
        new DefaultAzureCredentialOptions
        {
            ExcludeWorkloadIdentityCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeAzureCliCredential = true,
        }));

var token = await credentials.GetTokenAsync(request, cancellationToken);
return token.Token;
```

The Rust CLI makes the credential attempts more explicit and collects cleaner failures:

```rust
match get_token_with_azure_cli(tenant_id, scope) {
    Ok(token) => return Ok(Some(token)),
    Err(error) => errors.push(error),
}

match get_token_with_azure_developer_cli(tenant_id, scope) {
    Ok(token) => return Ok(Some(token)),
    Err(error) => errors.push(error),
}

Err(errors.join("\n"))
```

It also strips traceback noise before surfacing errors:

```rust
fn summarize_error(error: &str) -> String {
    let summary = error
        .split("Traceback")
        .next()
        .unwrap_or(error)
        .split("To troubleshoot")
        .next()
        .unwrap_or(error)
        .replace("Here is the traceback:", "")
        .replace('\r', " ");
```

This is a subtle but important quality-of-life improvement. I want the tool to help me get to `.http` files quickly. I do not want Azure authentication plumbing to be the part I have to babysit.

## The installer scripts are a real upgrade

The addition of installer scripts in [PR #390](https://github.com/christianhelle/httpgenerator/pull/390) is one of those changes that is easy to underestimate if you only look at the code diff.

The Bash installer detects the platform, downloads the correct release asset, verifies the archive structure, extracts the binary, installs it, and verifies the resulting installation:

```bash
detect_platform() {
    local os
    local arch
    os=$(uname -s | tr '[:upper:]' '[:lower:]')
    arch=$(uname -m)

    case "$os" in
        linux*)
            os="linux"
            ;;
        darwin*)
            os="darwin"
            ;;
    esac

    echo "${os}-${arch}"
}
```

The PowerShell installer does the equivalent work for Windows, including sensible default install locations and optional PATH management:

```powershell
function Get-DefaultInstallDir
{
  $candidates = @(
    "$env:LOCALAPPDATA\Programs\httpgenerator",
    "$env:USERPROFILE\.local\bin",
    "$env:USERPROFILE\bin"
  )

  foreach ($candidate in $candidates)
  {
    if (Test-Path $candidate -PathType Container)
    {
      return $candidate
    }
  }

  return "$env:LOCALAPPDATA\Programs\httpgenerator"
}
```

This matters because it turns the Rust CLI from "something Rust developers can build" into "something API developers can install".

## I added benchmarking to the smoke tests

Whenever I port something like this, I do not want to rely on intuition. I want the new implementation to prove itself against the old one on the same fixture set.

That is why I added a benchmark mode to the smoke test harness:

```powershell
if ($Benchmark) {
    Write-Host ">>> Benchmarking Rust CLI..."
    $rustTime = Measure-Command {
        RunTests -Method "RustCli" -Parallel $Parallel -SkipValidation $true -Production $false
    }

    Write-Host ">>> Benchmarking .NET CLI..."
    $dotnetTime = Measure-Command {
        RunTests -Method "HttpGenerator" -Parallel $Parallel -SkipValidation $true -Production $false
    }
}
```

The smoke tests now exercise both implementations against the same OpenAPI corpus across Swagger 2.0, OpenAPI 3.0, and OpenAPI 3.1, in both JSON and YAML formats, while also covering combinations such as:

- `--authorization-header`
- `--load-authorization-header-from-environment`
- `--skip-headers`
- `--content-type application/xml`
- environment-variable base URLs
- IntelliJ test generation

That gave me much more confidence than a simple "it builds" check ever could.

I am not ready to publish a polished performance report with nice charts yet, but the harness is now there. That means I can keep measuring the Rust CLI against the legacy .NET CLI while continuing to refine behavior.

## Keeping the .NET implementation was the right call

One of my favorite outcomes of this migration is that the repository now clearly shows its own history. The legacy implementation is still there under `src/dotnet`, and the new implementation lives under `src/rust`.

The README describes the legacy `.NET` CLI as the **migration oracle** and compatibility host, which is exactly how I used it during the port. That framing matters. It acknowledges that rewrites are risky, and it gives the new implementation something concrete to compare against.

This also means users who still need the old installation model can continue using:

```bash
dotnet tool install --global httpgenerator
```

while the Rust CLI matures as the primary distribution.

For this project, that is a much better approach than pretending the old implementation never existed.

## Conclusion

Porting HTTP File Generator from a .NET Tool to a Rust CLI was much more than a language rewrite. It changed the installation model, the release pipeline, the internal architecture, the OpenAPI loading strategy, and the operational shape of the tool.

The old .NET implementation proved the idea and made the tool genuinely useful. The new Rust implementation turns that idea into a native CLI with a better cross-platform story and a cleaner internal design.

Most importantly, I did not have to throw away everything that made the original version work. By keeping the legacy code in the repository and using it as a behavioral oracle, I was able to port the project incrementally and with much higher confidence.

That combination, a native distribution story, a clearer architecture, and a safer migration strategy, is what made this rewrite worth doing.