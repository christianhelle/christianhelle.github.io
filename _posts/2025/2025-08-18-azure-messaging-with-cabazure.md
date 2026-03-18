---
layout: post
title: Azure Messaging with Cabazure
date: 2025-08-18
author: Christian Helle
tags:
  - Azure
  - Messaging
  - .NET
categories:
  - Libraries
description: A unified abstraction layer for Azure Event Hub, Service Bus, and Storage Queue messaging in .NET
---

Building distributed systems means choosing between Azure messaging transports—Event Hubs for high-throughput streaming, Service Bus for reliable workflows, or Storage Queue for lightweight background jobs. But each transport has its own SDK and quirks. I found myself repeating the same patterns: inject a publisher, implement a processor, handle options, wire up dependency injection. That's boilerplate that should be shared.

That's why I use [Cabazure.Messaging](https://github.com/Cabazure/Cabazure.Messaging)—a .NET library written by [@rickykaare](https://github.com/rickykaare) that unifies all three Azure messaging transports under a single abstraction layer. Define your messages once, publish and process them the same way regardless of transport, and swap implementations with just a configuration change.

## Why I Built a Unified Messaging Layer

When you integrate multiple transports, you repeat code. Event Hub uses one connection style, Service Bus uses another. Event Hub metadata includes partition information, Service Bus includes session details, Storage Queue exposes dequeue count. Each processor needs slightly different setup.

I wanted a library where your business logic never knew which transport it was using. Your controller would inject `IMessagePublisher<OrderCreatedEvent>` and call `PublishAsync`. The same processor code would work if you switched from Event Hub to Service Bus. The framework would handle serialization, configuration, and lifecycle—you focus on logic.

## The Shared Abstractions

All three transports share a common foundation. Once you understand these two interfaces, you can use any transport.

### IMessagePublisher<TMessage>

The publisher is how your application sends messages:

```csharp
public interface IMessagePublisher<in TMessage>
{
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken = default);
    
    Task PublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken = default);
}
```

Inject it, call `PublishAsync`, and the framework routes to your configured transport. No transport-specific code in your business logic.

### IMessageProcessor<TMessage>

Implement this interface to define how you handle each message type:

```csharp
public interface IMessageProcessor<in TMessage>
{
    Task ProcessAsync(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
```

The `MessageMetadata` provides access to transport-level details—message ID, correlation ID, partition or session information, enqueued time—useful for observability and distributed tracing.

### PublishingOptions

When publishing, you can pass optional metadata:

```csharp
public class PublishingOptions
{
    public string? ContentType { get; set; }
    public string? CorrelationId { get; set; }
    public string? MessageId { get; set; }
    public string? PartitionKey { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
```

Each transport extends this with its own options—Event Hub adds `PartitionId`, Service Bus adds `SessionId` and `TimeToLive`, Storage Queue adds `VisibilityTimeout`.

### MessageMetadata

When your processor runs, it receives metadata about the message:

```csharp
public class MessageMetadata
{
    public string? ContentType { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? EnqueuedTime { get; set; }
    public string? MessageId { get; set; }
    public string? PartitionKey { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
```

Transport-specific metadata is added by each implementation—Event Hub provides `PartitionId` and `SequenceNumber`, Service Bus provides session and delivery info, Storage Queue provides `DequeueCount` and visibility.

## Event Hub: Setup, Publishing, Processing

Event Hubs excel at high-throughput telemetry and event streaming. Messages are partitioned and ordered within each partition. It's the right choice when you need to handle millions of events per second.

### Setting Up Event Hub

Register Event Hub with the DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("eventhub")!;
var blobsConnection = builder.Configuration
    .GetConnectionString("blobs")!;

builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(connectionString)
        .WithBlobStorage(blobsConnection))
    .AddPublisher<OrderCreatedEvent>("orders-hub", b => b
        .WithMessageId(e => e.OrderId.ToString())
        .WithPartitionKey(e => e.CustomerId.ToString()))
    .AddProcessor<OrderCreatedEvent, OrderCreatedEventProcessor>(
        "orders-hub", 
        "$Default", 
        b => b.WithBlobContainer("container1", createIfNotExist: true)));

var app = builder.Build();
```

The fluent API configures connection strings and blob storage (for checkpointing). You register publishers and processors for each message type, specifying the event hub name and consumer group. Publisher builders can configure partition keys and message IDs using message property lambdas.

### Publishing Events

Once registered, inject and publish:

```csharp
public class OrderController
{
    private readonly IMessagePublisher<OrderCreatedEvent> _publisher;

    public OrderController(IMessagePublisher<OrderCreatedEvent> publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var @event = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Total = request.Total
        };

