# Decision: November 2025 Historical Accuracy Revision

**Decided:** 2026-03-17 (by Rachael, Content Dev)  
**Owner:** Rachael  
**Status:** Completed

## Context

The blog post "Building a GitHub Changelog Generator in Zig" was dated 2025-11-15 but had been drafted with present-day (March 2026) project naming and features. Roy's validation check identified several factual inaccuracies that made the post historically misleading for readers who knew the project timeline.

## What Was Revised

### Project Naming
- Corrected from "chlogr" to "changelog-generator" (the November 2025 name)
- Explicitly noted in the intro that the project was "later renamed to chlogr" for reader familiarity
- Added `redirect_from` block to front matter per Pris's guidance

### CLI Interface  
- Corrected from combined `--repo owner/repo` to separate `--owner` and `--repo` flags (the November 2025 interface)
- Updated all CLI examples and code samples accordingly
- Updated CliArgs struct to reflect separate fields

### HTTP Implementation
- Corrected from `std.http.Client` to curl-based subprocess spawning (November 2025 approach)
- Updated the HTTP Client section to explain curl spawning philosophy and trade-offs
- Updated all API call code examples

### Token Resolution
- Removed anonymous fallback (was added March 2026)
- Token resolver now errors if no token found, not silent fallback
- Updated resolution documentation to reflect required token

### Output Format
- Removed "Unreleased Changes" section from generated output (was added March 2026)
- Updated code examples and output samples
- Removed unreleased section generation logic from changelog generator code

### Test Coverage
- Removed unreleased detection from test scenario list
- Updated test examples to reflect November 2025 capabilities

### Design Decisions & Limitations
- Reframed "no external HTTP library" as "curl for HTTP" (pragmatic choice, not pure stdlib)
- Updated limitations to be historically accurate
- Added curl as a documented runtime dependency

## Rationale

The post needed to be historically grounded so readers understand the project's evolution:
- What was available in November 2025 (the post date)
- What features arrived later (March 2026 and beyond)
- How the project was used and understood at that time

Without these corrections, readers would encounter code samples and examples that don't match what actually existed in November 2025, creating confusion about the project's development timeline and capabilities.

## Outcome

The post now accurately documents the November 2025 "changelog-generator" baseline while remaining readable to users who know the current "chlogr" project name and March 2026 features. The content is grounded in actual code from that timeframe, with clear framing of later evolutions.

**Verification:** Jekyll site builds successfully with the revised post.
