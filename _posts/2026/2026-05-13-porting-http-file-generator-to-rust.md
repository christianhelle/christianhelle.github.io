---
layout: post
title: Porting HTTP File Generator from .NET to Rust
date: 2026-05-13
author: Christian Helle
tags:
- Rust
- .NET
- OpenAPI
- CLI
- Architecture
redirect_from:
- /2026/05/porting-http-file-generator-to-rust
---

[HTTP File Generator](https://github.com/christianhelle/httpgenerator) is a tool that bridges the gap between OpenAPI specifications and the [IntelliJ IDEA / VS Code HTTP Client](https://www.jetbrains.com/help/idea/http-client.html). It parses an OpenAPI document and generates `.http` files with pre-configured headers, parameters, and even sample JSON bodies.

For years, this tool was implemented as a .NET Global Tool. While it worked well for anyone already within the .NET ecosystem, I decided it was time to move towards something more lightweight, portable, and architecturally sound.

## The .NET Era

The original implementation consisted of two main parts: a CLI application and a core library. The CLI used [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console) for argument parsing and console output, while the core library handled the actual HTTP file generation.

### The .NET Architecture

The .NET version had a straightforward dependency chain:

```
HttpGenerator (CLI)
    ├── HttpGenerator.Core (Library)
    │   ├── OasReader (OpenAPI parsing)
    │   ├── Microsoft.Extensions.Azure (Azure auth)
    │   └── System.Text.Json (JSON handling)
    ├── Spectre.Console.Cli (CLI framework)
    ├── Exceptionless (Analytics)
    └── Microsoft.ApplicationInsights (Telemetry)
```

### Loading OpenAPI Documents

The .NET version relied on the `OasReader` library to load and parse OpenAPI specifications. The factory pattern was simple but opaque:

```csharp
public static class OpenApiDocumentFactory
{
    public static async Task<OpenApiDocument> CreateAsync(string openApiPath)
    {
        var result = await OpenApiMultiFileReader.Read(openApiPath);
        return result.OpenApiDocument;
    }
}
```

The `OpenApiDocument` from `Microsoft.OpenApi` is a rich, fully-featured object model that supports the entire OpenAPI specification. While thorough, this came with significant overhead — the object graph was deep, and traversing it for our specific needs (generating `.http` files) meant paying for a lot of functionality we didn't use.

### The Generator

The core generation logic in `HttpFileGenerator.cs` was a series of static methods that operated directly on the `OpenApiDocument`:

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

    // ... URL resolution logic ...

    return settings.OutputType switch
    {
        OutputType.OneRequestPerFile => GenerateMultipleFiles(
            settings, document, baseUrl, operationNameGenerator),
        OutputType.OneFile => GenerateSingleFile(
            settings, document, operationNameGenerator, baseUrl),
        OutputType.OneFilePerTag => GenerateFilePerTag(
            settings, document, baseUrl, operationNameGenerator),
        _ => throw new ArgumentOutOfRangeException(
            nameof(settings.OutputType),
            $"Unknown output type: {settings.OutputType}")
    };
}

