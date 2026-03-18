---
name: "api-documentation-accuracy"
description: "Write accurate API examples for library documentation blog posts"
domain: "technical-writing"
confidence: "high"
source: "repository"
---

## Context

Use this skill when writing or patching blog posts that document library APIs, frameworks, or SDKs. The goal is to ensure every code example matches the real public API surface exactly.

## Patterns

### Ground Examples in Real Sources

- **Never invent API signatures.** Cross-reference builder methods, interface signatures, and DI registration patterns against the library's public documentation or source repository.
- If sample program files are listed but don't exist in the blog repo, they likely live in the library repo. Reference them descriptively rather than inventing code.
- When provided with API facts from external sources (GitHub, docs, package metadata), treat those as authoritative and align all examples to match.

### Common API Accuracy Traps

- **Return types:** Don't assume `ValueTask` when the API uses `Task`. Check the actual method signature.
- **Builder patterns:** Fluent builder chains differ between libraries. Some use `AddService(b => b.Configure(options => ...))`, others use `AddService(options => ...)`. Verify the exact pattern.
- **Filter/lambda parameters:** Some builders accept lambdas (`msg => msg.Amount > 1000`), others accept dictionaries or options objects. Don't assume—verify.
- **Parameterless vs. parameterized methods:** Builder methods like `.WithMessageId()` might be parameterless opt-in flags, not methods that accept values.

### When Sample Files Don't Exist Locally

- If the blog post references sample apps (Producer, Processor, AppHost, etc.) but those files don't exist in the blog repository, they're external references.
- Replace invented code snippets with accurate prose descriptions of component roles and responsibilities.
- Link to the actual library repository where readers can find the real sample code.
- Example: Instead of inventing a 20-line `Program.cs` snippet, write "The Producer console app configures the transport, registers publishers, builds a host, resolves the publisher from DI, and sends messages."

### Validation Before Publishing

- **Jekyll build must pass:** Run `bundle exec jekyll build` to ensure no Liquid/Markdown syntax errors.
- **Verify no invented patterns:** Every builder method, option type, and registration pattern should be verifiable against public docs or source.
- **Check for consistency:** If the same API pattern appears in multiple sections, ensure they all use the same accurate signature.

## Anti-Patterns

- Inventing API signatures based on what "feels right" or "should work."
- Copying inaccurate examples from earlier drafts without cross-checking.
- Assuming builder method signatures when only method names are known.
- Creating full sample program files in the blog repo when they should reference external samples.
- Mixing `Task` and `ValueTask` inconsistently without checking the real API.

## Example Corrections

### Bad: Invented Lambda Filter
```csharp
processor => processor.WithFilter(msg => msg.Amount > 1000)
```

### Good: Dictionary-Based Filter (If That's the Real API)
```csharp
processor => processor.WithFilter(new Dictionary<string, object> { ["Amount"] = 1000 })
```

---

### Bad: Nested Builder Wrapper (Invented)
```csharp
builder.Services.AddCabazureEventHub(b => b.Configure(options => 
{
    options.WithConnection("...");
}));
```

### Good: Direct Options Configuration (Real API)
```csharp
builder.Services.AddCabazureEventHub(options => 
{
    options.WithConnection("...");
});
```

---

### Bad: Invented Sample Code
```csharp
// Producer/Program.cs
var host = Host.CreateApplicationBuilder();
// ... 30 lines of invented setup code
```

### Good: Prose Description Referencing Real Samples
**Producer console app** — Publishes sample messages to demonstrate the publisher API. Configures the transport, registers publishers, builds a host, resolves the publisher from DI, and sends messages.

The [GitHub repository](https://github.com/user/library) has sample applications showing the complete setup.
