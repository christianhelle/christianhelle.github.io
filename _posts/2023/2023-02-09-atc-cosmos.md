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

For the past 6 years, I have been using [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/introduction?WT.mc_id=DT-MVP-5004822) (formerly known as [Document DB](https://azure.microsoft.com/en-us/blog/dear-documentdb-customers-welcome-to-azure-cosmos-db?WT.mc_id=DT-MVP-5004822)) as my go-to data store. Document databases make so much more sense for the things that I have been building over the past 6 years. The library [Atc.Cosmos](https://github.com/atc-net/atc-cosmos) is the result of years of collective experience solving problems using the same patterns. Atc.Cosmos is a library for configuring containers in Azure Cosmos DB and provides easy, efficient, and convenient ways to read and write document resources.

Here's an example usage of Atc.Cosmos in a Minimal API project targetting .NET 7.0

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