private static string GenerateRequest(
    OpenApiDocument document,
    KeyValuePair<string, IOpenApiPathItem> operationPath,
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

    // Separate path parameters from query parameters
    var pathParams = new List<KeyValuePair<string, string>>();
    var queryParams = new List<KeyValuePair<string, string>>();

    foreach (var param in parameterNameMap)
    {
        if (operationPath.Key.Contains($"{{{param.Key}}}"))
            pathParams.Add(param);
        else
            queryParams.Add(param);
    }

    // Replace path parameter placeholders
    foreach (var pathParam in pathParams)
    {
        url = url.Replace(
            $"{{{{{pathParam.Key}}}}}",
            $"{{{{{pathParam.Value}}}}}");
    }

    // Append query parameters
    if (queryParams.Count > 0)
    {
        url += "?" + string.Join("&",
            queryParams.Select(p => $"{p.Key}={{{{{p.Value}}}}}"));
    }

    code.AppendLine($"{verb.ToUpperInvariant()} {{{{baseUrl}}}}{url}");
    code.AppendLine("Content-Type: {{contentType}}");

    // ... authorization header, custom headers ...

    var requestBody = operation.RequestBody?.Content?
        .FirstOrDefault(c => c.Key.Contains(settings.ContentType))
        .Value?.Schema;

    if (requestBody != null)
    {
        var json = GenerateSampleJson(requestBody);
        code.AppendLine(json ?? string.Empty);
    }

    return code.ToString();
}
```

While this worked, the approach had some limitations:

1. **Tight coupling to `Microsoft.OpenApi`** — every change to the OpenAPI spec format required pulling in new versions of the library, even if we didn't use the new features.
2. **No normalization layer** — the raw `OpenApiDocument` object model varies significantly between OpenAPI 2.0 (Swagger), 3.0, and 3.1. Handling all three meant writing defensive code everywhere.
3. **Heavy runtime dependencies** — the .NET tool required the .NET runtime to be installed, and the NuGet package dependencies added to the overall footprint.

## Why Port to Rust?

The goal for the port was threefold:

1. **Performance** — A single, statically-linked binary that starts instantly and processes specifications faster.
2. **Portability** — Zero runtime dependencies. The binary works anywhere, on any OS, without installing anything else.
3. **Architectural improvement** — The port was an opportunity to introduce a **normalization layer** that makes the generator far more resilient to OpenAPI spec variations.

## The Rust Architecture

The Rust version restructured the codebase into a workspace with two crates:

```
httpgenerator/
├── src/rust/
│   ├── cli/          # CLI application (clap, Azure SDK)
│   │   ├── main.rs
│   │   ├── args/     # Argument definitions
│   │   ├── execution/ # Orchestration pipeline
│   │   └── ui/       # Console presentation
│   └── core/         # Core library (public API)
│       ├── lib.rs
│       ├── model/    # Settings, output types
│       ├── normalized/ # Normalized OpenAPI model
│       ├── openapi/  # Loading, parsing, normalization
│       └── generator/ # HTTP file generation
```

### The Normalization Layer

The key architectural improvement is the **normalization layer**. Instead of working with the raw, complex `OpenApiDocument` object model, the Rust version first parses the specification into a `NormalizedOpenApiDocument` — a lean structure containing only what we need for HTTP file generation.

```rust
#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct NormalizedOpenApiDocument {
    pub specification_version: NormalizedSpecificationVersion,
    pub servers: Vec<NormalizedServer>,
    pub operations: Vec<NormalizedOperation>,
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct NormalizedOperation {
    pub path: String,
    pub method: NormalizedHttpMethod,
    pub operation_id: Option<String>,
    pub summary: Option<String>,
    pub description: Option<String>,
    pub tags: Vec<String>,
    pub parameters: Vec<NormalizedParameter>,
    pub request_body: Option<NormalizedRequestBody>,
}
```

This normalized model abstracts away the differences between OpenAPI 2.0, 3.0, and 3.1. The generator logic operates entirely on this simplified structure, making it immune to spec version quirks.

### Loading and Normalizing OpenAPI Documents

The Rust version uses a multi-stage loading pipeline:

```rust
// In src/rust/core/src/openapi/loader.rs

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

pub fn load_and_normalize_document_with_options(
    input: &str,
    tolerate_invalid_openapi31: bool,
) -> Result<NormalizedOpenApiDocument, OpenApiDocumentNormalizationError> {
    let document = load_document_with_options(input, tolerate_invalid_openapi31)
        .map_err(OpenApiDocumentNormalizationError::Load)?;
    normalize_loaded_document(&document)
        .map_err(OpenApiDocumentNormalizationError::Normalize)
}
```

The normalization happens in a separate phase, which is a significant departure from the .NET approach where parsing and generation were tightly coupled:

```rust
// In src/rust/core/src/openapi/normalize/mod.rs

