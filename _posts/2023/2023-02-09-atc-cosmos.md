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

For the past 6 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) as my go-to data store. Document databases make so much more sense for the things that I have been building over the past 6 years. The library [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) is the result of years of collective experience solving problems using the same patterns. Atc.Cosmos is a library for configuring containers in Azure Cosmos DB and provides easy, efficient, and convenient ways to read and write document resources.

## Using Atc.Cosmos

Here's an example usage of Atc.Cosmos in a Minimal API project targeting .NET 7.0

```cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureCosmosDb();

var app = builder.Build();
app.MapGet(
    "/foo",
    (
        ICosmosReader<FooResource> reader,
        CancellationToken cancellationToken) =>
            reader
                .ReadAllAsync(FooResource.PartitionKey, cancellationToken)
                .ToBlockingEnumerable(cancellationToken)
                .Select(c => c.Bar))
    .WithName("ListFoo")
    .WithOpenApi();
app.MapGet(
    "/foo/{id}",
    async (
        ICosmosReader<FooResource> reader,
        string id,
        CancellationToken cancellationToken) =>
        {
            var foo = await reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);
            return foo is not null ? Results.Ok(foo.Bar) : Results.NotFound(id);
        })
    .WithName("GetFoo")
    .WithOpenApi();
app.MapPost(
    "/foo",
    async (
        ICosmosWriter<FooResource> writer,
        [FromBody] Dictionary<string, object> data,
        CancellationToken cancellationToken) =>
        {
            var id = Guid.NewGuid().ToString();
            await writer.CreateAsync(
                new FooResource
                {
                    Id = id,
                    Bar = data,
                },
                cancellationToken);
            return Results.CreatedAtRoute("GetFoo", new { id });
        })
    .WithName("PostFoo")
    .WithOpenApi();

app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
app.Run();
```

Let's break that down a bit and start with the `IServiceCollection` extension method `ConfigureCosmosDb()`. 

To use Atc.Cosmos you need to do the following:
- Implement `IConfigureOptions<CosmosOptions>` to configure the database itself
- Define Cosmos resource document types by deriving from `CosmosResource` or implementing `ICosmosResource`
- Implement `ICosmosContainerInitialize` to define a CosmosDb container for every Cosmos resource document type
- 

```cs
public static class ServiceCollectionExtensions
{
    public static void ConfigureCosmosDb(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureCosmosOptions>();
        services.ConfigureCosmos(
            cosmosBuilder =>
            {
                cosmosBuilder.AddContainer<FooContainerInitializer, FooResource>("foo");
                cosmosBuilder.UseHostedService();
            });
    }
}
```

Here's an example implementation of `IConfigureOptions<CosmosOptions>`

```cs
public class ConfigureCosmosOptions : IConfigureOptions<CosmosOptions>
{
    public void Configure(CosmosOptions options)
    {
        options.UseCosmosEmulator();
        options.DatabaseName = "SampleApi";
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}
```

Here's an example implementation of the `ICosmosContainerInitializer` interface for creating a container called `foo`:

```cs
public class FooContainerInitializer : ICosmosContainerInitializer
{
    public Task InitializeAsync(
        Database database,
        CancellationToken cancellationToken) =>
        database.CreateContainerIfNotExistsAsync(
            new ContainerProperties
            {
                PartitionKeyPath = "/pk",
                Id = "foo",
            },
            cancellationToken: cancellationToken);
}
```

Here's an example Cosmos resource document type called `FooResource` that derives from CosmosResource

```cs
public class FooResource : CosmosResource
{
    public const string PartitionKey = "foo";
    public string Id { get; set; } = null!;
    public string Pk => PartitionKey;
    public Dictionary<string, object> Bar { get; set; } = new Dictionary<string, object>();
    protected override string GetDocumentId() => Id;
    protected override string GetPartitionKey() => Pk;
}
```

## ICosmosReader< T >

Cosmos DB is very good at point-read operations, and this is cheap to do. The `ICosmosReader<T>` interface provides the following methods for point read operations:

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

`ReadAsync()` does a point read look-up on the document within the specified partition and throws a `CosmosException` with the Status code NotFound if the resource could not be found. `FindAsync()` on the other hand will return a `null` instance of `T` if the resource count not be found

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

When working with large partitions, you will most likely want to use paging to read out data so that you can return a response to the consumer of your system as fast as possible. `ICosmosReader<T>` provides the following methods for paged queries:

