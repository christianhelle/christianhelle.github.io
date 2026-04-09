---
layout: post
title: Querying Azure Data Explorer from .NET with Cabazure.Kusto
date: 2025-12-18
author: Christian Helle
tags:
  - Azure
  - Kusto
  - .NET
redirect_from:
  - /2025/12/18/azure-kusto-with-cabazure
  - /2025/12/18/azure-kusto-with-cabazure/
  - /2025/12/azure-kusto-with-cabazure
  - /2025/12/azure-kusto-with-cabazure/
  - /2025/azure-kusto-with-cabazure
  - /2025/azure-kusto-with-cabazure/
  - /azure-kusto-with-cabazure
  - /azure-kusto-with-cabazure/
---

Querying Azure Data Explorer (Kusto) from .NET requires managing connections, embedding `.kusto` scripts, parametrizing queries, deserializing results, and handling pagination. It's a lot of boilerplate—connection strings, credential chains, data readers, continuation tokens. Each project ends up with slightly different patterns. I found myself writing the same infrastructure code repeatedly.

That's why I use [Cabazure.Kusto](https://github.com/Cabazure/Cabazure.Kusto)—a .NET library written by my colleague [@rickykaare](https://github.com/rickykaare) that simplifies executing Kusto queries from .NET applications. Define your queries as records, embed `.kusto` scripts alongside your code, and let the framework handle connection management, parameter binding, result deserialization, and pagination. Your API endpoints become three lines of code.

## Why Query Abstraction Matters

When you query Kusto directly, you manage low-level details: construct connection strings, authenticate with `DefaultAzureCredential`, open readers, map columns to objects, handle pagination tokens. This repeats across every endpoint. Each query becomes a `Task` that returns a list or a single object. Pagination requires passing session IDs and continuation tokens, then mapping response headers. It adds noise to your business logic.

Cabazure.Kusto removes this by providing a unified query interface. Define a query record, create a matching `.kusto` file, and inject `IKustoProcessor`. Call `ExecuteAsync` with your query and cancellation token. The framework handles authentication, script loading, parameter binding, result mapping, and pagination. Your controller focuses on business logic, not infrastructure.

## Setting Up Cabazure.Kusto

Register the library in your dependency injection container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCabazureKusto(o =>
{
    o.HostAddress = new Uri("https://help.kusto.windows.net/");
    o.DatabaseName = "ContosoSales";
    o.Credential = new DefaultAzureCredential();
});

var app = builder.Build();
```

The options configure your Kusto cluster URL, target database, and authentication credential. `DefaultAzureCredential` supports managed identity, user sign-in, or environment variables—whatever your deployment requires.

## Defining Query Records and `.kusto` Files

Queries are defined as C# records that inherit from `KustoQuery<T>`, where `T` is your result type. Each query lives alongside a `.kusto` file with the same name.

Create a query record:

```csharp
namespace SampleApi.Queries;

using Cabazure.Kusto;
using SampleApi.Contracts;

public record CustomersQuery(
    int? CustomerId = null)
    : KustoQuery<Customer>;
```

The record properties become query parameters. Create a matching `.kusto` file in the same namespace directory:

```kusto
declare query_parameters (
    customerId:long = long(null)
);
Customers
| where isnull(customerId) or customerId == CustomerKey
| project
    CustomerKey,
    FirstName,
    LastName,
    CompanyName,
    CityName,
    StateProvinceName,
    RegionCountryName,
    ContinentName,
    Gender,
    MaritalStatus,
    Education,
    Occupation
```

The `.kusto` file declares parameters that match your record properties (note the camelCase convention in the script). The Kusto Query Language (KQL) builds your actual query. The result columns map directly to your result type record.

Define your result type as a record:

```csharp
namespace SampleApi.Contracts;

public record Customer(
    int CustomerKey,
    string FirstName,
    string LastName,
    string? CompanyName,
    string CityName,
    string StateProvinceName,
    string RegionCountryName,
    string ContinentName,
    string Gender,
    string MaritalStatus,
    string Education,
    string Occupation);
```

The property names and types must match the Kusto query's output columns.

## Executing Simple Queries

Once your query and result types are defined, executing is straightforward. Inject `IKustoProcessor` and call `ExecuteAsync`:

```csharp
app.MapGet(
    "/customers/{customerId}",
    async static (
        int customerId,
        IKustoProcessor processor,
        CancellationToken cancellationToken)
        => await processor.ExecuteAsync(
            new CustomersQuery(customerId),
            cancellationToken) switch
        {
            [{ } customer] => Results.Ok(customer),
            _ => Results.NotFound(),
        })
    .WithName("GetCustomer");
```

The `ExecuteAsync` method returns an array of your result type. Use pattern matching or LINQ to extract the single result or handle the collection as needed. Behind the scenes, the framework loads the `.kusto` file, binds your record properties as parameters, connects to Kusto, executes the query, and deserializes rows into `Customer` objects.

## Queries That Return Collections

For endpoints that return multiple results, use the same pattern:

```csharp
app.MapGet(
    "/customers",
    async static (
        IKustoProcessor processor,
        CancellationToken cancellationToken)
        => await processor.ExecuteAsync(
            new CustomersQuery(),
            cancellationToken))
    .WithName("ListCustomers");
