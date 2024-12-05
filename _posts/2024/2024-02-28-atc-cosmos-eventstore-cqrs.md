---
layout: post
title: Cosmos DB, Event Sourcing, and CQRS with A Touch of Class
date: '2024-02-28'
author: Christian Helle
tags:
- Azure
- Cosmos DB
redirect_from:
- /2024/02/atc-cosmos-eventstore-cqrs/
- /2024/02/atc-cosmos-eventstore-cqrs
- /2024/atc-cosmos-eventstore-cqrs/
- /2024/atc-cosmos-eventstore-cqrs
- /atc-cosmos-eventstore-cqrs/
- /atc-cosmos-eventstore-cqrs
---

For the past 6 or 7 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) as my go-to data store. Document databases make so much more sense for the things that I have been building over these past years. In an [old post](/2023/02/atc-cosmos.html), I wrote about a library called [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) that I was part of building that I use for configuring containers in Azure Cosmos DB to provides easy, efficient, and convenient ways to read and write document resources.

One of the things I use Azure Cosmos DB is for implementing [CQRS](https://www.eventstore.com/cqrs-pattern), a pattern I first heard about from [Mark Seemann](https://blog.ploeh.dk/), an old colleague from a decade and a half ago. I first started really working with Event Sourcing and CQRS 6 or 7 years ago, when I started working with a colleague named [Lars Skovslund](https://www.linkedin.com/in/larsskovslund) 

I must begin this post by stating that I am in no way an expert in the subject and this article is about implementing the pattern with Azure Cosmos DB using a library called [Atc.Cosmos.EventStore](https://github.com/atc-net/atc-cosmos-eventstore)

## Getting started with Atc.Cosmos.EventStore

Let's keep it simple and start off with a simple example .NET 8 Console App that records a few events using commands, and build a read-model using a projection job.

### Packages

To get started, let's bring in the [Atc.Cosmos.EventStore.Cqrs](https://www.nuget.org/packages/Atc.Cosmos.EventStore.Cqrs) NuGet package and for convenience let's add the [Atc.Cosmos](https://www.nuget.org/packages/Atc.Cosmos) package as well

```xml
<PackageReference Include="Atc.Cosmos.EventStore.Cqrs" Version="1.12.6" />
<PackageReference Include="Atc.Cosmos" Version="1.1.40" />
```

To be able to run background jobs in a console app to implement a [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder). To get started with implementing a generic host we need to import the [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting) package reference

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
```

### Configuration

Like any other modern .NET application, we configure our dependencies and services from `IServiceCollection`

```csharp
void ConfigureServices(IServiceCollection services)
{
}
```

To setup the event store database, you need to call the `AddEventStore()` extension method. Do this when you configure services used by your system

```csharp
services.AddEventStore(
    builder =>
    {
        builder.UseCosmosDb();
        builder.UseCQRS(
            c =>
            {
                c.AddInitialization(
                    4000,
                    serviceProvider => serviceProvider
                        .GetRequiredService<ICosmosInitializer>()
                        .InitializeAsync(CancellationToken.None));
            });
        });
```

The event store requires that you configure options for `EventStoreClientOptions` so let's implement `IConfigureOptions<EventStoreClientOptions>`

```csharp
public class ConfigureEventStoreOptions : IConfigureOptions<EventStoreClientOptions>
{
    public void Configure(EventStoreClientOptions options)
    {
        options.UseCosmosEmulator();
        options.EventStoreDatabaseId = "CQRS";
    }
}
```

And register the options to `IServiceCollection` using

```csharp
services.ConfigureOptions<ConfigureEventStoreOptions>();
```

The event store library will create the database and the required containers if they do not already exist. When you use the library from non-local environments you will need to ensure that the application using the library has rights to create a cosmos database and containers. If you use managed identity from Microsoft Azure, then I suggest that you provision the database and required containers from your deployment pipeline.

The required containers are the following:

- event-store
- stream-index
- subscriptions

### Events

To define events, you need to do is to create a record (or class) that is decorated with the `StreamEvent` attribute from the `Atc.Cosmos.EventStore.Cqrs` namespace

```csharp
[StreamEvent("added-event:v1")]
public record AddedEvent(string Name, string Address);
```

The Atc.Cosmos.EventStore library can detect all events within an assembly, this needs to be configured from the `AddEventStore()` extension method

```csharp
services.AddEventStore(
    builder =>
    {
        builder.UseEvents(catalogBuilder => catalogBuilder.FromAssembly<AddedEvent>());
    });
```

It is very important that stream event records/classes are never deleted. The commands and projections engine in Atc.Cosmos.EventStore require that event types are NEVER deleted or changed. If you want to make changes to these event types then you need to create a new version of it. So if we wanted to modify `added-event:v1`, you would deprecate v1 and introduce a `added-event:v2` event

```csharp
[Obsolete]
[StreamEvent("added-event:v1")]
public record AddedEventV1(string Name, string Address);

[StreamEvent("added-event:v2")]
public record AddedEvent(string FirstName, string LastName, string Address);
```

### Event Stream

Events are always persisted to CosmosDb under a partition key that describes the event stream. To specify the name of the partition key, you need to extend `EventStreamId`and implement `IEquatable<SampleEventStreamId?>`

Let's say that you want to create an event stream called `samples` that contains events for a given session. For this, we want to create an event stream that could be called `samples.[session id]`

```csharp
public sealed class SampleEventStreamId : EventStreamId, IEquatable<SampleEventStreamId?>
{
    private const string TypeName = "samples";
    public const string FilterIncludeAllEvents = TypeName + ".*";

    public SampleEventStreamId(string id)
        : base(TypeName, id)
    {
        Id = id;
    }

    public SampleEventStreamId(EventStreamId id)
        : base(id.Parts.ToArray())
    {
        Id = id.Parts[1];
    }

    public string Id { get; }

    public override bool Equals(object? obj)
        => Equals(obj as SampleEventStreamId);

    public bool Equals(SampleEventStreamId? other)
        => other != null && Value == other.Value;

    public override int GetHashCode()
        => HashCode.Combine(Value);
}
```

### Commands

Commands always come in two parts, a command and a command handler. To implement a command, we extend `CommandBase<EventStreamId>`

```csharp
public record CreateCommand(string Id, string Name, string Address)
    : CommandBase<SampleEventStreamId>(new SampleEventStreamId(Id));
```

To implement a command handler, we implement `ICommandHandler<Command>`

```csharp
public class CreateCommandHandler :
    ICommandHandler<CreateCommand>
{
    public ValueTask ExecuteAsync(
        CreateCommand command,
        ICommandContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
```

To use commands, we need to tell the system where to find these commands. With `Atc.Cosmos.EventStore.Cqrs`, you only need to specify which assembly the commands are available. This is done from `AddEventStore(builder => builder.UseCQRS())` like this:

```csharp
services.AddEventStore(
    builder => builder.UseCQRS(
        cqrs => cqrs.AddCommandsFromAssembly<CreateCommand>()))
```

The outcome of a command is an event. To persist an event, use the `AddEvent()` method of the `ICommandContext`. There are times where there is no outcome because the event had already happened. For example, for session XxxxXxxxXxxx, a user was added using the name and address. The command implementation can prevent inserting a duplicate event by checking if the event had already been recorded. To do this, the command must implement `IConsumeEvent<TEvent>` where `TEvent` is the event in the stream. This looks something like this:

```csharp
public class CreateCommandHandler :
    ICommandHandler<CreateCommand>,
    IConsumeEvent<AddedEvent>
{
    private bool created;

    public void Consume(AddedEvent evt, EventMetadata metadata)
    {
        this.created = true;
    }

    public ValueTask ExecuteAsync(
        CreateCommand command,
        ICommandContext context,
        CancellationToken cancellationToken)
    {
        if (!created)
        {
            context.AddEvent(new AddedEvent(command.Name, command.Address));
        }

        return ValueTask.CompletedTask;
    }
}
```

The `ICommandContext` also exposes the `ResponseObject` property which you can use to return values to the consumer of the command. The `ResponseObject` is a nullable object type and may contain anything, or nothing. Here's an example of the same command we created previously but now it sets the value `true` to the `ResponseObject` when successful, otherwise the value `false` is set to the `ResponseObject`

```csharp
public class CreateCommandHandler :
    ICommandHandler<CreateCommand>,
    IConsumeEvent<AddedEvent>
{
    private bool created;

    public void Consume(AddedEvent evt, EventMetadata metadata)
    {
        this.created = true;
    }

    public ValueTask ExecuteAsync(
        CreateCommand command,
        ICommandContext context,
        CancellationToken cancellationToken)
    {
        if (!created)
        {
            context.AddEvent(new AddedEvent(command.Name, command.Address));
            context.ResponseObject = true;
            return ValueTask.CompletedTask;
        }

        context.ResponseObject = false;
        return ValueTask.CompletedTask;
    }
}
```

It is possible to consume events asynchronously by using `IConsumeEventAsync<TEvent>`. This can be useful for calling external API's for whatever reason

```csharp
public class CreateCommandHandler :
    ICommandHandler<CreateCommand>,
    IConsumeEventAsync<AddedEvent>
{
    private bool created;

    public Task ConsumeAsync(
        AddedEvent evt,
        EventMetadata metadata,
        CancellationToken cancellationToken)
    {
        created = true;
        // Do something
        return Task.CompletedTask;
    }

    public ValueTask ExecuteAsync(
        CreateCommand command,
        ICommandContext context,
        CancellationToken cancellationToken)
    {
        if (!created)
        {
            context.AddEvent(new AddedEvent(command.Name, command.Address));
            context.ResponseObject = true;
        }

        return ValueTask.CompletedTask;
    }
}
```

Executing commands are done through the `ICommandProcessorFactory` interface. This is injected to the `IServiceCollection` IoC container upon `AddEventStore(builder => builder.UseCQRS())`. If we were to execute the `CreateCommand` above, it would look something like this:

```csharp
public class ConsoleHostedService(ICommandProcessorFactory commandProcessorFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var commandResult = await commandProcessorFactory
            .Create<CreateCommand>()
            .ExecuteAsync(
                new CreateCommand(
                    Guid.NewGuid().ToString("N"),
                    "Christian Helle", 
                    "Address 1, 2100 Copenhagen, Denmark"),
                cancellationToken);

        Console.WriteLine("Command Response: " + commandResult.Response);
    }
}
```

### Projections

The `Atc.Cosmos.EventStore.CQRS` library provides infrastructure to run projection jobs. Projections take advantage of the Cosmos DB Change Feed. The change feed in Azure Cosmos DB is a persistent record of changes to a container in the order they occur, which is perfect for Event Sourcing and CQRS as the order of events matter a lot. When introducing a new projection job it will by default start with events from the begining of the `event-store` container. To create projection jobs, you need to implement the `IProjection` interface. The `IProjection` implementation must be decorated with the `[ProjectionFilter]` attribute to specify the stream to read. Projections can be used to build read-models based on the events that have occurred.

```csharp
[ProjectionFilter(SampleEventStreamId.FilterIncludeAllEvents)]
public class SampleProjection : IProjection
{
    public Task<ProjectionAction> FailedAsync(
        Exception exception,
        CancellationToken cancellationToken)
        => Task.FromResult(ProjectionAction.Continue);

    public Task InitializeAsync(
        EventStreamId id,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task CompleteAsync(
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}
```

In the example above, the projection job will execute on all streams where the filter applies. The `InitializeAsync()` method is invoked every time an event is written or updated. Use the `InitializeAsync()` method to load the last known state of the read-model the projection built or prepare the initial state required for the read-model that is going to build. The `CompleteAsync()` method should be used to persist the changes to the read-model.

Here's an example of a projection that builds a read-model based on the events that have occurred. The example will also handle deletion events by deleting the read-model from the persistent store

```csharp
[ProjectionFilter(SampleEventStreamId.FilterIncludeAllEvents)]
public class SampleProjection(
    ICosmosReader<SampleReadModel> reader,
    ICosmosWriter<SampleReadModel> writer) :
    IProjection,
    IConsumeEvent<AddedEvent>,
    IConsumeEvent<NameChangedEvent>,
    IConsumeEvent<AddressChangedEvent>,
    IConsumeEvent<DeletedEvent>
{
    private SampleReadModel view = null!;
    private bool deleted = false;

    public Task<ProjectionAction> FailedAsync(
        Exception exception,
        CancellationToken cancellationToken) =>
        Task.FromResult(ProjectionAction.Continue);

    public async Task InitializeAsync(
        EventStreamId id,
        CancellationToken cancellationToken)
    {
        var streamId = new SampleEventStreamId(id);
        view = await reader.FindAsync(
                   streamId.Id,
                   streamId.Id,
                   cancellationToken) ??
               new SampleReadModel
               {
                   Id = streamId.Id
               };
    }

    public Task CompleteAsync(
        CancellationToken cancellationToken) =>
        deleted
            ? writer.TryDeleteAsync(view!.Id, view!.PartitionKey, cancellationToken)
            : writer.WriteAsync(view, cancellationToken);

    public void Consume(AddedEvent evt, EventMetadata metadata)
    {
        view.Name = evt.Name;
        view.Address = evt.Address;
    }

    public void Consume(NameChangedEvent evt, EventMetadata metadata)
    {
        view.Name = evt.NewName;
    }

    public void Consume(AddressChangedEvent evt, EventMetadata metadata)
    {
        view.Address = evt.NewAddress;
    }

    public void Consume(DeletedEvent evt, EventMetadata metadata)
    {
        deleted = true;
    }
}
```

There is no requirement that the read-model should be persisted in the same database that contains the events. Actually, the projection doesn't even need to produce read-models. A projection can also be used as a work flow engine that performs certain operations based on the events that have occurred, a projection should not has no means of persisting new events directly, but there is nothing preventing a projection from executing commands, which in turn, record events to the event stream.

To use projections, we need to configure the projection job from `AddEventStore(builder => builder.UseCQRS())`. This needs to be done for every projection that is used in the system

```csharp
services.AddEventStore(
    builder => builder.UseCQRS(
        cqrs => cqrs.AddProjectionJob<SampleProjection>(nameof(SampleProjection)))
```

For a system that has been running for some time, it's probably not a good idea to introduce a new projection as by default, this will always start processing events by looking at all the events in the event-store container. If you have events in the millions, billions, or trillions, or more, then `Atc.Cosmos.EventStore.Cqrs` will be unneccessarily consuming request units on the Cosmos DB account. A projection can be configured start from a specified date. This makes sense if a new feature was introduced at a later time hence no events related to the new projection will exist before the features release date.

To configure a projection to start at a specified date, use the `WithProjectionStartsFrom()` method and specify the start date using `SubscriptionStartOptions.FromDateTime()`

```csharp
services.AddEventStore(
    builder => builder.UseCQRS(
        cqrs => cqrs.AddProjectionJob<SampleProjection>(
            nameof(SampleProjection),
            projection => projection.WithProjectionStartsFrom(
                SubscriptionStartOptions.FromDateTime(
                    new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))));))
```

If you're interested in the full source code then you can grab it [here](/assets/samples/AtcCosmosEventStoreCqrs.zip).