```cs
Task<PagedResult<T>> PagedQueryAsync(
    QueryDefinition query,
    string partitionKey, 
    int? pageSize,
    string? continuationToken = default,
    CancellationToken cancellationToken = default);
```

When working with very large partitions, you might want to parallelize the processing of the documents you read from Cosmos DB, and this can be done by streaming a collection of documents instead of individual ones. `ICosmosReader<T>` provides the following methods for batch queries

```cs
IAsyncEnumerable<IEnumerable<T>> BatchReadAllAsync(
    string partitionKey,
    CancellationToken cancellationToken = default);

IAsyncEnumerable<IEnumerable<T>> BatchQueryAsync(
    QueryDefinition query,
    string partitionKey,
    CancellationToken cancellationToken = default);
```

Cross-partition queries are normally very inefficient, expensive, and slow. Regardless of these facts, there will be times when you will still need them. `ICosmosReader<T>` provides the following methods for performing cross-partition read operations. `ICosmosReader<T>` provides methods for executing a query, a paged query, or a batch query across multiple partitions

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

## ICosmosWriter < T >

There are multiple ways to write to Cosmos DB and my preferred way is to do upserts. This is to create when not exist, otherwise, update. `ICosmosWriter<T>` provides methods for simple upsert operations and methods that includes retry attempts.

```cs
Task<T> WriteAsync(
    T document,
    CancellationToken cancellationToken = default);

Task<T> UpdateOrCreateAsync(
    Func<T> getDefaultDocument,
    Action<T> updateDocument,
    int retries = 0,
    CancellationToken cancellationToken = default);
```

Deleting a resource will usually involve knowing what resource to delete. `ICosmosWriter<T>` provides methods for deleting a resource that MUST exists and another method that returns `true` if the resource was successfully deleted, otherwise `false`

```cs
Task DeleteAsync(
    string documentId,
    string partitionKey,
    CancellationToken cancellationToken = default);

Task<bool> TryDeleteAsync(
    string documentId,
    string partitionKey,
    CancellationToken cancellationToken = default);
```

## Unit Testing

The `ICosmosReader<T>` and `ICosmosWriter<T>` interfaces can easily be mocked, but there might be cases where you would want to fake it instead. For this purpose, you can use the `FakeCosmosReader<T>` or `FakeCosmosWriter<T>` classes from the `Atc.Cosmos.Testing` namespace contains the following fakes. For convenience, Atc.Cosmos.Testing provides the `FakeCosmos<T>` class which fakes both the reader and writer

Based on the example in the beginning of this post, let's say we have a component called `FooService` which can do CRUD operations over the `FooResource`

```cs
public class FooService
{
    private readonly ICosmosReader<FooResource> reader;
    private readonly ICosmosWriter<FooResource> writer;

    public FooService(
        ICosmosReader<FooResource> reader,
        ICosmosWriter<FooResource> writer)
    {
        this.reader = reader;
        this.writer = writer;
    }

    public Task<FooResource?> FindAsync(
        string id,
        CancellationToken cancellationToken = default) =>
        reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);

    public Task UpsertAsync(
        string? id = null,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default) =>
        writer.UpdateOrCreateAsync(
            () => new FooResource { Id = id ?? Guid.NewGuid().ToString() },
            resource => resource.Data = data ?? new Dictionary<string, object>(),
            retries: 5,
            cancellationToken);
}
```

Using a combination of `Atc.Cosmos.Testing` and the [Atc.Test](https://github.com/atc-net/atc-test) library, unit tests using the fakes could look like this:

```cs
public class FooServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Get_Existing_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        FooResource resource)
    {
        fakeCosmos.Documents.Add(resource);
        (await sut.FindAsync(resource.Id)).Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Create_New_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        Dictionary<string, object> data)
    {
        var count = fakeCosmos.Documents.Count;
        await sut.UpsertAsync(data: data);
        fakeCosmos.Documents.Should().HaveCount(count + 1);
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Update_Existing_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        FooResource resource,
        Dictionary<string, object> data)
    {
        fakeCosmos.Documents.Add(resource);
        await sut.UpsertAsync(resource.Id, data);

        fakeCosmos
            .Documents
            .First(c => c.Id == resource.Id)
            .Data
            .Should()
            .BeEquivalentTo(data);
    }
}
```

If you're interested in the full source code then you can grab it [here](/assets/samples/AtcCosmosMinimalApi.zip).