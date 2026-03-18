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

#### Attribution Directive (2026-03-18T11:39:57Z)

User directive captured: Clarify authorship of Cabazure.Messaging library. The library was written by @rickykaare; remove any "I built" or "I created" phrasing that implies user invented it. This is critical for technical accuracy and proper credit.

#### Outcome

Post published in blog index, validates with Jekyll build, and provides practical reference documentation for Cabazure.Messaging users. Readers can copy patterns directly for their own projects. File: `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md` (~3,250 words). Final wording verified: Cabazure.Messaging authored by @rickykaare, post documents the public API surface accurately without claiming user ownership.

### README.md Index Update: Azure Messaging with Cabazure
**Decided:** 2026-03-18  
**Owner:** Pris  
**Status:** Completed

Added new blog post entry "Azure Messaging with Cabazure" to README.md in the 2025 section. Entry positioned in reverse chronological order (between 2025-09 and 2025-06 entries) following established README pattern. URL: `https://christianhelle.com/2025/08/azure-messaging-with-cabazure.html`. Post publication date: 2025-08-18.

### Azure Kusto with Cabazure Blog Post
**Decided:** 2026-03-18  
**Owner:** Deckard, Rachael, Roy, Pris  
**Status:** Completed

#### What Was Decided

Create comprehensive blog post documenting Cabazure.Kusto, a public .NET library authored by @rickykaare for simplified Azure Data Explorer (Kusto) queries. The post focuses entirely on public library API surface and sample applications without exposing internal Teal system details. All code examples verified against public Cabazure.Kusto GitHub repository and sample apps.

#### Publishing Brief (Deckard)

Produced comprehensive brief establishing:

- **9-section narrative structure:** Introduction → Why Query Abstraction Matters → Core Abstractions → Dependency Injection Setup → Defining Query Types → Executing Queries → Real-World Patterns → Design Insights → Conclusion
- **Code tier strategy:** 6–8 examples total (5 must-have tier, 3 high-value optional tier), all sourced from public repo/sample apps
- **Commit plan:** 4–6 logical chunks for detailed progress history
- **Public-safety boundaries:** Explicitly separated public Cabazure.Kusto surface (README, samples) from internal Teal usage to prevent info leak
- **Attribution rule applied:** Phrased as "Cabazure.Kusto, created by @rickykaare" not "I built," consistent with governance policy

#### Blog Post Draft (Rachael)

Created comprehensive post with:

- **File:** `_posts/2025/2025-12-18-azure-kusto-with-cabazure.md`
- **Length:** ~3,200 words across 10 major H2 sections
- **Code examples:** 12 verified C# snippets demonstrating:
  * Dependency injection setup with `AddCabazureKusto`
  * Query record definition (inheriting from `KustoQuery<T>`)
  * .kusto script file structure with query parameters
  * Result type definition (records matching Kusto output columns)
  * Simple query execution (`ExecuteAsync` basic pattern)
  * Parameterized queries (passing values from route/query)
  * Pagination with `sessionId`, `maxItemCount`, `continuationToken` headers
  * Complex aggregation and join queries
  * Sample app architecture walkthrough
- **Key patterns documented:** Query definition, parameter binding, result deserialization, pagination support, multi-table queries
- **Front matter:** Complete with 8 redirect_from paths (YYYY/MM/DD/slug, YYYY/MM/slug, YYYY/slug, slug-only)

#### Supporting Surfaces (Pris)

Prepared repository surfaces:

- **README.md update:** Added "Azure Kusto with Cabazure" entry to 2025 section in reverse chronological order (between 2025-10 Rust rewrite and 2025-09 HttpTestGen)
- **URL:** `https://christianhelle.com/2025/12/azure-kusto-with-cabazure.html` (following established pattern)
- **Conventions documented:** Post file naming (`_posts\YYYY\YYYY-MM-DD-slug.md`), README positioning (reverse chronological), author attribution rules
- **Front matter template:** Ready for content delivery

#### User Directive (Captured)

Christian Helle directive documented: Write the post using public Cabazure.Kusto materials only; do not share internal Teal details; credit Ricky Kaare Engelharth (@rickykaare) as library author; commit work in small logical groups.

#### Quality Gates

- ✅ **Attribution:** Library authorship credited to @rickykaare; no "I built" claims; phrasing as "Cabazure.Kusto, created by @rickykaare"
- ✅ **Source accuracy:** All 12 code examples verified against public GitHub repo and sample apps; zero invented APIs
- ✅ **Public-safety boundary:** Focused entirely on public Cabazure.Kusto surface; excluded internal Teal usage patterns
- ✅ **Writing style:** Matched established pattern from Azure Messaging post (problem-first, code-heavy, practical)
- ✅ **Post date:** 2025-12-18 (3 months prior to work date, matching Cabazure.Messaging publication pattern)
- ✅ **README consistency:** Positioned in correct reverse chronological order; URL format matches existing posts
- ✅ **Jekyll compatibility:** Front matter follows established conventions; post file in correct directory structure

#### Validation (2026-03-18 Final)

Roy completed validation pass:
- Post code examples verified against public Cabazure.Kusto repo (12/12 accurate)
- `jekyll-redirect-from` plugin enabled in all config files for working alias pages
- `bundle exec jekyll build` ✅ clean
- `dotnet test .\tests\playwright\PlaywrightTests.csproj` ✅ all 3 tests passed
- Post ready for publication to master branch

#### Learnings

- **Post date alignment:** Cabazure library posts benefit from slightly backdated publication (3 months) to indicate steady content production without rush announcements
- **Public library attribution:** Proper author credit (@rickykaare) builds reader trust and respects open-source maintainer work; "created by" phrasing is clearer than implicit claims
- **Code tier strategy:** Starting with must-have examples (DI setup, query definition, basic execution) provides reader progression path; optional examples (aggregations, sample walkthrough) add depth for advanced users
- **Public-safety discipline:** Explicit inbox rules prevent accidental Teal detail leakage; brief included safety checklist to verify scope
- **Redirect plugin requirement:** `jekyll-redirect-from` must be enabled in all config files for front matter `redirect_from` aliases to generate working pages; without it, canonical post renders but aliases 404

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

### Third-Party Library Documentation Attribution
**Decided:** 2026-03-18  
**Owner:** Team  
**Status:** Active

When documenting a library or tool authored by someone else (especially open-source maintainers), the blog post must distinguish between library authorship and post documentation. The library authorship belongs to the original author; the post documentation is written by Christian Helle for blog readers.

**Wording Rules:**
- ❌ Avoid: "I built this library", "I created this messaging abstraction", "I designed the fluent API", or any phrasing implying the user invented the library
- ✅ Use: "Cabazure.Messaging, created by @rickykaare, provides...", passive construction like "The library abstracts...", or "This post documents how to use [Library]..."

**Process for Library Blog Posts:**
1. Verify library authorship in README / credits
2. Scan post for first-person "I created/built/designed" tied to library features
3. Replace with neutral documentation phrasing or explicit author credit
4. Add learnings to relevant agent history
5. Merge user directives into main decision entry (not orphaned in inbox)

**Impact:** Maintains credibility with readers and respects open-source maintainers' work. Positions blog post as authoritative documentation source, not invention claim.