        await _publisher.PublishAsync(@event, cancellationToken);
        
        return Ok(new { orderId = @event.OrderId });
    }
}
```

You can optionally provide `EventHubPublishingOptions` to control routing:

```csharp
var options = new EventHubPublishingOptions
{
    PartitionKey = request.CustomerId.ToString(),
    MessageId = Guid.NewGuid().ToString()
};
await _publisher.PublishAsync(@event, options, cancellationToken);
```

The `PartitionKey` ensures messages from the same customer go to the same partition, maintaining order.

### Processing Events

Implement the processor interface. The framework discovers it automatically and manages its lifecycle:

```csharp
public class OrderCreatedEventProcessor : IMessageProcessor<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventProcessor> _logger;
    private readonly IOrderRepository _repository;

    public OrderCreatedEventProcessor(
        ILogger<OrderCreatedEventProcessor> logger,
        IOrderRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task ProcessAsync(
        OrderCreatedEvent message,
        MessageMetadata metadata,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing order {OrderId} from customer {CustomerId}",
            message.OrderId,
            message.CustomerId);

        await _repository.CreateOrderAsync(message, cancellationToken);
    }
}
```

Event Hub metadata includes:

```csharp
var eventHubMetadata = (EventHubMetadata)metadata;
_logger.LogInformation("Partition: {PartitionId}, Sequence: {Sequence}",
    eventHubMetadata.PartitionId,
    eventHubMetadata.SequenceNumber);
```

### Stateless Processing

For scenarios where you don't need checkpointing (read-only or non-critical processing), use stateless processors:

```csharp
builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(connectionString)
        .WithBlobStorage(blobsConnection))
    .AddStatelessProcessor<OrderCreatedEvent, OrderAuditProcessor>(
        "orders-hub", 
        "$Default"));
```

Stateless processors don't persist their position, so every instance reads from the beginning. Use this for analytics, logging, or scenarios where you process the same events multiple times.

### Event Hub Advanced Options

The processor builder supports configuring filters and processor options. Property-based filtering allows you to route messages based on custom metadata, and processor options control batch sizing, wait times, and other Event Hub client behavior. Configure these during processor registration:

```csharp
builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(connectionString)
        .WithBlobStorage(blobsConnection))
    .AddProcessor<OrderCreatedEvent, OrderProcessor>(
        "orders-hub", 
        "$Default", 
        b => b
            .WithFilter(properties => properties.TryGetValue("CustomerId", out var customerId) && customerId is int customerIdValue && customerIdValue > 1000)
            .WithProcessorOptions(new EventProcessorOptions())
            .WithBlobContainer("processor-state", createIfNotExist: true)));
```

The `WithFilter` method accepts a lambda that receives message properties as `IDictionary<string, object>` for filtering. The `WithProcessorOptions` method accepts `EventProcessorOptions` to tune processor behavior without tying you to a specific transport implementation.

## Service Bus: Reliability, Sessions, and Filtering

Service Bus is ideal for critical workflows where reliability matters. It supports sessions (grouping), dead-letter queues, explicit acknowledgment, and message scheduling.

### Setting Up Service Bus

Register Service Bus similarly to Event Hub:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("topic")!;

builder.Services.AddCabazureServiceBus(b => b
    .Configure(o => o
        .WithConnection(connectionString))
    .AddPublisher<PaymentProcessedEvent>(
        "payments-topic",
        b => b.WithProperty(e => e.PaymentId))
    .AddProcessor<PaymentProcessedEvent, PaymentProcessedEventProcessor>(
        topicName: "payments-topic", 
        subscriptionName: "payment-service-subscription"));

var app = builder.Build();
```

You can publish to topics or queues, and subscribe with different consumer names (subscriptions). Publisher builders let you configure custom properties extracted from your message type.

### Publishing with Sessions

Service Bus sessions group related messages. Publish with a session ID to ensure all messages for a user are processed together:

```csharp
public class PaymentController
{
    private readonly IMessagePublisher<PaymentProcessedEvent> _publisher;

    public PaymentController(IMessagePublisher<PaymentProcessedEvent> publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var @event = new PaymentProcessedEvent
        {
            PaymentId = Guid.NewGuid(),
            Amount = request.Amount,
            Status = "Completed"
        };

        var options = new ServiceBusPublishingOptions
        {
            SessionId = request.UserId.ToString(),
            MessageId = @event.PaymentId.ToString(),
            CorrelationId = request.CorrelationId
        };
        
        await _publisher.PublishAsync(@event, options, cancellationToken);
        
        return Ok(@event);
    }
}
```

Session IDs ensure that all messages for a user are delivered to the same processor instance in order—critical for workflows where sequence matters.

