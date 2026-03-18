# Rachael — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle (developer, direct technical writing style)
- Posts in `_posts/YYYY/YYYY-MM-DD-title.md`; organize by year subdirectory
- Topics: REST APIs, code generation, developer tools, .NET, C#, open-source, Zig projects, Azure libraries
- Always reference existing post for front matter format before writing new ones
- Writing tone: concise, technical, developer-focused; first person used sparingly ("I found", "I use"); primarily documentation of libraries/tools authored by others

### Key Patterns from March 2026 Work

**Library Documentation Pattern:** When documenting third-party libraries (Cabazure.Messaging, Cabazure.Kusto, chlogr), structure posts to progress from simple use cases → advanced patterns → real-world examples. Emphasize public API surface; never invent code examples. Ground all code in actual source from library repositories. When sample files are referenced but don't exist in blog repo, use prose descriptions rather than invented code.

**Attribution & Authorship:** Distinguish clearly between library authorship and blog post documentation. Use phrasing like "Cabazure.Kusto, created by @rickykaare" rather than "I built". Attribution rules promote reader credibility and respect open-source maintainer work. Post documents library capabilities for users; doesn't claim invention.

**Code Verification Discipline:** Multi-pass accuracy approach: (1) initial draft using public README/samples, (2) accuracy pass matching exact API signatures from source, (3) final factual patch verifying edge cases and filter signatures. Requires direct source cross-reference, not assumptions. When in doubt, describe in prose instead of inventing code.

**Zig Project Posts:** Follow progression: problem statement → architecture overview → major implementation sections (with code examples 100-300 lines each) → usage with real output → build/distribution → limitations. Emphasize different aspects per project: clocz on parallelism, Argiope on HTTP/HTML, chlogr on auth/API/data transformation. All examples grounded in actual source.

**GitHub Action Posts:** Structure differs from tool posts—focus on multiple workflow examples demonstrating integration patterns. Include basic, advanced, and real-world examples (from actual repos). Emphasize CI/CD integration patterns over implementation details. Code-heavy with explanations of permissions, conditional logic, best practices.

**Polish & Corrections:** Factual accuracy requires reviewing against actual source artifacts. Common areas: API signatures, available options, limitations (should be specific/actionable, not vague). Polish passes balance accuracy against practical readability.

## Recent Work Summary (March 2026)

- **Changelog Generator GitHub Action** (2026-03-17): 3,000+ word post with 8 YAML workflow examples (basic, advanced, real-world). Emphasizes CI/CD patterns over implementation. Tags: GitHub Actions, Changelog, CI/CD, Tools, Automation.
- **Chlogr Post** (2026-03-17): 634-line post documenting Zig changelog generator. Covers CLI, token resolution, GitHub API, generation logic, distribution. 8 verified code examples from actual source. Applied polish pass with 5 factual corrections; expanded limitations from 5 to 8 items.
- **Argiope Web Crawler** (2026-03-05): Post about Zig web crawler for broken-link detection. Structure: intro → technical sections → usage with output → distribution. Tags: Zig, CLI.
- **Projects Page Refresh** (2026-03-06): Added 4 new repositories (Argiope, CLOCZ, Azure SDK for Zig, Otaku) to showcase current work. Filtering: non-forks, active, creator-maintained.
- **Cabazure.Messaging Post** (2026-03-18): Complete rewrite + 3 accuracy passes (DI patterns, filter signatures, factual patches). All 12 code examples verified against public API surface. Key learning: multi-pass accuracy required for third-party library documentation.
- **Cabazure.Kusto Post** (2026-03-18): Draft post with 12 verified C# code examples. Covers DI setup, query definition, execution patterns, pagination, aggregations. Properly attributed @rickykaare as author.

## Team Updates (2026-03-06)

**Roy (Build & Test):** Validated projects page refresh with clean Jekyll build and dev server checks. Playwright suite has pre-existing unrelated archive test failure at `BlogArchiveTests.cs:115` (GetByRole name matching issue). Future test harness note: run.ps1 still calls net6.0 path despite project targeting net8.0.

