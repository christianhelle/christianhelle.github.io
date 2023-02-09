---
layout: post
title: Atc.Cosmos - Azure Cosmos DB with A Touch of Class
date: '2023-02-09'
author: Christian Helle
tags: 
- CosmosDb
redirect_from:
- /2023/02/atc-cosmos
- /2023/02/atc-cosmos
- /2023/atc-cosmos/
- /2023/atc-cosmos
- /atc-cosmos/
- /atc-cosmos
---

A couple of years ago, me and a group of colleagues and friends, decided that we should open source the ideas, concepts, design patterns, and libraries that we have been carrying around from project to project. From this idea, [ATC.NET](https://github.com/atc-net) was born. We had to come up with a name for this project, which in turn became a GitHub Organization by this time of writing has 40 [members](https://github.com/orgs/atc-net/people), 28 active [repositories](https://github.com/orgs/atc-net/repositories), 1.8 million [total package downloads from NuGet](https://www.nuget.org/profiles/atc-net), and 70k monthly [downloads from PyPi](https://pypistats.org/packages/atc-dataplatform)

For the past 6 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) (formerly known as [Document DB](https://azure.microsoft.com/en-us/blog/dear-documentdb-customers-welcome-to-azure-cosmos-db?WT.mc_id=DT-MVP-5004822)) as my go-to data store. Document databases make so much more sense for the things that I have been building over the past 6 years. The library [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) is the result of years of collective experience solving problems using the same patterns. Atc.Cosmos is a library for configuring containers in Azure Cosmos DB and provides easy, efficient, and convenient ways to read and write document resources.

## Getting Started
The library is installed by adding the NuGet package Atc.Cosmos to your project. Once the library is added to your project, you will have access to the following interfaces, used for reading and writing Cosmos document resources:

- `ICosmosReader<T>`
- `ICosmosWriter<T>`
- `ICosmosBulkReader<T>`
- `ICosmosBulkWriter<T>`

Where `T` is a document resource represented by a class deriving from the `CosmosResource` base-class, or by implementing the underlying `ICosmosResource` interface directly.

## Configure Cosmos connection

To configure where each resource will be stored in Cosmos DB, the `ConfigureCosmos(builder)` extension method is used on the `IServiceCollection` when setting up dependency injection (usually in a Startup.cs file).

For configuring how the library connects to Cosmos, the library uses the `CosmosOptions` class. This includes the following settings:

| Name | Description |
|-|-|
| `AccountEndpoint` | Url to the Cosmos Account. |
| `AccountKey` | Key for Cosmos Account. |
| `DatabaseName` | Name of the Cosmos database (will be provisioned by the library). |
| `DatabaseThroughput` | The throughput provisioned for the database in measurement of Request Units per second in the Azure Cosmos DB service. |
| `SerializerOptions` | The `JsonSerializerOptions` used for the `System.Text.Json.JsonSerializer`. |
| `Credential` | The `TokenCredential` used for accessing [Cosmos DB with an Azure AD token](https://docs.microsoft.com/en-us/azure/cosmos-db/managed-identity-based-authentication?WT.mc_id=DT-MVP-5004822). Please note that setting this property will ignore any value specified in `AccountKey`. |

There are 3 ways to provide the `CosmosOptions` to the library:

1. As an argument to the `ConfigureCosmos()` extension method.
2. As a `Func<IServiceProvider, CosmosOptions>` factory method argument on the `ConfigureCosmos()` extension method.
3. As a `IOptions<CosmosOptions>` instance configured using the Options framework and registered in dependency injection.

This could be done by e.g. reading the `CosmosOptions` from configuration, like this:

```c#
services.Configure<CosmosOptions>(
    Configuration.GetSection(configurationSectionName));
```

Or by using a factory class implementing the `IConfigureOptions<CosmosOptions>` interface and register it like this:

```c#
services.ConfigureOptions<ConfigureCosmosOptions>();
```

The latter is the recommended approach.

## Configure containers

For each Cosmos resource you want to access using the `ICosmosReader<T>` and `ICosmosWriter<T>` you will need to:

1. Create class representing the Cosmos document resource.

    The class should implement the abstract `CosmosResource` base-class, which requires `GetDocumentId()` and `GetPartitionKey()` methods to be implemented.

    The class will be serialized to Cosmos using the `System.Text.Json.JsonSerializer`, so the `System.Text.Json.Serialization.JsonPropertyNameAttribute` can be used to control the actual property name in the json document.

    This can e.g. be useful when referencing the name of the id and partition key properties in a `ICosmosContainerInitializer` implementation which is described further down.

2. Configure the container used for the Cosmos document resource.

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyResource>(containerName));
    }
    ```

3. If you want to connect to multiple databases you would need to scope your container to a new `CosmosOptions` instance in the following way:

    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(
          b => b.AddContainer<MyResource>(containerName)
                .ForDatabase(secondDbOptions)
                  .AddContainer<MySecondResource>(containerName));
    }
    ```
    The first call to AddContainer will be scoped to the default options as the passed builder 'b' is always scoped to the default options.
    The subsequent call to *ForDatabase* will return a new builder scoped for the options passed to this method and any subsequent calls to this builder will have the same scope.
