---
layout: post
title: Cosmos DB, Event Sourcing, and CQRS using Chronicles for .NET
date: 2026-03-20
author: Christian Helle
tags:
  - Azure
  - Cosmos DB
  - .NET
  - CQRS
  - Event Sourcing
---

I wrote about this topic earlier in [Cosmos DB, Event Sourcing, and CQRS with A Touch of Class](/2024/02/atc-cosmos-eventstore-cqrs.html), which focused on [Atc.Cosmos.EventStore](https://github.com/atc-net/atc-cosmos-eventstore). Since then I have spent more time with [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) and with [Chronicles](https://github.com/chronicles-net/chronicles), a library created by [Lars Skovslund](https://www.linkedin.com/in/larsskovslund) - the same Lars who originally authored Atc.Cosmos.EventStore. Chronicles is available as a NuGet package under the name `Chronicles`.

What I like about Chronicles is that it treats event streams, commands, projections, documents, and subscriptions as parts of the same system rather than a bag of unrelated helpers. In this post I will use a simple order workflow to show how I would structure a .NET service with Chronicles on top of Cosmos DB, while keeping the examples generic and public-safe.

## Why Chronicles fits Cosmos DB, Event Sourcing, and CQRS

[Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) stores the facts that happened instead of only storing the latest snapshot of state. [CQRS](https://martinfowler.com/bliki/CQRS.html) separates the write side, where commands produce events, from the read side, where projections build documents optimized for queries. Cosmos DB is a good match for this style because it gives you durable, partitioned storage for both event streams and projected read models.

Chronicles gives that architecture a clear shape. A command handler appends events to a stream, a projection consumes those events and writes a document, and a reader queries the document without needing to know anything about the original command flow. That separation is the real value here - the write model protects invariants, while the read model stays cheap and easy to query.

```csharp
var streamId = new OrderStreamId("northwind", "order-123");

var commandResult = await commandProcessorFactory
    .Create<PlaceOrder.Command>()
    .ExecuteAsync(
        streamId,
        new PlaceOrder.Command(
            TenantId: "northwind",
            OrderId: "order-123",
            CustomerId: "customer-42",
            Total: 149.95m,
            PlacedAt: timeProvider.GetUtcNow()),
        requestOptions: null,
        cancellationToken: ct);

var order = await orderQueries.GetAsync("northwind", "order-123", ct);
```

That short example captures the whole mental model. `ExecuteAsync` works against an event stream, not a mutable row. The later query reads a projected document, not the stream itself. Once that separation clicks, the rest of Chronicles starts to feel very natural.

## Events and streams

The first thing I usually model is the language of the stream: which events exist, how they are versioned, and how a stream is identified. Chronicles keeps event definitions simple. Events are plain records, and they are registered with versioned names such as `order-placed:v1`.

I also like that stream identity is explicit. In small systems a simple `new StreamId("order", orderId)` is enough. In multi-tenant systems I prefer to include the tenant in the composite stream ID so the partitioning strategy is obvious from day one.

```csharp
using Chronicles.EventStore;
using Chronicles.EventStore.DependencyInjection;

public static class OrderEvents
{
    public static EventStoreBuilder AddOrderEvents(
        this EventStoreBuilder builder) => builder
        .AddEvent<OrderPlaced>("order-placed:v1")
        .AddEvent<OrderConfirmed>("order-confirmed:v1")
        .AddEvent<OrderCancelled>("order-cancelled:v1");

    public record OrderPlaced(
        string TenantId,
        string OrderId,
        string CustomerId,
        decimal Total,
        DateTimeOffset PlacedAt);

    public record OrderConfirmed(
        string OrderId,
        DateTimeOffset ConfirmedAt);

    public record OrderCancelled(
        string OrderId,
        string Reason,
        DateTimeOffset CancelledAt);
}

public record OrderStreamId(
    string TenantId,
    string OrderId)
    : StreamId(CategoryName, TenantId.ToLowerInvariant(), OrderId)
{
    public const string CategoryName = "order";

    public static OrderStreamId FromStreamId(StreamId streamId)
        => ((string)streamId).Split('.') switch
        {
            [CategoryName, { } tenantId, { } orderId] => new(tenantId, orderId),
            _ => throw new InvalidOperationException($"Invalid stream id '{streamId}'"),
        };
}
```

The practical rule here is simple: once an event is published, treat it as history. Do not mutate it, and do not silently reuse the same name for a different shape later. If the event changes, introduce `v2`. That discipline is what makes replay, projections, and long-lived systems viable.

## Command handlers and state projections

Chronicles gives you a few different command handler shapes, and I think that is one of its strongest design choices. Some commands are naturally stateless - for example, creating a new order. Others need to replay enough stream state to decide whether a change is allowed - for example, confirming an order that might already have been cancelled.

For new streams I usually start with `IStatelessCommandHandler<TCommand>` and a `CommandOptions` requirement that the stream must be new. That keeps the fast path very simple.

```csharp
using Chronicles.Cqrs;
using Chronicles.EventStore;

public static class PlaceOrder
{
    public static readonly CommandOptions Options = new()
    {
        RequiredState = StreamState.New,
    };

    public record Command(
        string TenantId,
        string OrderId,
        string CustomerId,
        decimal Total,
        DateTimeOffset PlacedAt);

    public class Handler : IStatelessCommandHandler<Command>
    {
        public ValueTask ExecuteAsync(
            ICommandContext<Command> context,
            CancellationToken cancellationToken)
            => context
                .AddEvent(
                    new OrderEvents.OrderPlaced(
                        context.Command.TenantId,
                        context.Command.OrderId,
                        context.Command.CustomerId,
                        context.Command.Total,
                        context.Command.PlacedAt))
                .WithResponse(_ => new { context.Command.OrderId, Status = "Placed" })
                .AsAsync();
    }
}
```

When the handler needs history, I switch to `ICommandHandler<TCommand, TState>` and replay only the state I need. This is usually cleaner than reading a full document just to check a couple of flags.

```csharp
public static class ConfirmOrder
{
    public record Command(
        string TenantId,
        string OrderId,
        DateTimeOffset ConfirmedAt);

    public record State(
        bool Placed = false,
        bool Confirmed = false,
        bool Cancelled = false);

    public class Handler : ICommandHandler<Command, State>
    {
        public State CreateState(StreamId streamId) => new();

        public State? ConsumeEvent(StreamEvent evt, State state)
            => evt.Data switch
            {
                OrderEvents.OrderPlaced => state with { Placed = true },
                OrderEvents.OrderConfirmed => state with { Confirmed = true },
                OrderEvents.OrderCancelled => state with { Cancelled = true },
                _ => state,
            };

        public ValueTask ExecuteAsync(
            ICommandContext<Command> context,
            State state,
            CancellationToken cancellationToken)
        {
            if (!state.Placed || state.Confirmed || state.Cancelled)
            {
                context.Response = new { context.Command.OrderId, Status = "Ignored" };
                return ValueTask.CompletedTask;
            }

            return context
                .AddEvent(new OrderEvents.OrderConfirmed(
                    context.Command.OrderId,
                    context.Command.ConfirmedAt))
                .WithResponse(_ => new { context.Command.OrderId, Status = "Confirmed" })
                .AsAsync();
        }
    }
}
```

There is also a third shape, `ICommandHandler<TCommand>`, where you manage replay through `IStateContext` yourself. I tend to use that when I want an even smaller, hand-built slice of state. Regardless of which handler shape you choose, the command result will tell you whether the stream changed, did not change, or conflicted. That makes it straightforward to model idempotent APIs and safe retries.

## Documents and document projections

On the read side, Chronicles works with documents that implement `IDocument` and projections that implement `IDocumentProjection<TDocument>`. The document is your query model in Cosmos DB. The projection is the piece that translates stream history into that document shape.

I like to keep projected documents boring. They should be explicit about IDs and partition keys, easy to query, and free from write-side logic. If a projection owns a document, I want all state changes for that document to come from events.

```csharp
using System.Text.Json.Serialization;
using Chronicles.Cqrs;
using Chronicles.Documents;
using Chronicles.EventStore;

[ContainerName(ContainerName)]
public record OrderDocument(
    string TenantId,
    string OrderId)
    : IDocument
{
    public const string ContainerName = "order";
    private const string PartitionKeyPrefix = ContainerName + ".";

    public static string CreateDocumentId(string orderId) => orderId;

    public static string CreatePartitionKey(string tenantId)
        => string.Concat(PartitionKeyPrefix, tenantId.ToLowerInvariant());

    [JsonPropertyName("id")]
    public string Id { get; init; } = CreateDocumentId(OrderId);

    [JsonPropertyName("pk")]
    public string Pk { get; init; } = CreatePartitionKey(TenantId);

    public string CustomerId { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Status { get; init; } = "Draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public DateTimeOffset Timestamp { get; set; }

    public string GetDocumentId() => Id;
    public string GetPartitionKey() => Pk;
}
```

The projection builds that document incrementally. `CreateState` gives you a blank document from the stream ID, `ConsumeEvent` handles each event, and `OnCommitAsync` gives you one last hook before the document is written back to Cosmos DB.

```csharp
public class OrderProjection : IDocumentProjection<OrderDocument>
{
    public OrderDocument CreateState(StreamId streamId)
        => OrderStreamId.FromStreamId(streamId) switch
        {
            { } id => new(id.TenantId, id.OrderId),
        };

    public ValueTask<DocumentCommitAction> OnCommitAsync(
        OrderDocument document,
        CancellationToken cancellationToken)
    {
        document.Timestamp = DateTimeOffset.UtcNow;
        return ValueTask.FromResult(DocumentCommitAction.Update);
    }

    public OrderDocument? ConsumeEvent(
        StreamEvent evt,
        OrderDocument state)
        => evt.Data switch
        {
            OrderEvents.OrderPlaced data => state with
            {
                CustomerId = data.CustomerId,
                Total = data.Total,
                Status = "Placed",
                CreatedAt = data.PlacedAt,
            },

            OrderEvents.OrderConfirmed data => state with
            {
                Status = "Confirmed",
                ConfirmedAt = data.ConfirmedAt,
            },

            OrderEvents.OrderCancelled data => state with
            {
                Status = "Cancelled",
                CancelledAt = data.CancelledAt,
            },

            _ => null,
        };
}
```

Returning `null` for unhandled events is a nice touch because it makes projection intent obvious. If you need post-commit publishing, Chronicles also supports `IDocumentPublisher<TDocument>` and `AddPublishingDocumentProjection`, which is a clean way to react to a committed read model without pushing that concern back into the aggregate handler.

## Dependency injection and wiring up Chronicles

Once the domain types exist, wiring up Chronicles is mostly about three things: registering documents, registering events, and registering commands plus subscriptions. The Cosmos DB configuration lives in `DocumentOptions`, and the event store plus CQRS pipeline hang off `AddChronicles`.

In a real service I would keep the document options in a dedicated `IConfigureOptions<DocumentOptions>` class and let application configuration decide whether to use a connection string or credentials.

```csharp
using Chronicles.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

public class ConfigureDocumentOptions(IConfiguration configuration)
    : IConfigureOptions<DocumentOptions>
{
    public void Configure(DocumentOptions options)
    {
        options.UseDatabase("Sales");
        options.AddDocumentType<OrderDocument>(OrderDocument.ContainerName);

        options.UseConnectionString(
            configuration.GetConnectionString("cosmos")!);

        options.AddInitialization(o => o
            .CreateDatabase()
            .CreateContainer<OrderDocument>(p =>
            {
                p.Id = OrderDocument.ContainerName;
                p.PartitionKeyPath = "/pk";
            })
            .CreateEventStore());
    }
}
```

The service registration itself stays quite readable. I generally group command registrations in an extension method so `Program.cs` or the composition root can stay focused on the workflow rather than individual types.

```csharp
using Chronicles.Cqrs.DependencyInjection;
using Chronicles.EventStore.DependencyInjection;

public static class OrdersDependencyInjectionExtensions
{
    public static CqrsBuilder AddOrderCommands(
        this CqrsBuilder cqrs) => cqrs
        .AddCommand<PlaceOrder.Command, PlaceOrder.Handler>(PlaceOrder.Options)
        .AddCommand<ConfirmOrder.Command, ConfirmOrder.Handler, ConfirmOrder.State>();

    public static IServiceCollection AddOrderWorkflow(
        this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureDocumentOptions>();

        services.AddChronicles(b => b
            .WithEventStore(evtStore => evtStore
                .AddOrderEvents()
                .AddEventSubscription("order-projections", s => s
                    .AddExceptionHandler<OrderSubscriptionExceptionHandler>()
                    .MapStream(OrderStreamId.CategoryName, c => c
                        .AddDocumentProjection<OrderDocument, OrderProjection>()
                        .AddEventProcessor<OrderNotificationsProcessor>()))
                .WithCqrs(cqrs => cqrs.AddOrderCommands())));

        return services;
    }
}
```

That builder chain is one of the reasons I find Chronicles pleasant to work with. You can see the write model, read model, and subscription setup in one place. The system is still event-sourced underneath, but the application wiring does not turn into a maze.

## Document read and write patterns

Projected documents are meant to be read often, so `IDocumentReader<TDocument>` ends up in query handlers, API endpoints, and background jobs. The two methods I reach for most are `FindAsync`, which returns `null` when the document is missing, and `PagedQueryAsync`, which keeps large result sets honest with continuation tokens.

For direct document writes I try to be deliberate. I do not like writing projection-owned documents outside their projection. Where `IDocumentWriter<TDocument>` really shines is for supporting documents such as drafts, export jobs, reservations, caches, or short-lived workflow state.

```csharp
using Chronicles.Documents;

public class OrderQueries(IDocumentReader<OrderDocument> reader)
{
    public Task<OrderDocument?> GetAsync(
        string tenantId,
        string orderId,
        CancellationToken ct)
        => reader.FindAsync(
            OrderDocument.CreateDocumentId(orderId),
            OrderDocument.CreatePartitionKey(tenantId),
            cancellationToken: ct);

    public Task<PagedResult<OrderDocument>> ListConfirmedAsync(
        string tenantId,
        int? maxItemCount,
        string? continuationToken,
        CancellationToken ct)
        => reader.PagedQueryAsync<OrderDocument>(
            reader.CreateQuery(q => q
                .Where(order => order.Status == "Confirmed")
                .OrderByDescending(order => order.Timestamp)),
            OrderDocument.CreatePartitionKey(tenantId),
            maxItemCount,
            continuationToken,
            options: null,
            cancellationToken: ct);
}
```

Here is a small supporting document that uses TTL. This is the kind of thing I would happily write directly with `IDocumentWriter<TDocument>` because it is operational state, not aggregate history.

```csharp
using System.Text.Json.Serialization;

[ContainerName(ContainerName)]
public record OrderReservationDocument(
    string TenantId,
    string ReservationId)
    : IDocument
{
    public const string ContainerName = "order-reservation";
    private const string PartitionKeyPrefix = ContainerName + ".";

    public static string CreatePartitionKey(string tenantId)
        => string.Concat(PartitionKeyPrefix, tenantId.ToLowerInvariant());

    [JsonPropertyName("id")]
    public string Id { get; init; } = ReservationId;

    [JsonPropertyName("pk")]
    public string Pk { get; init; } = CreatePartitionKey(TenantId);

    [JsonPropertyName("ttl")]
    public int? ExpirationInSeconds { get; init; }

    public string OrderId { get; init; } = string.Empty;

    public string GetDocumentId() => Id;
    public string GetPartitionKey() => Pk;
}

public class OrderReservationWriter(
    IDocumentWriter<OrderReservationDocument> writer)
{
    public Task ReserveAsync(
        string tenantId,
        string orderId,
        CancellationToken ct)
        => writer.WriteAsync(
            new OrderReservationDocument(
                tenantId,
                Guid.NewGuid().ToString("N"))
            {
                OrderId = orderId,
                ExpirationInSeconds = 900,
            },
            ct);

    public Task<bool> ReleaseAsync(
        string tenantId,
        string reservationId,
        CancellationToken ct)
        => writer.TryDeleteAsync(
            reservationId,
            OrderReservationDocument.CreatePartitionKey(tenantId),
            ct);
}
```

If you need optimistic concurrency for operational documents, `UpdateAsync` and `UpdateOrCreateAsync` are the next tools to reach for. The important thing is keeping the ownership model clear: events own projected read models, while direct writers own supporting documents that are not derived from stream history.

## Event subscriptions

Event subscriptions are where Chronicles starts to feel like a full application framework rather than only an event store. A subscription can update projections, trigger side effects, or hand off work to other components. That makes it a good place for notifications, orchestration, audit logging, and integration boundaries.

The key trade-off is consistency. The event is durable as soon as it is written, but the side effect happens asynchronously. That is usually what I want, but it does mean I keep true business invariants inside command handlers and reserve subscriptions for follow-up work.

```csharp
using Chronicles.EventStore;

public class OrderNotificationsProcessor(
    INotificationService notifications)
    : IEventProcessor
{
    public async ValueTask ConsumeAsync(
        StreamEvent evt,
        IStateContext state,
        bool hasMore,
        CancellationToken cancellationToken)
    {
        switch (evt.Data)
        {
            case OrderEvents.OrderConfirmed confirmed:
                await notifications.SendOrderConfirmedAsync(
                    confirmed.OrderId,
                    cancellationToken);
                break;

            case OrderEvents.OrderCancelled cancelled:
                await notifications.SendOrderCancelledAsync(
                    cancelled.OrderId,
                    cancelled.Reason,
                    cancellationToken);
                break;
        }
    }
}

public partial class OrderSubscriptionExceptionHandler(
    ILogger<OrderSubscriptionExceptionHandler> logger)
    : IEventSubscriptionExceptionHandler
{
    public ValueTask HandleAsync(Exception exception)
    {
        logger.LogError(exception, "Order subscription failed");
        return ValueTask.CompletedTask;
    }
}
```

If I need a category-specific workflow, I use `MapStream(OrderStreamId.CategoryName, ...)`. If I need a cross-cutting processor such as an audit logger, `MapAllStreams(...)` is a better fit. Either way, I try to keep processors small and idempotent because subscriptions are exactly where transient infrastructure problems like retries and duplicate handling become very real.

## Testing with AddFakeChronicles

One of the nicest parts of Chronicles is that `AddFakeChronicles()` is a drop-in replacement for the production registrations. That means I can keep the same event, command, and projection configuration in tests without talking to Cosmos DB at all.

I usually wrap that in an `EventStreamScenario`-style base class that starts the hosted subscription, exposes helper methods for command execution and document lookup, and gives me a `FakeTimeProvider` when I need deterministic timestamps.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public abstract class OrderScenario : EventStreamScenario
{
    protected override void ConfigureServices(
        ConfigurationManager configuration,
        ServiceCollection services)
        => services
            .AddLogging()
            .ConfigureOptions<ConfigureDocumentOptions>()
            .AddFakeChronicles(b => b
                .WithEventStore(evtStore => evtStore
                    .AddOrderEvents()
                    .AddEventSubscription("order-projections", s => s
                        .AddExceptionHandler<OrderSubscriptionExceptionHandler>()
                        .MapStream(OrderStreamId.CategoryName, c => c
                            .AddDocumentProjection<OrderDocument, OrderProjection>()))
                    .WithCqrs(cqrs => cqrs.AddOrderCommands())));
}
```

With that in place, the actual tests become small and very readable. The scenario sets up the history, then the assertions check both the command result and the projection.

```csharp
public class ConfirmOrderTests : OrderScenario
{
    private readonly string tenantId = "northwind";
    private readonly string orderId = Guid.NewGuid().ToString("N");

    protected override async ValueTask ConfigureScenarioAsync(CancellationToken ct)
    {
        await ExecuteCommand(
            "place-order",
            new OrderStreamId(tenantId, orderId),
            new PlaceOrder.Command(
                tenantId,
                orderId,
                "customer-42",
                149.95m,
                TimeProvider.GetUtcNow()));

        AdvanceTime(TimeSpan.FromMinutes(1));

        await ExecuteCommand(
            "confirm-order",
            new OrderStreamId(tenantId, orderId),
            new ConfirmOrder.Command(
                tenantId,
                orderId,
                TimeProvider.GetUtcNow()));
    }

    [Fact]
    public void Command_should_succeed()
        => Assert.Equal(ResultType.Changed, LastCommandResult!.Result);

    [Fact]
    public async Task Projection_should_be_updated()
    {
        var document = await FindDocumentAsync<OrderDocument>(
            OrderDocument.CreateDocumentId(orderId),
            OrderDocument.CreatePartitionKey(tenantId));

        Assert.NotNull(document);
        Assert.Equal("Confirmed", document!.Status);
    }

    [Fact]
    public async Task Stream_should_contain_two_events()
    {
        var events = await GetStreamEventsAsync(
            new OrderStreamId(tenantId, orderId));

        Assert.Equal(2, events.Length);
        Assert.IsType<OrderEvents.OrderPlaced>(events[0].Data);
        Assert.IsType<OrderEvents.OrderConfirmed>(events[1].Data);
    }
}
```

That is exactly the kind of test shape I want in an event-sourced system. It verifies the event stream, the resulting read model, and the command outcome without needing a real Cosmos DB instance. For me, that is where Chronicles becomes especially practical rather than just architecturally interesting.

## Practical guidance, trade-offs, and when Chronicles is a good fit

Chronicles is a good fit when you care about an audit trail, when your read models differ from your write model, and when Cosmos DB is already a natural storage choice for the service you are building. It is also a good fit when you want event-driven workflows without hand-rolling the whole stack yourself.

It is not the first thing I would choose for a tiny CRUD application, for a domain dominated by ad-hoc relational joins, or for a team that is not ready to think in terms of eventual consistency. Event sourcing pays off, but it does ask more from both the design and the people maintaining it.

```csharp
var result = await commandProcessorFactory
    .Create<ConfirmOrder.Command>()
    .ExecuteAsync(
        new OrderStreamId(tenantId, orderId),
        new ConfirmOrder.Command(
            tenantId,
            orderId,
            timeProvider.GetUtcNow()),
        new CommandRequestOptions
        {
            CommandId = $"confirm-{orderId}",
            CorrelationId = Activity.Current?.TraceId.ToString(),
            RequiredVersion = 1,
        },
        ct);

if (result.Result == ResultType.Conflict)
{
    // Reload state, return a 409, or retry based on the workflow.
}
```

That final example is small, but it shows a few of the operational details that matter in production: idempotency through command IDs, traceability through correlation IDs, and explicit concurrency checks through required versions. Those are the kinds of details I want close to the command execution boundary.

A few practical rules have worked well for me:

- Choose your stream ID and partition key strategy early.
- Keep events immutable and version them by name when they evolve.
- Keep business invariants in command handlers, not in subscriptions.
- Let projections own their documents instead of mixing direct writes into them.
- Use paged document queries for anything that can grow.
- Treat eventual consistency as a deliberate design choice, not a surprise.

## Conclusion

Chronicles is interesting to me because it treats Cosmos DB, Event Sourcing, and CQRS as one coherent programming model. Commands append facts, projections build documents, readers query those documents, subscriptions react to change, and tests can run entirely in memory with `AddFakeChronicles()`.

If you found my older [Cosmos DB, Event Sourcing, and CQRS with A Touch of Class](/2024/02/atc-cosmos-eventstore-cqrs.html) article useful, Chronicles is a natural next thing to explore. And to be very clear about the lineage, the credit for Chronicles and for the original Atc.Cosmos.EventStore belongs to Lars Skovslund.
