# Squad Decisions

## Active Decisions

### Share Button X Branding
**Decided:** 2026-03-06  
**Owner:** Pris, Roy  
**Status:** Completed

Modernized the blog post share button UI from Twitter bird to X icon. Updated SVG glyph and accessible label/title to "Share on X" while preserving the existing `twitter.com/intent/tweet` endpoint and all Liquid parameters. This is a pure UI rebrand with no functional change to the share flow. Added focused Playwright regression test coverage in `ShareUiTests.cs` to prevent future share UI regressions.

### Archive Playwright Locator Hardening
**Decided:** 2026-03-06  
**Owner:** Roy  
**Status:** Completed

Hardened `tests/playwright/BlogArchiveTests.cs` by using `Exact = true` on archive link role locators. The archive crawl was failing due to substring matching collisions when post titles share prefixes (e.g., "Danish Developer Conference 2012" and "Multi-platform Mobile Development"). Exact matching is a test-only fix that prevents future regressions without altering site content.

### Blog Post Series: Zig Projects
**Decided:** 2026-03-05  
**Owner:** Deckard, Rachael, Roy  
**Status:** Completed

Continue documenting Zig projects with blog posts following established style conventions. Third entry "Building a GitHub Changelog Generator in Zig" (2026-03-17) completed following established style pattern. Comprehensive 634-line post with 11 sections, verified code examples, and detailed limitations documentation. Includes working brief, peer review gate, factual corrections pass, and final validation sweep.

### Projects Page Refresh
**Decided:** 2026-03-06  
**Owner:** Rachael  
**Status:** Completed

Updated projects.md with 4 new non-fork, active repositories showcasing Christian's recent work: Argiope (web crawler in Zig), CLOCZ (line counter in Zig), Azure SDK for Zig, and Otaku (manga reader). All existing projects retained. Selection emphasized Zig ecosystem growth and practical developer value in portfolio presentation.

### Chlogr Blog Post: GitHub Changelog Generator in Zig
**Decided:** 2026-03-17  
**Owner:** Deckard, Rachael, Roy, Pris  
**Status:** Completed

Published comprehensive blog post "Building a GitHub Changelog Generator in Zig" documenting the chlogr project. Post follows established Zig project documentation pattern with working brief, 11 must-have sections (problem statement through lessons learned), verified code examples, testing section, usage guide, and detailed limitations. Multi-pass workflow: writing brief → draft → validation → review gate → factual corrections → README update → final validation. All code examples grounded in actual source. Post commits: 5fdafad (draft), b87af59 (polish), ff8e640 (README). Final validation: jekyll build ✅, dotnet build ✅, smoke checks ✅.

### Changelog Generator GitHub Action Post
**Decided:** 2026-03-17  
**Owner:** Rachael  
**Status:** Completed

Published comprehensive blog post "Generate a Changelog from GitHub Actions" documenting the changelog-generator-action for GitHub Actions workflows. Post covers basic and advanced usage patterns, real-world Argiope integration example, CI/CD integration patterns (release triggers, scheduled updates, artifact upload), troubleshooting, and best practices. ~2,370 words with 8 YAML workflow examples demonstrating conditional commits, cross-repositories changelogs, tag filtering, label exclusion, version pinning, and automated PR creation. Post grounds readers in both the GitHub Action wrapper and underlying chlogr CLI tool (Zig-based, fast, zero dependencies). Tags: GitHub Actions, Changelog, CI/CD, Tools, Automation.

### Azure Messaging with Cabazure Blog Post
**Decided:** 2026-03-18  
**Owner:** Rachael  
**Status:** Completed

Published comprehensive blog post "Azure Messaging with Cabazure" (dated 2025-08-18) documenting the public Cabazure.Messaging library for unified Azure messaging patterns across Event Hubs, Service Bus, and Storage Queues.

#### What Was Decided

Write a long-form technical blog post demonstrating how Cabazure.Messaging abstracts Azure Event Hubs, Service Bus, and Storage Queue with shared APIs (IMessagePublisher<T>, IMessageProcessor<T>, PublishingOptions, MessageMetadata). The post focuses entirely on public library surface without exposing internal Teal repository details. All code examples verified against public README and sample applications.