### Processing with Filtering

Implement a processor and optionally filter which messages it handles:

```csharp
public class PaymentProcessedEventProcessor : IMessageProcessor<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedEventProcessor> _logger;
    private readonly IPaymentRepository _repository;

    public PaymentProcessedEventProcessor(
        ILogger<PaymentProcessedEventProcessor> logger,
        IPaymentRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task ProcessAsync(
        PaymentProcessedEvent message,
        MessageMetadata metadata,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment {PaymentId} processed for ${Amount}",
            message.PaymentId,
            message.Amount);

        await _repository.UpdatePaymentStatusAsync(message, cancellationToken);
    }
}
```

Service Bus subscription filters let you route messages based on custom properties. Configure filters using a lambda over the message properties:

```csharp
builder.Services.AddCabazureServiceBus(b => b
    .Configure(o => o
        .WithConnection(connectionString))
    .AddProcessor<PaymentProcessedEvent, PaymentProcessedEventProcessor>(
        topicName: "payments-topic",
        subscriptionName: "payment-service-subscription",
        b => b.WithFilter(properties => properties.TryGetValue("Amount", out var amount) && amount is int amountValue && amountValue > 1000)));
```

The `WithFilter` method accepts a lambda that receives message properties as `IReadOnlyDictionary<string, object>` for filtering. Only messages matching the filter criteria will be delivered to this subscription.

### Service Bus Transport Options

Service Bus publishing options let you control time-to-live, scheduling, and more:

```csharp
var options = new ServiceBusPublishingOptions
{
    SessionId = "user-123",
    TimeToLive = TimeSpan.FromHours(24),
    ScheduledEnqueueTime = DateTime.UtcNow.AddHours(1),
    MessageId = Guid.NewGuid().ToString()
};
await _publisher.PublishAsync(@event, options, cancellationToken);
```

`TimeToLive` expires messages if not processed within a duration. `ScheduledEnqueueTime` defers delivery. Sessions group messages logically, ensuring order and cohesion.

## Storage Queue: Lightweight and Simple

Storage Queue is the straightforward option—no frills, low cost, good for background jobs and non-critical messaging. It integrates naturally with Azure Storage.

### Setting Up Storage Queue

Register Storage Queue:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var queuesConnection = builder.Configuration
    .GetConnectionString("queues")!;

builder.Services.AddCabazureStorageQueue(b => b
    .Configure(o => o
        .WithConnection(queuesConnection))
    .AddPublisher<EmailSendRequest>("email-queue")
    .AddProcessor<EmailSendRequest, EmailSendRequestProcessor>("email-queue", b => b
        .WithInitialization(createIfNotExists: true)
        .WithPollingInterval(TimeSpan.FromSeconds(5))));

var app = builder.Build();
```

Storage Queue is simple—one publisher, one processor per queue. The processor builder lets you configure polling intervals and auto-creation behavior.

### Publishing Messages

Publishing is the same API as the other transports:

```csharp
public class NotificationController
{
    private readonly IMessagePublisher<EmailSendRequest> _publisher;

    public NotificationController(IMessagePublisher<EmailSendRequest> publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail(
        SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        var message = new EmailSendRequest
        {
            To = request.Email,
            Subject = "Welcome!",
            Body = "Thanks for signing up."
        };

        var options = new StorageQueuePublishingOptions
        {
            VisibilityTimeout = TimeSpan.FromMinutes(5),
            TimeToLive = TimeSpan.FromDays(7)
        };

        await _publisher.PublishAsync(message, options, cancellationToken);

        return Accepted();
    }
}
```

`VisibilityTimeout` hides a message from other processors while one is working on it (prevents duplicate processing). `TimeToLive` expires messages if not processed within a duration.

### Processing Messages

Implement your processor:

```csharp
public class EmailSendRequestProcessor : IMessageProcessor<EmailSendRequest>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailSendRequestProcessor> _logger;

