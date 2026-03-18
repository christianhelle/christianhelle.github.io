# Decision: Azure Messaging with Cabazure Blog Post

**Decided:** 2026-03-18  
**Owner:** Rachael  
**Status:** Completed

## Summary

Published comprehensive blog post "Azure Messaging with Cabazure" (dated 2025-08-18) documenting the public Cabazure.Messaging library for unified Azure messaging patterns.

## What Was Decided

Write a long-form technical blog post demonstrating how Cabazure.Messaging abstracts Azure Event Hubs, Service Bus, and Storage Queue with shared APIs. The post focuses entirely on public library surface (README, sample apps) without exposing internal Teal repository details.

## Why This Decision

Cabazure.Messaging is a high-value library that Christian maintains publicly. The abstraction pattern (shared IMessagePublisher<T> and IMessageProcessor<T> across three transports) is powerful and worth documenting through a practical, conversational post. This content demonstrates thought leadership in .NET distributed systems patterns.

## Technical Details

- **File:** `_posts/2025/2025-08-18-azure-messaging-with-cabazure.md`
- **Word count:** ~3,250 words
- **Sections:** 9 major sections covering abstractions, three transports (with registration + publishing + processing examples each), multi-transport patterns, transport-specific options, getting started
- **Code samples:** C# using WebApplication.CreateBuilder, DI registration, IMessagePublisher/IMessageProcessor patterns, HttpController examples
- **Grounding:** All examples verified against public Cabazure.Messaging README and sample applications
- **Tags:** Azure, Messaging, .NET
- **Jekyll build:** ✅ Clean build, HTML generated successfully

## Key Patterns Used

- **Abstraction sections:** Introduced IMessagePublisher<T>, IMessageProcessor<T>, PublishingOptions, MessageMetadata with interface signatures
- **Transport sections:** Each transport (Event Hub, Service Bus, Queue) follows identical pattern: setup code, publishing example in Controller, processor implementation
- **Multi-transport:** Showed how to register all three in single application with identical application code—value proposition of the abstraction
- **Real-world code:** C# 12, minimal APIs, async/await, nullable annotations, dependency injection patterns
- **Tone:** Technical but accessible, problem-driven narrative, code-heavy explanations

## Outcome

Post is published in blog index (README.md already included entry), validates with Jekyll build, and provides practical reference documentation for Cabazure.Messaging users. Readers can copy patterns directly for their own projects.
