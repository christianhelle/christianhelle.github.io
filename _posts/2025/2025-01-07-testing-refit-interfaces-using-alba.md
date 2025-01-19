---
layout: post
title: Testing Refit interfaces using Alba
date: 2025-01-07
author: Christian Helle
tags:
- Refit
- OpenAPI
redirect_from:
- /2025/01/testing-refit-interfaces-with-alba
- /2025/01/testing-refit-interfaces-with-alba
- /2025/testing-refit-interfaces-with-alba/
- /2025/testing-refit-interfaces-with-alba
- /testing-refit-interfaces-with-alba/
- /testing-refit-interfaces-with-alba
---

I use [Refit](https://github.com/reactiveui/refit) to consume Web APIs.
The Refit interfaces that I work with are usually code generated using [Refitter](https://github.com/christianhelle/refitter).
Every now and then I stumble upon a bug regarding serialization of the payload.
I've found it interesting to test these interfaces,
and I've been looking for a way to do this in a more structured way.

For a while now, I've been using [Alba](https://jasperfx.github.io/alba/) to test my ASP.NET Core Web APIs.
Alba allows me to skip the heavy details of setting up the test server and client,
and instead focus on the actual test.

Let's say that I have an API that returns a list of `Todo` items.
This would be defined a solution with 3 projects: API, Client, Contracts.
The solution will also contain a unit test project.

The API code is defined as follows:

```csharp
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(
            0,
            AppJsonSerializerContext.Default);
    });

var app = builder.Build();

var sampleTodos = new Todo[]
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet(
    "/{id}",
    (int id) =>
        sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.Run();

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext
    : JsonSerializerContext
{
}

public partial class Program
{
}
```

The API projects has a reference to the Contracts project.
The Contracts project contains the following class:

```csharp
public record Todo(
    int Id,
    string? Title,
    DateOnly? DueBy = null,
    bool IsComplete = false);
```

The Client project has a reference to the Contracts project and the Refit package.
The Client project contains the following Refit interface:

```csharp
public interface IApiClient
{
    [Get("/todos")]
    Task<Todo[]> GetTodos();

    [Get("/todos/{id}")]
    Task<Todo> GetTodoById(int id);
}
```

The unit test project has a reference to the API project, Contracts project, Client project, and the Alba package.
The unit test project contains the following test:

```csharp
public class ApiClientTests
{
    [Fact]
    public async Task Can_Get_Todos()
    {
        await using var host = await AlbaHost.For<Program>();
        var serverBaseAddress = host.GetTestClient().BaseAddress;

        var services = new ServiceCollection();
        services.AddRefitClient<IApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = serverBaseAddress)
            .ConfigurePrimaryHttpMessageHandler(host.Server.CreateHandler);

        var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<IApiClient>();

        var results = await sut.GetTodos();
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }
}
```

The code above configures [Refit](https://github.com/reactiveui/refit) using the [Refit.HttpClientFactory](https://www.nuget.org/packages/refit.httpclientfactory) library
to use the Test HTTP Client instance provided by the Alba host.
This way, we don't need to run the actual API project before we can test
if the Refit interface works as expected.

I published an example project to [Github](https://github.com/christianhelle/TestingRefitWithAlba) if you want to try it out yourself.
