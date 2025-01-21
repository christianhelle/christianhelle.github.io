---
layout: post
title: Azure Entra ID Authentication with Scalar and .NET 9.0
date: 2025-01-20
author: Christian Helle
tags:
- Scalar
- OpenAPI
- .NET 9.0
redirect_from:
- /2025/01/scalar-azure-authentication/
- /2025/01/scalar-azure-authentication
- /2025/scalar-azure-authentication
- /2025/scalar-azure-authentication
- /scalar-azure-authentication/
- /scalar-azure-authentication
---

[Swagger UI](https://swagger.io/tools/swagger-ui/) and [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) are two popular libraries for working with OpenAPI for ASP.NET Core Web APIs.
Both tools have come with benefits and problems,
but that said, most .NET developers have gotten used to it.

In May 2024, Microsoft [announced that Swashbuckle.AspNetCore will be removed from .NET 9.0](https://github.com/dotnet/aspnetcore/issues/54599). For the past year or so, I started using .http files in favor of Swagger UI. Using .http files immediately lead me to develop [HTTP File Generator](https://github.com/christianhelle/httpgenerator), a tool that can [generate a suite of .http files from OpenAPI specifications](/2023/11/http-file-generator.html)

The use of .http files were not immediately adopted by my teams so I started looking at other alternatives, particularly, [Scalar](https://scalar.com). Other teams I work with have built a work flow based on sharing [Postman Collections](https://www.postman.com/collection/) configured with Authentication and multiple environments, like Dev, Test, and Production.
[Scalar](https://scalar.com) feels a lot like [Postman](https://www.postman.com) which caught the interest of teams around me

You can try out Scalar [here](https://docs.scalar.com/swagger-editor) and it looks like this:

![Scalar demo](/assets/images/scalar.png)

In any project I'm involved in, one of the first thing I do, even before setting up a CI/CD pipeline, is to configure security.
This usually uses [OAuth2 with the implicit (or authorization code) flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-implicit-grant-flow?WT.mc_id=DT-MVP-5004822) and Azure Entra ID as a Secure Token Service (STS) for authentication.

This post will show you how to setup a .NET 9.0 project that produces an OpenAPI docment
and will demonstrate how to use [Scalar](https://scalar.com) instead of Swagger UI.
We will configure Scalar to authenticate against Azure Entra ID.

Let's start by creating a simple API with .NET 9.0 (AOT)

By default, the `.csproj` file looks something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <InvariantGlobalization>true</InvariantGlobalization>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

</Project>
```

and the `Program.cs`

```csharp
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(
            0,
            AppJsonSerializerContext.Default);
    })

var app = builder.Build();
var todosApi = app.MapGroup("/todos");

var sampleTodos = new Todo[]
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet(
    "/{id}",
    (int id) =>
        sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.Run();

public record Todo(
    int Id,
    string? Title,
    DateOnly? DueBy = null,
    bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

Next, we need to install a couple of NuGet packages
- [Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer) - as of writing v9.0.1
- [Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi) - as of writing v9.0.1
- [Scalar.AspNetCore](https://www.nuget.org/packages/Scalar.AspNetCore) - as of writing v1.2.*

The `.csproj` file should now look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <InvariantGlobalization>true</InvariantGlobalization>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.1" />
    <PackageReference Include="Scalar.AspNetCore" Version="1.2.*" />
  </ItemGroup>

</Project>
```

Next, we need to configure the API to expose OpenAPI specifications using the Microsoft OpenAPI toolset. We need to register the Microsoft OpenAPI dependencies using the `AddOpenApi()` extension method to `IServiceCollection` and configure the middleware using the `UseOpenApi()` on the `WebApplication`

```csharp
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(
            0,
            AppJsonSerializerContext.Default);
    })
    .AddOpenApi();

...

var app = builder.Build();
app.UseOpenApi();

...
```

Next, we configure Scalar. This is done registering setting up the Scalar middleware calling `MapScalarApiReference()` on the `WebApplication`. You will need to import the `Scalar.AspNetCore` namespace

```csharp
using Scalar.AspNetCore;

...

var app = builder.Build();
app.UseOpenApi();
app.MapScalarApiReference();

...
```

With this, we should be able to see the Scalar page on `/scalar/v1`. It looks something like this:

![scalar default](/assets/images/scalar-default.png)

![scalar get todos](/assets/images/scalar-get-todos-preview.png)

Clicking on the **Test Request** button from the `GET /todos` section will allow you to perform tests against the endpoint. With the current setup, it would look something like this:

![scalar get todos result](/assets/images/scalar-get-todos-results.png)
