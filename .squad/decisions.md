# Decisions

## 2026-03-20

### Initialization
- Initialized Squad state in this worktree for Christian Helle's Blog.
- Cast the active team from the Alien universe for this repository.
- The current delivery is a Chronicles-focused long-form post that credits Lars Skovslund and avoids Teal-specific details.

### Blog Style Guide (Hicks — Editor)

**Status:** Adopted

Established a unified style guide for Christian Helle's Blog based on analysis of two recent posts (*clocz* and *argiope*). This guide ensures all future posts maintain consistent voice, structure, and readability while preserving the author's authentic tone.

**Key Elements:**
- **Voice:** Conversational, direct, practical; avoid corporate jargon and hype
- **Structure:** 6-part arc (Hook → How It Works → Implementation → Usage → Distribution → Conclusion)
- **Code Blocks:** 15–40 lines per block; use ellipsis to compress non-essential lines; comments explain *why*, not *what*
- **Front Matter:** layout, title, date, author (Christian Helle), tags (specific, lowercase)
- **Post Length:** ~2000–2500 words (medium-long form)

**Rationale:** Consistency builds reader trust and reduces editorial friction. Documented patterns enable faster drafting without losing authentic voice.

**Scope:** Applies to all technical blog posts going forward.

---

### Chronicles Post Review Criteria (Lambert — Technical Reviewer)

**Status:** Approved for Gate Enforcement

The Chronicles technical post will be evaluated against a **structured 6-point review checklist** that prioritizes technical accuracy, Teal-specific content removal, comprehensive coverage, domain neutrality, proper attribution, and editorial quality.

**Review Gates (Non-Negotiable):**
1. **Technical Accuracy** — All code patterns, API contracts, and architecture decisions match Chronicles library reference documentation
2. **Teal-Specific Content Removal** — Zero mentions of internal Teal services, domain models (ChargingSession, Charger, Connector), or infrastructure patterns
3. **Comprehensive Coverage** — All five core abstractions (IDocument, IDocumentProjection, ICommandHandler variants, IDocumentReader, IDocumentWriter) with real examples
4. **Domain Neutrality** — All examples must use generic, independently understandable domains (Bank Accounts, Blog Posts, Shopping Carts, etc.)
5. **Proper Attribution** — Lars Skovslund credited early for CQRS/Event Sourcing mentorship; Chronicles library authors acknowledged
6. **Editorial Quality** — Logical flow, clear explanations, runnable code examples, proper formatting

**Acceptance Criteria:** Post achieves ACCEPT status when all six gates pass verification.

**Review Workflow:** Draft → Assessment → Outcome (ACCEPT / REQUEST REVISIONS / REJECT) → Revision (if needed) → Publication.

**Enforcement Notes:**
- Technical accuracy is non-negotiable; all code examples must be testable against Chronicles NuGet package
- Teal content removal is a hard gate; any mention of Teal infrastructure triggers revision flag
- Attribution is a respect gate; failure to credit Lars or Chronicles authors will be flagged
- Domain neutrality protects IP; generic examples demonstrate patterns without exposing Teal's multi-tenant architecture

**Rationale:** Ensures public safety (no IP leakage), technical correctness, and comprehensive standalone educational value.

---

### Chronicles Blog Post Structure & Content Strategy (Dallas — Chronicles Researcher)

**Status:** Ready for Editorial Review

The blog post "Cosmos DB, Event Sourcing, and CQRS using Chronicles for .NET" should follow a **4-part progressive structure** designed to serve readers with different expertise levels (newcomers to event sourcing, experienced .NET engineers, architects).

**Recommended Article Structure:**
1. **Foundation (Conceptual)** — What and why (event sourcing, CQRS, Cosmos DB integration)
2. **Core Concepts & APIs** — Chronicle's core abstractions (Events, Streams, Documents, Projections, Commands, DI)
3. **Practical Patterns** — Hands-on usage (event registration, projection building, command execution, testing)
4. **Advanced Topics (Optional)** — Optional deep dives (Cosmos integration, scaling, error handling, sagas)