```

When `ExecuteAsync` returns without specifying pagination, it returns the full result array. Kusto imposes limits on result sizes, so for large datasets, implement pagination.

## Pagination and Continuation Tokens

For endpoints serving paginated results, use the three-parameter overload of `ExecuteAsync`. This supports session-based continuation:

```csharp
app.MapGet(
    "/customers",
    async static (
        [FromHeader(Name = "x-client-session-id")] string? sessionId,
        [FromHeader(Name = "x-max-item-count")] int? maxItemCount,
        [FromHeader(Name = "x-continuation-token")] string? continuationToken,
        IKustoProcessor processor,
        CancellationToken cancellationToken)
        => await processor.ExecuteAsync(
            new CustomersQuery(),
            sessionId,
            maxItemCount ?? 100,
            continuationToken,
            cancellationToken))
    .WithName("ListCustomers");
```

The processor returns a `PagedResult<T>`:

```csharp
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken);
```

Your response includes `Items` (the current page) and a `ContinuationToken` (for fetching the next page). The client passes the token in subsequent requests to continue pagination. The `sessionId` maintains state across multiple requests, and `maxItemCount` controls page size.

## Complex Queries and Aggregations

Kusto excels at analytical queries. Create a query for customer sales aggregation:

```csharp
namespace SampleApi.Queries;

using Cabazure.Kusto;
using SampleApi.Contracts;

public record CustomerSalesQuery
    : KustoQuery<CustomerSales>
{
}
```

With the matching `.kusto` file:

```kusto
Customers
| join kind=inner SalesFact on CustomerKey
| extend CustomerName = strcat(FirstName, ' ', LastName)
| summarize 
    SalesAmount = todecimal(round(sum(SalesAmount), 2)),
    TotalCost = todecimal(round(sum(TotalCost), 2))
  by CustomerKey, CustomerName
| take 100
```

And the result type:

```csharp
namespace SampleApi.Contracts;

public record CustomerSales(
    int CustomerKey,
    string CustomerName,
    decimal SalesAmount,
    decimal TotalCost);
```

Execute it the same way:

```csharp
app.MapGet("/customer-sales", 
    (IKustoProcessor processor, CancellationToken cancellationToken)
        => processor.ExecuteAsync(
            new CustomerSalesQuery(), 
            cancellationToken))
    .WithName("GetCustomerSales");
```

Kusto's analytical operators (join, summarize, extend, take, top, etc.) let you build aggregations and pivots without multiple round-trips. The query runs server-side; only results are deserialized.

## The Sample Application

The [Cabazure.Kusto repository](https://github.com/Cabazure/Cabazure.Kusto) includes a `samples/SampleApi` project that demonstrates these patterns using the public Azure Data Explorer cluster with the `ContosoSales` database. It shows real endpoints: listing customers, fetching a single customer, and querying customer sales data.

To run the sample:

1. Clone the repository
2. Navigate to `samples/SampleApi`
3. Run `dotnet run`
4. Visit `http://localhost:5000/swagger` to explore endpoints

The sample uses `DefaultAzureCredential`, which automatically discovers credentials in your environment. For local testing, ensure you have the Azure CLI authenticated.

## When to Use Cabazure.Kusto

Cabazure.Kusto shines when:

- **You query Kusto frequently** from a .NET application. Boilerplate reduction pays off immediately.
- **You have many query types** and want consistency. All queries follow the same record + `.kusto` file pattern.
- **You need pagination**. The built-in continuation token handling is cleaner than managing it manually.
- **You want strong typing**. Your queries are C# records with compile-time type safety; result deserialization is automatic and type-checked.
- **You're building an API** with multiple Kusto endpoints. Minimal setup per endpoint means faster development.

It's less useful if you have a single, massive query or if you're building an interactive query tool. For those scenarios, use the Kusto SDK directly.

## Under the Hood

The library provides a simple but powerful abstraction:

```csharp
public interface IKustoProcessor
{
    Task ExecuteAsync(
        IKustoCommand command,
        CancellationToken cancellationToken);

    Task<T?> ExecuteAsync<T>(
        IKustoQuery<T> query,
        CancellationToken cancellationToken);

    Task<PagedResult<T>?> ExecuteAsync<T>(
        IKustoQuery<IReadOnlyList<T>> query,
        string? sessionId,
        int? maxItemCount,
        string? continuationToken,
        CancellationToken cancellationToken);
}
```

When you call `ExecuteAsync`, the processor:

1. Locates the `.kusto` file matching your query record's namespace and name
2. Binds your record properties to Kusto query parameters
3. Authenticates using the configured credential
4. Executes the query against your Kusto cluster
5. Deserializes results into your result type
6. Returns the populated objects (or a paged result with a continuation token)

All of this happens transparently. Your code is clean and focused.

## Getting Started

To use Cabazure.Kusto in your project:

1. Install the NuGet package: `dotnet add package Cabazure.Kusto`
2. Register in Program.cs with your cluster URL, database, and credential
3. Create a query record for each query type
4. Add a matching `.kusto` file alongside your query record
5. Define a result type record matching your Kusto columns
6. Inject `IKustoProcessor` and call `ExecuteAsync`

The README in the [Cabazure.Kusto repository](https://github.com/Cabazure/Cabazure.Kusto) provides full documentation and additional examples.

## Conclusion

Building analytics APIs shouldn't require managing low-level Kusto infrastructure. Cabazure.Kusto removes the boilerplate so you can focus on query logic and API contracts. If you query Azure Data Explorer from .NET, give it a try. The source code is on GitHub at [https://github.com/Cabazure/Cabazure.Kusto](https://github.com/Cabazure/Cabazure.Kusto).