    public EmailSendRequestProcessor(
        IEmailService emailService,
        ILogger<EmailSendRequestProcessor> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcessAsync(
        EmailSendRequest message,
        MessageMetadata metadata,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending email to {To}", message.To);
        
        await _emailService.SendAsync(
            message.To,
            message.Subject,
            message.Body,
            cancellationToken);
    }
}
```

Storage Queue metadata includes:

```csharp
var sqMetadata = (StorageQueueMetadata)metadata;
_logger.LogInformation("Dequeued {DequeueCount} times", sqMetadata.DequeueCount);
```

### Polling and Initialization

Configure polling interval and whether to auto-create the queue during processor registration:

```csharp
builder.Services.AddCabazureStorageQueue(b => b
    .Configure(o => o
        .WithConnection(queuesConnection))
    .AddProcessor<EmailSendRequest, EmailSendRequestProcessor>(
        "email-queue", 
        b => b
            .WithPollingInterval(TimeSpan.FromSeconds(2))
            .WithInitialization(createIfNotExists: true)));
```

`WithPollingInterval` controls how often the processor checks the queue. `WithInitialization` auto-creates the queue if it doesn't exist.

## Sample App Architecture

The Cabazure.Messaging repository includes sample applications for each transport demonstrating end-to-end message flows. Each sample follows the same pattern:

**Producer console app** — Publishes sample messages and demonstrates the publisher API. Shows how to configure message publishing with custom metadata and batch patterns.

**Processor console app** — Implements `IMessageProcessor<TMessage>` and runs as a long-lived consumer. Demonstrates message handling patterns and graceful shutdown.

**AppHost project** — Coordinates the sample services and their local dependencies. Provides orchestration for running complete scenarios across all three transports.

**ServiceDefaults library** — Shares logging, health checks, telemetry, and common host setup across all sample applications. Ensures consistent configuration and observability patterns.

The sample projects demonstrate realistic configuration and DI patterns for each transport. Clone the repository and run the samples to see Event Hub, Service Bus, and Storage Queue in action.

## Multi-Transport Example

Here's where the abstraction really shines—using multiple transports in one application:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Streaming telemetry with Event Hub
builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(builder.Configuration.GetConnectionString("eventhub")!)
        .WithBlobStorage(builder.Configuration.GetConnectionString("blobs")!))
    .AddPublisher<TelemetryEvent>("telemetry-hub")
    .AddProcessor<TelemetryEvent, TelemetryProcessor>("telemetry-hub", "$Default"));

// Reliable orders with Service Bus
builder.Services.AddCabazureServiceBus(b => b
    .Configure(o => o
        .WithConnection(builder.Configuration.GetConnectionString("servicebus")!))
    .AddPublisher<OrderEvent>("orders-topic")
    .AddProcessor<OrderEvent, OrderProcessor>(
        topicName: "orders-topic", 
        subscriptionName: "order-subscription"));

// Background jobs with Storage Queue
builder.Services.AddCabazureStorageQueue(b => b
    .Configure(o => o
        .WithConnection(builder.Configuration.GetConnectionString("queues")!))
    .AddPublisher<EmailSendRequest>("email-queue")
    .AddProcessor<EmailSendRequest, EmailProcessor>("email-queue"));

var app = builder.Build();
app.Run();
```

Your application code never branches on transport type. Controllers inject `IMessagePublisher<OrderEvent>` without knowing or caring whether it goes to Service Bus, Event Hub, or Storage Queue. Switch transports by changing configuration, not code.

## Why the Abstraction Matters in Practice

I built Cabazure.Messaging because I found myself copy-pasting the same setup code for each project. Every time I added a new message type, I repeated: register the publisher, implement the processor, wire up DI, handle options.

The abstraction solves real problems:

**One API, any transport.** Your business logic never sees transport-specific code. An event handler looks the same whether you're using Event Hub or Service Bus.

**Easy migration.** If you discover Event Hub doesn't fit your scale or Service Bus offers better guarantees, you swap one configuration block. No code changes.

**Consistency across teams.** Everyone uses the same patterns—same publisher injection, same processor implementation, same options structure. Onboarding is faster.

**Built-in observability.** Correlation IDs, message IDs, and partition info flow through metadata. You get distributed tracing for free.

**Reduced boilerplate.** The library handles serialization, lifecycle management, and error handling. You focus on your business logic.

## Getting Started

Cabazure.Messaging packages are on NuGet:

```bash
# Install the transport you need
dotnet add package Cabazure.Messaging.EventHub
dotnet add package Cabazure.Messaging.ServiceBus
dotnet add package Cabazure.Messaging.StorageQueue
```

The [GitHub repository](https://github.com/christianhelle/Cabazure.Messaging) has sample applications for each transport showing a complete end-to-end flow: Producer publishes events, Processor handles them, AppHost orchestrates everything locally.

Clone the repo, run the AppHost, and see all three transports in action. The sample projects are your best reference for configuration and real-world usage patterns.

## Conclusion

Azure messaging is powerful, but the SDKs are transport-specific. Cabazure.Messaging gives you a unified abstraction—one way to publish, one way to process, regardless of whether you're building real-time telemetry pipelines, reliable order workflows, or lightweight background job queues.

The library handles the details. You focus on what your messages mean and how to respond to them. And if your needs evolve, you can swap transports without touching your business logic.

If you're building distributed systems on Azure, give it a try. The repository includes everything you need to get started, and the abstraction will save you time and headaches as your system scales.
