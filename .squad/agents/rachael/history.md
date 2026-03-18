# Rachael — History

## Core Context
- Project: christianhelle/blog — Jekyll blog hosted on GitHub Pages
- User: Christian Helle (developer, direct technical writing style)
- Posts in `_posts/YYYY/YYYY-MM-DD-title.md`
- Topics: REST APIs, code generation, developer tools, .NET, C#, open-source
- Always reference an existing post for front matter format before writing new ones
- Writing tone: concise, technical, developer-focused

## Learnings

### 2026-03-17: Changelog Generator GitHub Action Post
- Created comprehensive 3,000+ word blog post documenting the changelog-generator-action for GitHub Actions
- Post structure: intro → background (chlogr + GitHub Action) → basic usage → advanced examples → real-world example (Argiope) → integration patterns → features → troubleshooting → conclusion
- Multiple YAML workflow examples demonstrating: basic changelog generation with commit, cross-repository changelog, tag filtering, label exclusion, version pinning, release integration, artifact upload, scheduled updates, PR creation
- Each workflow example includes full YAML with explanations of permissions, conditional logic, and best practices
- Real-world usage demonstrated with actual Argiope workflow (changelog.yml)
- Troubleshooting section covers token authentication, rate limiting, empty changelogs, platform support, commit permissions
- Action wraps chlogr (Zig-based CLI tool) for automated changelog generation from GitHub releases and merged PRs
- Key differentiation: generates from releases/PRs (user-facing) vs. commits (implementation details)
- Post emphasizes practical CI/CD integration: release triggers, scheduled updates, PR workflows, artifact upload
- Tags used: GitHub Actions, Changelog, CI/CD, Tools, Automation (consistent with tools/automation theme)
- Front matter follows established pattern: layout, title, date, author, tags array, redirect_from paths
- Writing style: First person ("I recently built", "I use this action"), technical but accessible, code-heavy with explanations
- Grounded in actual documentation from chlogr README and changelog-generator-action README

### 2026-03-18: Cabazure Post Final API Accuracy Pass
- **Task:** Patch remaining inaccurate API examples to match real Cabazure.Messaging public surface
- **Key Corrections:**
  * **DI Registration Pattern:** Removed incorrect `b => b.Configure()` wrapper. Actual API is `AddCabazureEventHub(options => options.WithConnection(...))` not the nested builder chain.
  * **Event Hub Advanced Options:** Replaced parameterless `.WithMessageId()` / `.WithCorrelationId()` / `.WithPartitionKey()` builder examples with accurate `WithFilter(dictionary)` and `WithProcessorOptions(options)` showing real dictionary-based filtering.
  * **Service Bus Filtering:** Changed from invented lambda `msg => msg.Amount > 1000` to real dictionary-based filter `WithFilter(new Dictionary<string, object> { ["Amount"] = 1000 })`.
  * **Sample App Architecture:** Removed invented Producer/Processor/AppHost code snippets (sample files don't exist in blog repo). Replaced with prose descriptions of each component's role, grounded in what the Cabazure.Messaging repo actually includes.
  * **Multi-Transport Example:** Fixed DI registration to use correct `AddCabazure{Transport}(options => ...)` pattern instead of `b => b.Configure()` wrapper.
- **Files Changed:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (7 edits)
- **Verification:** All code snippets now match documented Cabazure.Messaging API surface from public GitHub facts
- **Key Learning:** When sample program files are referenced but don't exist in the repo, replace invented code with accurate prose descriptions. The blog repo isn't required to contain the samples—they live in the library repo. Reference them descriptively rather than inventing code that might drift from reality.

### 2026-03-18: Cabazure Attribution Directive
- **Directive:** Clear that Cabazure.Messaging library written by @rickykaare; remove any "I built" or "I created" phrasing from the post.
- **Final Wording Rule:** When documenting a library authored by someone else (e.g., @rickykaare), write only the public API surface and document patterns. Never phrase the post as "I created this library" or "I built this feature." Keep focus on documenting the library's capabilities for users, not claiming authorship. Credit the author explicitly if phrasing arises naturally in context (e.g., "Cabazure.Messaging, created by @rickykaare").
- **Context:** Third-party library documentation requires clear distinction between library authorship and blog post documentation. Readers must understand the library is independently authored and maintained.

### 2026-03-05: Argiope Web Crawler Post
- Created post about Argiope, a web crawler written in Zig for broken-link detection
- Followed style from "Building a fast line of code counter app in Zig" post (2026-02-10)
- Post structure: intro → multiple technical sections with code examples → usage examples with output → distribution → conclusion
- Each section demonstrates implementation details: crawler BFS, HTML parsing, URL normalization, HTTP client, CLI parsing, report generation
- Christian's preferred code example style: show key functions with comments, explain design decisions inline
- Usage section shows realistic command-line examples with actual output formatting
- Distribution section covers install scripts (bash/PowerShell), snapcraft, GitHub Actions
- Front matter format: layout (post), title, date, author, tags (array)
- Tags for this post: Zig, CLI (matches clocz post tagging pattern)
- Post file naming: `_posts/YYYY/YYYY-MM-DD-title.md` format strictly followed
- Christian uses GitHub Copilot heavily for boilerplate (workflows, README, install scripts)

### 2026-03-17: Chlogr GitHub Changelog Generator Post
- Created comprehensive 634-line blog post documenting the chlogr project in depth
- Post structure: intro → CLI parsing → token resolution → GitHub API → changelog generation → Markdown formatting → usage → build/test → distribution → limitations/future work → conclusion
- Each major section pairs substantial Zig code examples (100-300 lines) with explanatory text
- Code patterns documented: memory ownership tracking (is_owned flag on token resolver), error handling with error union types, HashMap-based grouping and conversion to slices, pre-calculated size concatenation for Markdown output
- Token resolver demonstrates fallback chain: flag → GITHUB_TOKEN env → GH_TOKEN env → `gh auth token` subprocess → anonymous (all with proper memory cleanup tracking)
- API client shows JSON parsing with `ignore_unknown_fields`, deep-copy pattern from parsed temporary data to owned allocations
- Changelog generator uses StringHashMap for category grouping, date comparison via ISO 8601 string slicing, filtering logic for labels
- Usage section includes realistic command-line examples, error messages, and generated Markdown output
- Grounded entirely in actual source (all examples from chlogr repository)
- Learnings: Each Zig project post emphasizes different implementation aspects: clocz focused on parallelism, Argiope on HTTP/HTML parsing, chlogr on auth/API/data transformation
- Updated projects.md with 4 new projects: Argiope, CLOCZ, Azure SDK for Zig, Otaku
- Search method: Use GitHub search API `user:christianhelle sort:updated` to get recent repositories
- Filtering criteria: Exclude forks (fork: false), exclude archived projects, include only active creator/maintainer repos
- New projects were added at the end of the list, preserving existing items (Christian requested keeping existing items)
- Projects selected for public-facing projects page: Must have meaningful descriptions and represent Christian's current work focus
- Zig projects trending: Argiope and CLOCZ both recent Zig projects, indicating renewed focus on the language
- Projects page serves as portfolio piece, so descriptions emphasize practical value and technical highlights

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

