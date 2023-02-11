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

## Getting Started with Atc.Cosmos

The library is installed by adding the [Atc.Cosmos NuGet package](https://www.nuget.org/packages/Atc.Cosmos) to your project. Once the library is added to your project, you will have access to the following interfaces, used for reading and writing Cosmos document resources:

- `ICosmosReader<T>`
- `ICosmosWriter<T>`
- `ICosmosBulkReader<T>`
- `ICosmosBulkWriter<T>`

Where `T` is a document resource represented by a class derived from the `CosmosResource` base-class, or by implementing the underlying `ICosmosResource` interface directly.

### ICosmosReader<T>

Cosmos DB is really good at point read operations, and this is really cheap to do. The `ICosmosReader<T>` interface provides the following methods for point read operations:

```cs
Task<T> ReadAsync(
    string documentId, 
    string partitionKey, 
    CancellationToken cancellationToken = default);

Task<T?> FindAsync(
    string documentId, 
    string partitionKey, 
    CancellationToken cancellationToken = default);
```

`ReadAsync()` does a point read look up on the document within the specified partition and throws a `CosmosException` with the Status code NotFound if the resource could not be found. `FindAsync()` on the other hand will return a `null` instance of `T` if the resource count not be found

You will notice that the majority of methods exposed in `ICosmosReader<T>` require the partition key to be specified. this is because read operations on Azure Cosmos DB are very cheap and efficient as long as you stay within a single partition.

`ICosmosReader<T>` provides methods for reading multiple documents out. This can be done by reading all the documents within a partition or running a query against the partition. Here are some methods that do exactly that:

```cs
IAsyncEnumerable<T> ReadAllAsync(
    string partitionKey, 
    CancellationToken cancellationToken = default);

IAsyncEnumerable<T> QueryAsync(
    QueryDefinition query, 
    string partitionKey, 
    CancellationToken cancellationToken = default);
```

As the name states, `ReadAllAsync()` reads **all documents** from the specified partition and returns an asynchronous stream of individual documents. `QueryAsync()` executes a `QueryDefinition` against the specified partition.

When working with large partitions, you will most likely want to using paging to read out data so that you can return a response to the consumer of your system as fast as possible. `ICosmosReader<T>` provides the following methods for paged queries:

```cs
Task<PagedResult<T>> PagedQueryAsync(
    QueryDefinition query,
    string partitionKey, 
    int? pageSize,
    string? continuationToken = default,
    CancellationToken cancellationToken = default);
```

When working with very large partitions, you might want to parallelize processing of the documents you read from Cosmos DB, and this can be done by streaming a collection of documents instead of individual ones. `ICosmosReader<T>` provides the following methods for batch queries

```cs
IAsyncEnumerable<IEnumerable<T>> BatchReadAllAsync(
    string partitionKey,
    CancellationToken cancellationToken = default);

IAsyncEnumerable<IEnumerable<T>> BatchQueryAsync(
    QueryDefinition query,
    string partitionKey,
    CancellationToken cancellationToken = default);
```

Cross partition queries are normally very ineffecient, expensive, and slow. Regardless of these facts, there will be times where you will still need them. `ICosmosReader<T>` provides the following methods for performing cross partition read operations. `ICosmosReader<T>` provides methods for executing a query, a paged query, or a batch query across multiple partitions

```cs
IAsyncEnumerable<T> CrossPartitionQueryAsync(
    QueryDefinition query,
    CancellationToken cancellationToken = default);

Task<PagedResult<T>> CrossPartitionPagedQueryAsync(
    QueryDefinition query,
    int? pageSize,
    string? continuationToken = default,
    CancellationToken cancellationToken = default);

IAsyncEnumerable<IEnumerable<T>> BatchCrossPartitionQueryAsync(
    QueryDefinition query,
    CancellationToken cancellationToken = default);
```

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

```cs
services.Configure<CosmosOptions>(
    Configuration.GetSection(configurationSectionName));
```

Or by using a factory class implementing the `IConfigureOptions<CosmosOptions>` interface and register it like this:

```cs
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

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyResource>(containerName));
    }
    ```

3. If you want to connect to multiple databases you would need to scope your container to a new `CosmosOptions` instance in the following way:

    ```cs
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

## Initialize containers

The library supports adding initializers for each container, that can then be used to create the container, and configure it with the correct keys and indexes.

To do this you will need to:

1. Create an initializer by implementing the `ICosmosContainerInitializer` interface.

    Usually the implementation will call the `CreateContainerIfNotExistsAsync()` method on
    the provided `Database` object with the desired `ContainerProperties`.

2. Setup the initializer to be run during initialization

    This is done on the `ICosmosBuilder` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.AddContainer<MyInitializer>(containerName));
    }
    ```

3. Chose a way to run the initialization

    For an AspNet Core services, a HostedService can be used, like this:
    
    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b.UseHostedService()));
    }
    ```

    For Azure Functions, the `AzureFunctionInitializeCosmosDatabase()` extension method
    can be used to execute the initialization (synchronously) like this:
    
    ```cs
    public void Configure(IWebJobsBuilder builder)
    {
        ConfigureServices(builder.Services);
        builder.Services.AzureFunctionInitializeCosmosDatabase();
    }
    ```


## Using the readers and writers

Once the setup is in place, the readers and writers are registered with the [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) container, and can be obtained via constructor injection on any service.

The registered interfaces are:

|Name|Description|
|-|-|
|`ICosmosReader<T>`| Represents a reader that can read Cosmos resources. |
|`ICosmosWriter<T>`| Represents a writer that can write Cosmos resources. |
|`ICosmosBulkReader<T>`| Represents a reader that can perform bulk reads on Cosmos resources. |
|`ICosmosBulkWriter<T>`| Represents a writer that can perform bulk operations on Cosmos resources. |

The bulk reader and writer are for optimizing performance when executing many operations towards Cosmos. It works by creating all the tasks and then use the `Task.WhenAll()` to await them. This will group operations by partition key and send them in batches of 100.

When not operating with bulks, the normal readers are faster as there is no delay waiting for more work.


## Change Feeds

The library supports adding change feed processors for a container.

To do this you will need to:

1. Create a processor by implementing the `IChangeFeedProcessor` interface.

2. Setup the change feed processor during initialization

    This is done on the `ICosmosBuilder<T>` made available using the `ConfigureCosmos()` extension on the `IServiceCollection`, like this:

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer, MyResource>(containerName)
        .WithChangeFeedProcessor<MyChangeFeedProcessor>());
    }
    ```

    or using the `ICosmosContainerBuilder<T>` like this:

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
      services.ConfigureCosmos(b => b
        .AddContainer<MyInitializer>(
          containerName,
          c => c
            .AddResource<MyResource>()
            .WithChangeFeedProcessor<MyChangeFeedProcessor>()));
    }
    ```

***Note**: The change feed processor relies on a `HostedService`, which means that this feature is only available in ASP.NET Core services.*


## Unit Testing

The reader and writer interfaces can easily be mocked, but in some cases it is nice to have a fake version of a reader or writer to mimic the behavior of the read and write operations. For this purpose the `Atc.Cosmos.Testing` namespace contains the following fakes:

|Name|Description|
|-|-|
|`FakeCosmosReader<T>`| Used for faking an `ICosmosReader<T>` or `ICosmosBulkReader<T>` |
|`FakeCosmosWriter<T>`| Used for faking an `ICosmosWriter<T>` or `ICosmosBulkWriter<T>` |
|`FakeCosmos<T>`| Used for getting a `FakeCosmosReader` and `FakeCosmosWriter` that share state. |

Using the [Atc.Test](https://github.com/atc-net/atc-test) setup a test using the fakes could look like this:

```cs
[Theory, AutoNSubstituteData]
public async Task Should_Update_Cosmos_With_NewData(
    [Frozen(Matching.ImplementedInterfaces)]
    FakeCosmos<MyCosmosResource> cosmos,
    MyCosmosService sut,
    MyCosmosResource resource,
    string newData)
{
    cosmos.Documents.Add(resource);

    await service.UpdateAsync(resource.Id, newData);

    resource
        .Data
        .Should()
        .Be(newData);
}
```