**Lars Skovslund Attribution Strategy:**
- **Full credit for creation:** "Chronicles was created by Lars Skovslund."
- **Historical context:** "Chronicles evolved from earlier event sourcing work (Atc.Cosmos.EventStore), building on lessons learned to provide a comprehensive, production-hardened library designed specifically for Cosmos DB."
- **Implementation:** Mention Lars in introduction; link to Chronicles repository; reference Atc.Cosmos.EventStore as evolutionary predecessor (not detailed); acknowledge inspiration without claiming direct lineage

**De-Teal-ification Strategy:**

All code examples must avoid exposing Teal-specific implementation details.

**Approved Domains (Generic, Universal):**
- ✅ E-Commerce Order Management (OrderRequested, OrderShipped, OrderDelivered)
- ✅ SaaS Tenant Provisioning (TenantSignedUp, TenantActivated, TenantDeleted)
- ✅ Financial Account Transactions (FundsDeposited, FundsWithdrawn, InterestApplied)
- ✅ Blog Post Publishing (PostDrafted, PostPublished, PostArchived)

**Prohibited Domains (Teal-Specific):**
- ❌ Charging sessions, OCPP, EV chargers, connectors
- ❌ Multi-tenant roaming, load management, pricing services
- ❌ Device provisioning, user authorization
- ❌ Internal Teal stream naming (SessionStreamId, RemoteSessionTokenDocument, etc.)

**Code Example Philosophy:**
1. **Length:** 20–40 lines per example (readable in one screen)
2. **Completeness:** Show registration → definition → usage (but no boilerplate)
3. **Realism:** Use async/await, proper error handling, DI patterns
4. **Comments:** Only on *why* decisions are made, not *what* code does
5. **Variants:** Show all three command handler patterns; show projection patterns
6. **Testing:** Include at least one testing example with AddFakeChronicles()
7. **No Teal copy-paste:** Even if patterns are from Teal, rewrite conceptually

**Risk Mitigation Strategy:**

| Risk | Mitigation |
|------|-----------|
| **Cosmos DB specifics** | Frame as "Cosmos example"; explain *why* (partition keys, performance) not *how* (DBA tuning) |
| **Custom fakes** | Present EventStreamScenario as idiomatic, not exclusive; note projects can extend |
| **Eventual consistency** | Explicitly discuss consistency guarantees; clarify projections are eventually consistent but events durable |
| **Sagas/distributed transactions** | Mention as advanced topic; note event orchestrators enable choreography, detail optional |
| **Event versioning** | Explain versioned name pattern; note migration strategies are project-specific |
| **Performance benchmarks** | Avoid specific throughput claims; state "designed for performance; workload-dependent" |
| **Version targeting** | Specify Chronicles version early; note API may vary in other versions |

**Pre-Publishing Validation Checklist:**
- [ ] All code examples compile and run as written
- [ ] Zero Teal-specific terminology or patterns in examples
- [ ] Lars Skovslund credited prominently in introduction
- [ ] Atc.Cosmos.EventStore mentioned as evolutionary predecessor
- [ ] All core abstractions (events, streams, documents, projections, commands, DI, I/O, subscriptions, testing) reflected in content
- [ ] Consistency guarantees (eventual consistency of projections) explicitly discussed
- [ ] Testing section includes AddFakeChronicles() example
- [ ] No claims about performance without backing data
- [ ] Chronicles version number specified
- [ ] Links to Chronicles repository and documentation are canonical/current
- [ ] Peer review by team member with Chronicles expertise completed
- [ ] No internal Teal architecture exposed in any section

**Rationale:** Progressive disclosure serves readers with different expertise levels. Generic examples protect IP while demonstrating reusable patterns. Version targeting and consistency guarantees prevent user confusion.

**Next Steps:**
1. Editorial team reviews and approves structure
2. Writer completes blog post draft following approved structure
3. Peer review for technical accuracy (Chronicles expertise required)
4. Final edits for tone, flow, and completeness
5. Lambert's review checklist pass → Publication
