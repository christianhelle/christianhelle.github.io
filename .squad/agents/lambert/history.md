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