pub fn normalize_loaded_document(
    document: &LoadedOpenApiDocument,
) -> Result<NormalizedOpenApiDocument, OpenApiNormalizationError> {
    Ok(NormalizedOpenApiDocument {
        specification_version: normalize_specification_version(document),
        servers: servers::normalize_servers(document)?,
        operations: operations::normalize_operations(document.raw().value())?,
    })
}
```

### Schema Normalization

One of the most challenging aspects of OpenAPI parsing is handling schema composition (`allOf`, `oneOf`, `anyOf`) and `$ref` resolution. The Rust version handles this with a dedicated normalization pass:

```rust
// In src/rust/core/src/openapi/normalize/schema.rs

pub(super) fn normalize_schema(root: &Value, value: &Value) -> NormalizedSchema {
    let mut resolution_stack = Vec::new();
    normalize_schema_with_resolution(root, value, &mut resolution_stack)
}

fn normalize_schema_with_resolution(
    root: &Value,
    value: &Value,
    resolution_stack: &mut Vec<String>,
) -> NormalizedSchema {
    match value {
        Value::Object(schema) => {
            let reference = schema
                .get("$ref")
                .and_then(Value::as_str)
                .map(str::to_string);

            // Resolve $ref if present
            let mut normalized = reference
                .as_deref()
                .and_then(|reference| resolve_internal_reference(
                    root, reference, resolution_stack))
                .unwrap_or_default();

            // Apply overlay from the current schema node
            let overlay = NormalizedSchema {
                reference,
                types: normalize_schema_types(schema.get("type")),
                properties: schema.get("properties")
                    .and_then(Value::as_object)
                    .map(|properties| {
                        properties.iter().map(|(name, property)| {
                            NormalizedSchemaProperty {
                                name: name.clone(),
                                schema: normalize_schema_with_resolution(
                                    root, property, resolution_stack),
                            }
                        }).collect()
                    }).unwrap_or_default(),
                all_of: normalize_schema_array(
                    root, schema.get("allOf"), resolution_stack),
                one_of: normalize_schema_array(
                    root, schema.get("oneOf"), resolution_stack),
                any_of: normalize_schema_array(
                    root, schema.get("anyOf"), resolution_stack),
            };

            merge_schema(&mut normalized, overlay);
            normalized
        }
        Value::Bool(value) => NormalizedSchema {
            types: vec![NormalizedSchemaType::Other(
                format!("boolean-schema:{value}"))],
            ..NormalizedSchema::default()
        },
        _ => NormalizedSchema::default(),
    }
}
```

This approach ensures that schemas are fully resolved and flattened before the generator ever sees them.

## Code Comparisons

### OpenAPI Document Loading

**Before (.NET):**

```csharp
public static async Task<OpenApiDocument> CreateAsync(string openApiPath)
{
    var result = await OpenApiMultiFileReader.Read(openApiPath);
    return result.OpenApiDocument;
}
```

**After (Rust):**

```rust
pub fn load_and_normalize_document_with_options(
    input: &str,
    tolerate_invalid_openapi31: bool,
) -> Result<NormalizedOpenApiDocument, OpenApiDocumentNormalizationError> {
    let document = load_document_with_options(input, tolerate_invalid_openapi31)
        .map_err(OpenApiDocumentNormalizationError::Load)?;
    normalize_loaded_document(&document)
        .map_err(OpenApiDocumentNormalizationError::Normalize)
}
```

The Rust version doesn't just load the document — it normalizes it in one step, producing a clean, version-agnostic model.

### Request Rendering

**Before (.NET):**

```csharp
private static string GenerateRequest(
    OpenApiDocument document,
    KeyValuePair<string, IOpenApiPathItem> operationPath,
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
        url = url.Replace($"{{{{{pathParam.Key}}}}}",
            $"{{{{{pathParam.Value}}}}}");
    }

    if (queryParams.Count > 0)
    {
        url += "?" + string.Join("&",
            queryParams.Select(p => $"{p.Key}={{{{{p.Value}}}}}"));
    }

    code.AppendLine($"{verb.ToUpperInvariant()} {{{{baseUrl}}}}{url}");
    code.AppendLine("Content-Type: {{contentType}}");

    // ...
}
```

**After (Rust):**

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
                &format!("{{{{{generated_name}}}}}"));
        } else {
            query_parameters.push((original_name, generated_name));
        }
    }

    if !query_parameters.is_empty() {
        url.push('?');
        url.push_str(
            &query_parameters.iter()
                .map(|(name, generated)|
                    format!("{name}={{{{{generated}}}}}"))
                .collect::<Vec<_>>()
                .join("&"));
    }

    push_line(&mut content,
        &format!("{} {{{{baseUrl}}}}{url}",
            operation.method.as_str().to_ascii_uppercase()));
    push_line(&mut content, "Content-Type: {{contentType}}");

    // ...
}
```

