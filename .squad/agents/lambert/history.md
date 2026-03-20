# History

## Project Context

- User: Christian Helle
- Project: Christian Helle's Blog
- Stack: Jekyll, Ruby/Bundler, Markdown content, .NET Playwright tests
- Current focus: Write a comprehensive Chronicles article for the blog

## Learnings

- Squad initialized on 2026-03-20.
- Created comprehensive review checklist for Chronicles technical post on 2026-03-20.
- **Chronicles Knowledge Base**: Mastered five core abstractions:
  1. **IDocument + [ContainerName]** — Read models stored in Cosmos with partition key pattern `"container.tenantId"`
  2. **IDocumentProjection** — Event consumers that use `ConsumeEvent` pattern matching to build documents
  3. **ICommandHandler** (3 variants) — Stateless, document-driven state, and custom lightweight state patterns
  4. **IDocumentReader/Writer** — Cosmos operations with tenant scoping via partition keys
  5. **DI Setup** — EventStoreBuilder + DocumentOptions + CqrsBuilder fluent configuration
- **Multi-tenant isolation principle**: Always include TenantId in composite StreamId and partition key patterns
- **Teal boundary awareness**: Generic (Bank, Blog, Shopping Cart) domain examples required; ChargingSession/Connector models are internal
- **Attribution priority**: Lars Skovslund credited as CQRS/Event Sourcing mentor; Chronicles authors acknowledged
- **Review gate criteria**: Technical soundness > Teal leakage removal > Comprehensive coverage > Proper credits
- **Draft review completed 2026-03-20**: Verdict REQUEST REVISIONS. One blocking issue: missing Chronicles repo/package reference. All six review gates passed except actionability — readers have no way to find or install Chronicles. Three advisory notes issued (missing import in reservation code block, eventual consistency could be more explicit near projections, IDocumentPublisher mentioned but not demonstrated). Technical accuracy verified: every code pattern in the draft matches the Chronicles reference documentation precisely. Zero Teal leakage confirmed via regex search. Lars Skovslund attribution is proper and prominent.
- **Review efficiency insight**: The structured 6-gate checklist worked well for systematic verification. The most time-consuming gate was technical accuracy (cross-referencing every API surface). The fastest gate was Teal content removal (automated regex). Future reviews should always start with the automated gates.
- **Key pattern**: Posts that teach library usage MUST include installation/access instructions. This is a hard gate regardless of how good the content is.
- **Final review completed 2026-03-20**: Verdict ACCEPT. The blocking issue (B1: missing Chronicles repo/package reference) was resolved — introduction now includes GitHub link and NuGet package name. All six gates pass on re-review. Three advisory notes remain as optional polish (A1: missing import in reservation block, A2: eventual consistency callout near projections, A3: IDocumentPublisher not demonstrated). Zero technical inaccuracies across all code patterns. Zero Teal leakage confirmed via regex.
- **Two-round review efficiency**: The structured 6-gate checklist proved its value across two review rounds. The first round caught one blocking issue and three advisories. The second round was faster because gates 2–5 (Teal removal, coverage, domain neutrality, attribution) were already clean — only gate 1 (technical accuracy) and the blocker resolution needed full re-verification. Future multi-round reviews should prioritize re-checking only changed areas and previously flagged items.
- **Blocker pattern confirmed**: The B1 pattern (missing installation/access instructions) validates the earlier learning that library-teaching posts MUST include package references. This should remain a first-pass automated check in future reviews.