#### Complete Rewrite (2026-03-18 Early)

Initial draft invented APIs not in the public surface. Completely rewrote the post with 100% accuracy to documented Cabazure.Messaging API:

- **IMessagePublisher<TMessage>:** Two overloads—`PublishAsync(message, cancellationToken)` and `PublishAsync(message, options, cancellationToken)`
- **IMessageProcessor<TMessage>:** Single `ProcessAsync(message, metadata, cancellationToken)` method
- **PublishingOptions & MessageMetadata:** Shared base with ContentType, CorrelationId, MessageId, PartitionKey, Properties; transport-specific subclasses for each transport
- **Event Hub Setup:** `AddCabazureEventHub(options => options.WithSerializerOptions().WithConnection().WithBlobStorage())`; Publisher builder with `.WithMessageId()`, `.WithCorrelationId()`, `.WithProperty()`, `.WithPartitionKey()`; Processor with `.WithFilter()`, `.WithProcessorOptions()`, `.WithBlobContainer()`
- **Service Bus Setup:** `AddCabazureServiceBus(options => options.WithSerializerOptions().WithConnection())`; Publisher builder with `.WithMessageId()`, `.WithSessionId()`, `.WithCorrelationId()`, `.WithProperty()`, `.WithPartitionKey()`, `.WithSenderOptions()`; Processor with `.WithFilter()`, `.WithProcessorOptions()`
- **Storage Queue Setup:** `AddCabazureStorageQueue(options => options.WithSerializerOptions().WithConnection())`; Processor with `.WithPollingInterval()`, `.WithInitialization(createIfNotExists: bool)`
- **Sample App Architecture:** Removed invented code snippets; replaced with accurate prose descriptions of Producer/Processor/AppHost/ServiceDefaults roles grounded in library repo samples
- Post structure: 9 major sections covering abstractions, three transports with registration/publishing/processing examples each, multi-transport patterns, and practical guidance
- All examples verified against public Cabazure.Messaging README and sample applications
- Jekyll build: ✅ Clean build, HTML generated successfully
- Tags: Azure, Messaging, .NET

#### Final API Accuracy Passes (2026-03-18 Late)

Two targeted patches aligned all code snippets with canonical API surface and sample patterns:

1. **Dictionary vs Lambda Filters:** Changed `WithFilter` examples from dictionary literals to lambda predicates matching real API signatures—Event Hub uses `Func<IDictionary<string, object>, bool>` and Service Bus uses `Func<IReadOnlyDictionary<string, object>, bool>`. Updated both transports to use filter patterns: `properties => properties.TryGetValue("Amount", out var amount) && amount is int amountValue && amountValue > 1000`

2. **Verified Filter Signature Pattern:** Confirmed lambda predicate is canonical shape for `WithFilter` across both Event Hub and Service Bus (different dictionary interfaces but same predicate pattern)

#### Key Learnings Captured

- **For library blog posts:** Core interface signatures and base option classes must match public API exactly; these are the contract readers will code against
- **Builder pattern fluency:** Fluent APIs may expose methods not captured in initial GitHub fact gathering; require source verification or official docs cross-check
- **Sample program handling:** When sample files are referenced but don't exist in blog repo, they live in library repo—describe component roles in accurate prose rather than inventing code
- **Fact-checking flow:** (1) verify interface method signatures, (2) verify public properties on option/metadata classes, (3) verify DI registration patterns, (4) verify builder fluent API methods if used in examples

#### Outcome

Post published in blog index, validates with Jekyll build, and provides practical reference documentation for Cabazure.Messaging users. Readers can copy patterns directly for their own projects. File: `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (~3,250 words).

### README.md Index Update: Azure Messaging with Cabazure
**Decided:** 2026-03-18  
**Owner:** Pris  
**Status:** Completed

Added new blog post entry "Azure Messaging with Cabazure" to README.md in the 2025 section. Entry positioned in reverse chronological order (between 2025-09 and 2025-06 entries) following established README pattern. URL: `https://christianhelle.com/2025/08/azure-messaging-with-cabazure.html`. Post publication date: 2025-08-18.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