The logic is nearly identical, but the Rust version benefits from operating on the normalized model — there's no need for defensive null checks or type conversions because the normalization layer has already resolved all the complexity.

### JSON Sample Generation

**Before (.NET):**

```csharp
private static string? GenerateSampleJson(IOpenApiSchema? schema)
{
    if (schema == null) return null;

    if (schema.AllOf?.Count > 0)
        return GenerateSampleJson(schema.AllOf.FirstOrDefault(s => s != null));

    if (schema.OneOf?.Count > 0)
        return GenerateSampleJson(schema.OneOf.FirstOrDefault(s => s != null));

    if (schema.AnyOf?.Count > 0)
        return GenerateSampleJson(schema.AnyOf.FirstOrDefault(s => s != null));

    if (schema.Properties?.Count > 0)
    {
        var props = schema.Properties
            .Take(3)
            .Select(p => $"  \"{p.Key}\": {GetPropertySampleValue(p.Value)}");
        return "{\n" + string.Join(",\n", props) + "\n}";
    }

    if (schema.Type == null) return "{}";
    var genType = schema.Type.Value;
    if (genType.HasFlag(JsonSchemaType.Object))
        return "{\n  \"property\": \"value\"\n}";
    if (genType.HasFlag(JsonSchemaType.Array))
        return "[\n  \"item1\",\n  \"item2\"\n]";
    if (genType.HasFlag(JsonSchemaType.String))
        return "\"example\"";
    if (genType.HasFlag(JsonSchemaType.Integer))
        return "0";
    if (genType.HasFlag(JsonSchemaType.Number))
        return "0";
    if (genType.HasFlag(JsonSchemaType.Boolean))
        return "true";
    return "{}";
}
```

**After (Rust):**

```rust
pub(super) fn generate_sample_json(schema: &NormalizedSchema) -> String {
    if let Some(all_of) = schema.all_of.first() {
        return generate_sample_json(all_of);
    }

    if let Some(one_of) = schema.one_of.first() {
        return generate_sample_json(one_of);
    }

    if let Some(any_of) = schema.any_of.first() {
        return generate_sample_json(any_of);
    }

    if !schema.properties.is_empty() {
        let properties = schema.properties.iter()
            .take(3)
            .map(|property| format!(
                "  \"{}\": {}",
                property.name,
                property_sample_value(&property.schema)
            ))
            .collect::<Vec<_>>()
            .join(",\n");
        return format!("{{\n{properties}\n}}");
    }

    if schema.types.contains(&NormalizedSchemaType::Object) {
        return "{\n  \"property\": \"value\"\n}".to_string();
    }

    if schema.types.contains(&NormalizedSchemaType::Array) {
        return "[\n  \"item1\",\n  \"item2\"\n]".to_string();
    }

    if schema.types.contains(&NormalizedSchemaType::String) {
        return "\"example\"".to_string();
    }

    if schema.types.contains(&NormalizedSchemaType::Integer)
        || schema.types.contains(&NormalizedSchemaType::Number)
    {
        return "0".to_string();
    }

    if schema.types.contains(&NormalizedSchemaType::Boolean) {
        return "true".to_string();
    }

    "{}".to_string()
}
```

The logic is the same, but the Rust version uses an enum-based type system (`NormalizedSchemaType`) instead of `JsonSchemaType` flags, making it more explicit and easier to reason about.