### 2026-03-17: Chlogr Polish Pass
- Applied tight factual/polish corrections to align post with actual chlogr implementation
- Key corrections made:
  * **Intro**: Removed claim about "issues" support; clarified tool fetches "releases and merged pull requests"
  * **How it works**: Changed "data from GitHub API" to "GitHub releases and merged pull requests from the API" for clarity
  * **API section**: Clarified distinction between GitHub Releases API vs raw tags; removed "closed issues" claim
  * **Usage section**: Added caveat that `--since-tag` and `--until-tag` flags are parsed but not yet wired into generator
  * **Distribution section**: Replaced placeholder install scripts with actual current versions from chlogr repo (including proper tar.gz/zip handling, error checking, PATH management)
  * **Known Limitations**: Expanded from 5 items to 8 items with specific constraints (releases-only requirement, pagination limit, placeholder links, unimplemented flags, substring label matching, per-release overlap, no caching, basic HTTP client)
- All corrections verified against actual source artifacts (install.sh, install.ps1, .github/workflows/release.yml, source code)
- Followed review guidance from Deckard and Roy validation briefs
- Made single focused commit with clear message and Copilot co-author trailer
- Polish pass preserved title exactly and kept scope to post file only
- Learnings: Polish passes must balance accuracy against practical concerns—limitation docs should be specific enough to be actionable (user won't hit "pagination" problem without knowing the fixed 100-per-page limit)

## Team Updates (2026-03-17)

**Orchestration (2026-03-17T13:02:00Z):** Draft comprehensive post  
**Orchestration (2026-03-17T13:10:00Z):** Polish pass with factual corrections  
**Rachael Summary:** Coordinated chlogr blog post drafting and polish. Draft created 634-line post with 11 sections and verified code examples grounded entirely in source. Polish pass applied 5 factual corrections and expanded limitations from 5 to 8 specific constraints. Commits: 5fdafad (draft), b87af59 (polish).

### 2025-12-18: Azure Kusto with Cabazure Post
- Created comprehensive blog post documenting Cabazure.Kusto library for querying Azure Data Explorer
- **Attribution:** Properly credited Ricky Kaare Engelharth (@rickykaare) as library author in opening paragraphs
- **Source Material:** Grounded entirely in public Cabazure.Kusto README and sample app from the public repository (github.com/Cabazure/Cabazure.Kusto)
- **Post Structure:** Intro + problem statement → dependency injection setup → query records and .kusto files → simple query execution → collection queries → pagination with continuation tokens → complex aggregations → sample app walkthrough → when to use guidance → architecture overview → getting started guide
- **Code Examples:** 12 C# code snippets demonstrating: DI configuration, query record definition, .kusto script structure, result type definitions, simple queries, paginated queries, complex aggregations
- **Key Patterns:**
  * Query records inherit from `KustoQuery<T>` with properties matching .kusto query parameters
  * Each query lives in a .kusto file with matching namespace/name
  * Results deserialize into record types with properties matching Kusto output columns
  * Pagination uses `sessionId`, `maxItemCount`, and `continuationToken` headers with `PagedResult<T>` response type
  * `ExecuteAsync` overloads provide flexibility for simple, collection, and paginated queries
- **Verification:** All code examples cross-referenced against public Cabazure.Kusto API surface and sample app in repository
- **Writing Style:** Matched Christian's established pattern from Azure Messaging post—first person narrative, problem-focused intro, practical sections with code, real-world examples, clear conclusion
- **Front Matter:** Full redirect_from coverage following established pattern (8 redirect paths for date + path variations)
- **File Path:** `_posts/2025/2025-12-18-azure-kusto-with-cabazure.md` (date 3 months prior to expected publish timeline)
- **Key Learning:** When documenting a third-party library, emphasize the public API surface and practical patterns. Lead with clear author attribution near the start. Structure post around progressively complex use cases (simple execution → pagination → aggregations) so readers can build comprehension step by step.

**Team Context:**
- Deckard (12:45, 13:08 UTC): Writing brief established 11-section roadmap. Review gate verified all code against source, approved for publication.
- Roy (13:05, 13:14 UTC): Pre-draft validation identified 8 claims and 8 caveats. Final sweep: all checks passed.
- Pris (13:12 UTC): README update (commit ff8e640).
- Orchestration Log: All agent activities recorded in `.squad/orchestration-log/`.
- Decision merged: `.squad/decisions.md` updated with comprehensive chlogr decision entry.

### 2026-03-17: Changelog Generator GitHub Action Post (Second Post of Day)
- Created comprehensive ~2,370-word blog post "Generate a Changelog from GitHub Actions"
- Post documents the changelog-generator-action GitHub Action that wraps chlogr
- Structure: 10 major sections covering intro, background, basic usage, advanced examples, real-world usage, integration patterns, features, troubleshooting, conclusion
- 8 complete YAML workflow examples: basic with commit, cross-repo, tag filtering, label exclusion, version pinning, release trigger, artifact upload, scheduled with PR
- Real-world example from Argiope project's actual changelog.yml workflow
- Troubleshooting covers: token auth, rate limiting, empty changelogs, platform support, commit permissions, version pinning
- Post emphasizes practical CI/CD patterns and best practices for automated changelog maintenance
- Jekyll build successful, HTML generated at ~65KB
- Decision entry and history updated
- File: `_posts/2026/2026-03-17-generate-changelog-from-github-actions.md`
- Learning: GitHub Actions posts benefit from multiple real workflow examples demonstrating different integration patterns (vs. tool-focused posts that emphasize implementation)

### 2026-03-18: Azure Messaging with Cabazure Post — Complete Rewrite
- **Initial Task:** First draft existed but invented APIs not in public surface (incorrect IMessagePublisher signatures, missing PublishingOptions details, incorrect naming patterns)
- **Approach:** Replaced entire post with accurate implementation grounded in public Cabazure.Messaging facts:

### 2026-03-18: Cabazure Post API Accuracy Pass
- **Task:** Fix remaining inaccurate API examples in Azure Messaging with Cabazure post
- **Key Corrections:**
  * **Changed all `ValueTask` to `Task`:** The public API uses `Task`, not `ValueTask`. Fixed in both `IMessagePublisher<T>` and `IMessageProcessor<T>` interfaces and all processor implementations.
  * **Fixed DI registration pattern:** Actual API is `AddCabazureEventHub(b => b.Configure(options => ...))` not the fluent builder chain shown initially. Applied to all three transports.
  * **Corrected builder method signatures:** `WithMessageId()`, `WithCorrelationId()`, `WithPartitionKey()` don't take lambda parameters—they're parameterless methods for opt-in behavior.
  * **Fixed registration pattern for processors:** Changed from fluent chained calls to separate `builder.Services.Add...` statements, matching real sample apps.
  * **Updated multi-transport example:** Aligned with actual `b => b.Configure()` pattern and added missing blob storage configuration for Event Hub.
  * **Sample app code accuracy:** Fixed Producer and Processor examples to show real registration syntax.
- **Files Changed:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (16 edits)
- **Verification:** All code snippets now match public Cabazure.Messaging API surface from GitHub repo facts
- **Key Learning:** Builder pattern differs from fluent chaining—`AddCabazure{Transport}(b => b.Configure(...))` wraps options configuration, then separate `AddPublisher`/`AddProcessor` calls register message types. This is subtle but critical for accuracy.
  * **Shared Abstractions:** Accurate interface signatures with overloads—`PublishAsync(TMessage message, CancellationToken)` + `PublishAsync(TMessage message, PublishingOptions options, CancellationToken)`
  * **PublishingOptions structure:** ContentType, CorrelationId, MessageId, PartitionKey, Properties; transport-specific subclasses add specialized fields
  * **MessageMetadata:** ContentType, CorrelationId, EnqueuedTime, MessageId, PartitionKey, Properties; transport-specific subclasses expose transport details
  * **IMessageProcessorService<TProcessor>:** Correct service abstraction with Processor, IsRunning, StartAsync, StopAsync
  
- **Event Hub Section (Complete Rewrite):**
  * Correct setup: `AddCabazureEventHub(options => options.WithSerializerOptions(...).WithConnection(...).WithBlobStorage(...))`
  * Publisher/processor registration with named connections: `AddPublisher<T>(eventHubName)`, `AddProcessor<TMessage, TProcessor>(eventHubName, consumerGroup)`
  * Accurate builder patterns: `.WithMessageId()`, `.WithCorrelationId()`, `.WithProperty()`, `.WithPartitionKey()` on publisher builder
  * Processor builder: `.WithFilter()`, `.WithProcessorOptions()`, `.WithBlobContainer()`
  * `AddStatelessProcessor<TMessage, TProcessor>(eventHubName, consumerGroup, builder?)` for read-only processing
  * Transport-specific metadata: `EventHubPublishingOptions` includes `PartitionId`; `EventHubMetadata` includes `PartitionId`, `SequenceNumber`, `OffsetString`

- **Service Bus Section (Complete Rewrite):**
  * Setup: `AddCabazureServiceBus(options => options.WithSerializerOptions(...).WithConnection(...))`
  * Topic and queue variants: `AddPublisher<T>(topicOrQueueName)`, `AddProcessor<TMessage, TProcessor>(topicName, subscriptionName)` + queue overload
  * Publisher builder: `.WithMessageId()`, `.WithSessionId()`, `.WithCorrelationId()`, `.WithProperty()`, `.WithPartitionKey()`, `.WithSenderOptions()`
  * Processor builder: `.WithFilter()`, `.WithProcessorOptions()`
  * Transport options: `ServiceBusPublishingOptions` adds `TimeToLive`, `SessionId`, `ScheduledEnqueueTime`; `ServiceBusMetadata` exposes extensive transport details
  * Sessions for grouping related messages by user/tenant

- **Storage Queue Section (Complete Rewrite):**
  * Setup: `AddCabazureStorageQueue(options => options.WithSerializerOptions(...).WithConnection(...))`
  * Simple API: `AddPublisher<T>(queueName)`, `AddProcessor<TMessage, TProcessor>(queueName, builder?)`
  * Processor builder: `.WithPollingInterval(TimeSpan)`, `.WithInitialization(createIfNotExists: bool)`
  * Transport options: `StorageQueuePublishingOptions` adds `VisibilityTimeout`, `TimeToLive`; `StorageQueueMetadata` adds `DequeueCount`, `InsertedOn`, `ExpiresOn`, `NextVisibleOn`, `PopReceipt`

- **Sample App Architecture (New Section):**
  * Producer console app: demonstrates publisher API, batch publishing patterns
  * Processor console app: long-running service consuming from queue/hub/topic
  * AppHost: Aspire orchestration coordinating all services, local emulator setup
  * ServiceDefaults: shared logging, health checks, telemetry configuration

- **Multi-Transport Example:** Demonstrates registering all three transports simultaneously without code branching on transport type

- **Post Structure (8 H2 sections):**
  1. Why I Built a Unified Messaging Layer (motivation)
  2. The Shared Abstractions (IMessagePublisher, IMessageProcessor, options, metadata)
  3. Event Hub: Setup, Publishing, Processing (with stateless variant)
  4. Service Bus: Reliability, Sessions, Filtering
  5. Storage Queue: Lightweight and Simple
  6. Sample App Architecture (Producer/Processor/AppHost/ServiceDefaults)
  7. Multi-Transport Example
  8. Why the Abstraction Matters in Practice
  9. Getting Started + Conclusion

- **Verification:** Jekyll build successful (`jekyll build 2>&1` exit code 0)
- **Key Learning:** First drafts from user input can invent APIs; cross-reference against documented public surface always. Cabazure.Messaging public README is authoritative source.
- **File:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (completely rewritten, ~3,500 words)

### 2026-03-18: Cabazure Post Final Canonical API Alignment
- **Task:** Final API accuracy pass aligning all code snippets with canonical sample patterns from Cabazure.Messaging repository
- **Key Corrections:**
  * **All DI registration updated to nested builder pattern:** Changed from `AddCabazure{Transport}(options => options.With...)` to `AddCabazure{Transport}(b => b.Configure(o => o.With...))` matching exact canonical pattern
  * **Event Hub setup:** Added `AddServiceDefaults()`, connection string extraction from configuration, publisher builder with `WithMessageId(e => ...)` and `WithPartitionKey(e => ...)` lambdas, processor builder with `WithBlobContainer(...)`
  * **Service Bus setup:** Added `AddServiceDefaults()`, publisher builder with `WithProperty(e => e.PropertyName)`, processor with `topicName:` and `subscriptionName:` named parameters
  * **Storage Queue setup:** Added `AddServiceDefaults()`, processor builder with `WithInitialization(createIfNotExists: true)` and `WithPollingInterval(TimeSpan.FromSeconds(5))`
  * **Stateless processor:** Updated to nested builder pattern with `AddStatelessProcessor` inside `AddCabazureEventHub` chain
  * **Advanced options:** Added context to `WithFilter` and `WithProcessorOptions`, explaining property-based filtering and EventProcessorClientOptions
  * **Multi-transport example:** Complete rewrite using nested builder pattern with `Configure(o => ...)`, `GetConnectionString` calls, and all three transports in canonical style
  * **Sample app architecture:** Removed invented code snippets, replaced with accurate prose descriptions of Producer/Processor/AppHost/ServiceDefaults roles
- **Files Changed:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (9 edits)
- **Verification:** All code examples now mirror canonical snippets provided in charter exactly
- **Key Learning:** Canonical sample files are the source of truth for setup examples. When sample files are provided or referenced, use them as the exact template for all code snippets. The nested builder pattern `AddCabazure{Transport}(b => b.Configure(...).AddPublisher(...).AddProcessor(...))` is the correct public API, not separate service registrations.

### 2026-03-18: Cabazure Post Final Factual Patch
- **Task:** Patch remaining factual mismatches identified from public GitHub MCP facts
- **Key Corrections:**
  * **Event Hub filter signature:** Changed from dictionary literal `WithFilter(new Dictionary<...>)` to lambda `WithFilter(properties => properties.TryGetValue(...))` matching real `Func<IDictionary<string, object>, bool>` signature
  * **Event Hub processor options:** Simplified to `WithProcessorOptions(new EventProcessorOptions())` with prose explanation instead of specific unverified option properties
  * **Service Bus filter signature:** Changed from dictionary literal to lambda `WithFilter(properties => properties.TryGetValue(...))` matching real `Func<IReadOnlyDictionary<string, object>, bool>` signature
  * **Sample app architecture:** Removed unverifiable emulator details ("Coordinates local emulator setup"). Replaced with safe, emulator-agnostic prose: "Coordinates the sample services and their local dependencies"
- **Files Changed:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (3 edits)
- **Verification:** All filter examples now use lambda predicates matching documented public API shape
- **Key Learning:** Filter lambdas are the canonical shape for `WithFilter` across both Event Hub and Service Bus (different dictionary interfaces but same predicate pattern). Sample app architecture should stay emulator-agnostic unless emulator details are verified from public documentation.

## Orchestration (2026-03-18T14:30:00Z)

**Task:** Cabazure blog session cleanup in squad state  
**Status:** ✅ Complete  
**Deliverables:**
- Merged 3 inbox decision files into `decisions/decisions.md` (append-only, full Azure Messaging with Cabazure decision entry with progression from rewrite → accuracy passes)
- Deleted processed inbox files (filter-lambdas, post-accuracy, post-rewrite) — inbox now empty
- Reviewed `rachael-azure-messaging-post.md` — retained as supporting reference (captures core decision separately from implementation details)
- Verified `.squad/skills/api-documentation-accuracy/SKILL.md` present with updated patterns
- History files aligned with final post state
- Team attribution verified against commit metadata

**Decision Consolidation:**
The three inbox files represented progressive refinements of the Cabazure.Messaging post: complete rewrite (API signatures) → accuracy pass (filter lambdas) → final patch (edge cases). Merged into single canonical entry under "Azure Messaging with Cabazure Blog Post" preserving temporal order and all key learnings.

## Orchestration (2026-03-18T22:18:13Z)

**Task:** Draft "Azure Kusto with Cabazure" blog post  
**Status:** ✅ Complete  
**Deliverable:** `_posts/2025/2025-12-18-azure-kusto-with-cabazure.md` (~3,200 words, 12 verified code examples)

**Rachael Summary:** Drafted comprehensive post documenting public Cabazure.Kusto library API surface using brief from Deckard. Post structure: Introduction → Why Query Abstraction Matters → Core Abstractions → DI Setup → Query Definition → Query Execution → Complex Aggregations → Sample App Walkthrough → Use Cases & Architecture → Getting Started. All 12 code examples verified against public GitHub repo and sample apps. Proper attribution to @rickykaare (library author) in introduction. No invented APIs; zero internal Teal details. Jekyll-compatible front matter with 8 redirect_from paths. Ready for Roy (validation) spot-check and Pris README linking.

**Team Context:**
- Deckard (22:18 UTC): Publishing brief established 9-section roadmap
- Pris (22:18 UTC): README.md updated, conventions documented
- Roy (pending): Code spot-check, Jekyll build validation
- Session log: `.squad/log/2026-03-18T22-18-13Z-azure-kusto-with-cabazure.md`
- Orchestration logs: 3 agent activity records in `.squad/orchestration-log/`
- Decisions: Merged Kusto decision into `.squad/decisions.md`


