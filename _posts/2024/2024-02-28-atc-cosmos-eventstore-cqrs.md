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

Let's keep it simple and start off with a simple example .NET 8 Console App that records a few events using commands, and build a real-model using a projection job.

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

The event store library will create the database and the required containers if they do not already exist. When you use the library from non-local environments you will need to ensure that the application using the library has rights to create a cosmos database and containers. If you use managed identity from Microsoft Azure, then I suggest that you provision the database and required containers.

The required containers are the following:

- event-store
- stream-index
- subscriptions