### The CLI Entry Point

**Before (.NET):**

```csharp
private static int Main(string[] args)
{
    if (args.Length == 0) args = ["--help"];

    CommandApp<GenerateCommand> app = new();
    app.Configure(configuration =>
    {
        configuration
            .SetApplicationName("httpgenerator")
            .AddExample("./openapi.json")
            .AddExample("./openapi.json", "--output", "./")
            .AddExample("./openapi.json", "--output-type", "onefile")
            .AddExample("https://petstore.swagger.io/v2/swagger.json")
            .AddExample("https://petstore3.swagger.io/api/v3/openapi.json",
                "--base-url", "https://petstore3.swagger.io")
            .AddExample("./openapi.json", "--generate-intellij-tests")
            // ... more examples ...
    });

    return app.Run(args);
}
```

**After (Rust):**

```rust
fn main() {
    let raw_args = raw_args_with_help();
    let args = parse_args(&raw_args);
    let mut telemetry = TelemetryRecorder::from_cli_args(
        &raw_args, &args, NoopTelemetrySink);
    let started_at = Instant::now();
    let mut presenter = CliPresenter::detect();
    presenter.print_header(args.no_logging);

    match execute_with_observer(args.clone(), &mut presenter) {
        Ok(_summary) => {
            telemetry.record_feature_usage(&args);
            presenter.print_success(started_at.elapsed());
        }
        Err(error) => {
            telemetry.record_error(&args,
                error.telemetry_name(), &error.to_string());
            presenter.print_error(&error);
            std::process::exit(1);
        }
    }
}
```

The Rust CLI uses `clap` with derive macros for argument parsing, which is more declarative and type-safe than the Spectre.Console.Cli approach.

### Execution Pipeline

**Before (.NET):**

```csharp
protected override async Task<int> ExecuteAsync(
    CommandContext context, Settings settings,
    CancellationToken cancellationToken = default)
{
    Analytics.Configure(settings);

    var stopwatch = Stopwatch.StartNew();
    DisplayHeader(settings);

    if (!settings.SkipValidation)
        await ValidateOpenApiSpec(settings);

    await AcquireAzureEntraIdToken(settings);

    var generatorSettings = new GeneratorSettings
    {
        AuthorizationHeader = settings.AuthorizationHeader,
        AuthorizationHeaderVariableName = settings.AuthorizationHeaderVariableName,
        AuthorizationHeaderFromEnvironmentVariable =
            settings.AuthorizationHeaderFromEnvironmentVariable,
        OpenApiPath = settings.OpenApiPath,
        ContentType = settings.ContentType,
        BaseUrl = settings.BaseUrl,
        OutputType = settings.OutputType,
        Timeout = settings.Timeout,
        GenerateIntelliJTests = settings.GenerateIntelliJTests,
        CustomHeaders = settings.CustomHeaders,
        SkipHeaders = settings.SkipHeaders,
    };

    GeneratorResult result = await HttpFileGenerator.Generate(generatorSettings);
    await Analytics.LogFeatureUsage(settings);
    await WriteFiles(settings, result);

    DisplaySuccess(stopwatch.Elapsed);
    return 0;
}
```

**After (Rust):**

```rust
pub(crate) fn execute_with<F, O>(
    args: CliArgs,
    observer: &mut O,
    acquire_token: F,
) -> Result<ExecutionSummary, CliError>
where
    F: Fn(Option<&str>, &str) -> Result<Option<String>, String>,
    O: ExecutionObserver,
{
    let open_api_path = args.open_api_path
        .clone().ok_or(CliError::MissingInput)?;

    if !args.skip_validation {
        observer.validation_started();
    }

    let validation = validate_openapi_document(
        &open_api_path, args.skip_validation)?;
    if let Some(inspection) = &validation {
        observer.validation_succeeded(inspection);
    }

    let should_attempt_azure_auth = should_attempt_azure_auth(&args);
    if should_attempt_azure_auth {
        observer.azure_auth_started();
    }

    let (authorization_header, azure_auth) =
        resolve_authorization_header(&args, acquire_token);
    if should_attempt_azure_auth {
        observer.azure_auth_finished(&azure_auth);
    }

    let document = load_and_normalize_document_with_options(
        &open_api_path, args.skip_validation)
        .map_err(|error| CliError::LoadOpenApi(error.to_string()))?;

    let settings = build_generator_settings(
        &args, open_api_path.clone(), authorization_header);

    let result = generate_http_files(&settings, &document);
    observer.file_writing_started(result.files.len());

    let output_folder = PathBuf::from(&args.output_folder);
    let files = write_files(&output_folder, result.files, args.timeout)?;
    observer.files_written(&files);

    Ok(ExecutionSummary {
        output_folder, files, validation, azure_auth,
    })
}
```

The Rust version uses an observer pattern for telemetry and validation events, making the execution pipeline more testable and extensible.

## Key Architectural Improvements

### 1. Normalization Layer

The biggest architectural improvement is the introduction of a **normalization layer** that sits between loading and generation. This decouples the generator from the complexities of OpenAPI spec versions.

In the .NET version, the generator worked directly with `Microsoft.OpenApi` types, which varied significantly between OpenAPI 2.0, 3.0, and 3.1. Every new OpenAPI version meant writing more defensive code.

In the Rust version, the normalization layer produces a `NormalizedOpenApiDocument` that is consistent regardless of the source spec version. The generator operates entirely on this normalized model, making it immune to spec version quirks.

### 2. Schema Resolution

The Rust version handles `$ref` resolution and schema composition (`allOf`, `oneOf`, `anyOf`) during the normalization phase. This means the generator never has to deal with unresolved references or composition keywords — they're all flattened and resolved upfront.

### 3. Type Safety

The Rust version uses explicit enums for HTTP methods, parameter locations, and schema types instead of flags or strings. This eliminates entire classes of bugs at compile time.

```rust
pub enum NormalizedHttpMethod {
    Get, Put, Post, Delete, Options, Head, Patch, Trace,
}

pub enum NormalizedParameterLocation {
    Path, Query, Header, Cookie,
}

pub enum NormalizedSchemaType {
    String, Integer, Number, Boolean, Object, Array, Null,
    Other(String),
}
```

### 4. Zero Dependencies

The Rust binary is statically linked and requires no runtime installation. The .NET version required the .NET runtime plus several NuGet packages.

## Dependencies Comparison

**Before (.NET):**

| Package | Purpose |
|---------|---------|
| Spectre.Console.Cli | CLI framework |
| OasReader | OpenAPI parsing |
| Microsoft.Extensions.Azure | Azure authentication |
| Exceptionless | Analytics |
| Microsoft.ApplicationInsights | Telemetry |
| System.Text.Json | JSON handling |

**After (Rust):**

| Crate | Purpose |
|-------|---------|
| clap | CLI argument parsing |
| openapiv3 / openapiv3_1 | OpenAPI 3.0 / 3.1 parsing |
| azure_core / azure_identity | Azure authentication |
| serde / serde_json | Serialization |
| yaml_serde | YAML support |
| reqwest | HTTP client (for loading remote specs) |

The Rust dependencies are more focused and the overall footprint is smaller.

## Conclusion

The port from .NET to Rust was not just about changing languages — it was about refining the design. The normalization layer introduced during the port makes HTTP File Generator far more resilient to OpenAPI spec variations, and the type-safe Rust model eliminates entire classes of bugs that were only caught at runtime in the .NET version.

The tool remains functionally identical — it still generates the same `.http` files with the same headers, parameters, and sample bodies. But underneath, the architecture is cleaner, more robust, and faster.

If you want to check out the new version, head over to the [HTTP File Generator repository](https://github.com/christianhelle/httpgenerator).

For more on my journey with Rust and .NET, check out [testing Refit interfaces using Alba](/2025/01/testing-refit-interfaces-using-alba.html) or [integration testing with httprunner](/2026/01/integration-testing-with-httprunner.html).
