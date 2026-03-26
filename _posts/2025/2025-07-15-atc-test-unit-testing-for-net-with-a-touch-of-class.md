---
layout: post
title: Atc.Test - Unit testing for .NET with A Touch of Class
date: '2025-07-15'
author: Christian Helle
tags:
  - .NET
  - Testing
  - xUnit
  - AutoFixture
  - NSubstitute
  - FluentAssertions
redirect_from:
  - /2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class/
  - /2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class
  - /2025/07/15/atc-test-unit-testing-for-net-with-a-touch-of-class/
  - /2025/07/15/atc-test-unit-testing-for-net-with-a-touch-of-class
---

I have been writing unit tests in .NET for years, and my go-to stack has always been some variation of [xUnit](https://xunit.net/), [FluentAssertions](https://fluentassertions.com/), [NSubstitute](https://nsubstitute.github.io/), and [AutoFixture](https://github.com/AutoFixture/AutoFixture). These are excellent tools individually, but using them together often means writing the same boilerplate over and over: instantiating mocks, wiring up constructor dependencies, and building test objects that do not really say anything about what the test is trying to prove.

[Atc.Test](https://github.com/atc-net/atc-test) is a library that solves this by building on top of that familiar stack and making the tests themselves the focus. Instead of verbose setup code, you write parameters. The library takes care of the rest.

## Why Atc.Test

The idea behind Atc.Test is simple: let the test parameters do the work.

When you use xUnit with AutoFixture and NSubstitute manually, each test tends to look like this:

```csharp
[Fact]
public void Add_ShouldReturnSum()
{
    // Arrange
    var mockExchange = Substitute.For<IExchange>();
    var sut = new PriceConverter(mockExchange);
    var amount = 100m;
    mockExchange.Convert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>())
        .Returns(85m);

    // Act
    var result = sut.Convert(amount);

    // Assert
    result.Should().Be(85m);
}
```

With Atc.Test, the same test becomes:

```csharp
[Theory]
[AutoNSubstituteData]
public void Add_ShouldReturnSum(
    [Frozen] IExchange exchange,
    PriceConverter sut,
    decimal amount)
{
    exchange
        .Convert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>())
        .Returns(85m);

    sut.Convert(amount).Should().Be(85m);
}
```

The boilerplate shrinks. The intent stays clear. And when the constructor of `PriceConverter` changes, you do not have to touch every test that creates it.

## Setup

Add the package to your test project along with xUnit v3:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit.v3" Version="3.0.1" />
    <PackageReference Include="Atc.Test" Version="2.0.17" />
  </ItemGroup>
</Project>
```

Atc.Test does not bundle xUnit v3 transitively. It depends on `xunit.v3.extensibility.core` but leaves the runner decision to your project. That lets you pin your xUnit version independently and avoids warnings from runner assets that do not target `netstandard2.1`.

I also recommend adding global usings to keep the test files clean:

```xml
<ItemGroup>
  <Using Include="Atc.Test" />
  <Using Include="AutoFixture" />
  <Using Include="AutoFixture.Xunit3" />
  <Using Include="FluentAssertions" />
  <Using Include="NSubstitute" />
  <Using Include="Xunit" />
</ItemGroup>
```

## Auto-generated data with `[AutoNSubstituteData]`

The simplest entry point is `[AutoNSubstituteData]`. It tells xUnit to resolve every parameter using AutoFixture, and to automatically substitute interfaces and abstract types with NSubstitute mocks.

```csharp
public sealed class TemperatureConverter
{
    public decimal ToFahrenheit(decimal celsius)
        => celsius * 9 / 5 + 32;
}

public sealed class TemperatureConverterTests
{
    [Theory]
    [AutoNSubstituteData]
    public void ToFahrenheit_converts_correctly(
        decimal celsius,
        TemperatureConverter sut)
        => sut.ToFahrenheit(celsius).Should().Be(celsius * 9 / 5 + 32);
}
```

Both `celsius` and `sut` are generated automatically. If `TemperatureConverter` had a constructor dependency on an interface, that interface would be a mock.

Here is a more realistic example with a service that calls a repository:

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public sealed record Product(Guid Id, string Name, decimal Price);

public sealed class ProductService(IProductRepository repository)
{
    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken);
}

public sealed class ProductServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task GetAsync_returns_product_from_repository(
        [Frozen] IProductRepository repository,
        ProductService sut,
        Guid id,
        Product product,
        CancellationToken cancellationToken)
    {
        repository
            .GetByIdAsync(id, cancellationToken)
            .Returns(product);

        var result = await sut.GetAsync(id, cancellationToken);

        result.Should().Be(product);
        await repository.Received(1).GetByIdAsync(id, cancellationToken);
    }
}
```

The `[Frozen]` attribute on `repository` tells AutoFixture to reuse that same mock instance everywhere it needs an `IProductRepository`. This is what lets us set up the mock, run the SUT, and then verify against the same instance.

## Inline data with `[InlineAutoNSubstituteData]`

When you want specific values for some parameters but still want the fixture to handle the rest, use `[InlineAutoNSubstituteData]`. Each `[InlineAutoNSubstituteData(...)]` attribute adds one theory row where the first arguments come from your inline values and the rest are auto-generated.

```csharp
public sealed class OrderCalculator
{
    public decimal CalculateTotal(decimal subtotal, decimal discountRate)
        => subtotal * (1 - discountRate);
}

public sealed class OrderCalculatorTests
{
    [Theory]
    [InlineAutoNSubstituteData(100m, 0.1m, 90m)]
    [InlineAutoNSubstituteData(200m, 0.25m, 150m)]
    [InlineAutoNSubstituteData(50m, 0m, 50m)]
    public void CalculateTotal_applies_discount_correctly(
        decimal subtotal,
        decimal discountRate,
        decimal expected,
        OrderCalculator sut)
        => sut.CalculateTotal(subtotal, discountRate).Should().Be(expected);
}
```

The inline values fill `subtotal` and `discountRate`, and `sut` is created automatically.

## Member and class data

For shared data sources, Atc.Test provides `[MemberAutoNSubstituteData]` and `[ClassAutoNSubstituteData]`. These work like their xUnit counterparts but augment the data with auto-generated parameters.

```csharp
public sealed class ShippingCalculator
{
    public decimal Calculate(decimal weight, decimal distance)
        => weight * distance * 0.05m;
}

public sealed class ShippingCalculatorTests
{
    public static IEnumerable<object?[]> ShippingScenarios()
    {
        yield return [1m, 100m, 5m];
        yield return [5m, 200m, 50m];
        yield return [10m, 50m, 25m];
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(ShippingScenarios))]
    public void Calculate_returns_correct_shipping_cost(
        decimal weight,
        decimal distance,
        decimal expected,
        ShippingCalculator sut)
        => sut.Calculate(weight, distance).Should().Be(expected);
}
```

The member data provides the first three values. The remaining parameter, `sut`, is generated.

Class-based data works the same way:

```csharp
public sealed class DiscountRates : TheoryData<decimal, decimal, decimal>
{
    public DiscountRates()
    {
        Add(100m, 0.05m, 95m);
        Add(200m, 0.10m, 180m);
        Add(50m, 0.15m, 42.5m);
    }
}

public sealed class DiscountCalculator
{
    public decimal Apply(decimal amount, decimal rate)
        => amount * (1 - rate);
}

public sealed class DiscountCalculatorTests
{
    [Theory]
    [ClassAutoNSubstituteData(typeof(DiscountRates))]
    public void Apply_applies_discount(
        decimal amount,
        decimal rate,
        decimal expected,
        DiscountCalculator sut)
        => sut.Apply(amount, rate).Should().Be(expected);
}
```

## Understanding `[Frozen]`

The `[Frozen]` attribute is central to how Atc.Test works. It tells AutoFixture to freeze a resolved specimen so that any subsequent request for the same type gets the same instance.

This matters most when testing services that depend on shared collaborators. Without `[Frozen]`, each reference to an interface type would get its own mock instance. With `[Frozen]`, you get one mock that is injected everywhere.

```csharp
[Theory]
[AutoNSubstituteData]
public async Task ProcessAsync_saves_and_sends_notification(
    [Frozen] IOrderRepository repository,
    [Frozen] INotificationService notifier,
    OrderService sut,
    Order order,
    CancellationToken cancellationToken)
{
    await sut.ProcessAsync(order, cancellationToken);

    await repository.Received(1).SaveAsync(order, cancellationToken);
    await notifier.Received(1).SendAsync(
        Arg.Any<string>(),
        Arg.Any<string>(),
        cancellationToken);
}
```

Both `repository` and `notifier` are frozen. The same instances are injected into `OrderService`, so the Received assertions work correctly.

### Positional reuse

When using inline or member data, a value supplied at the same position as a `[Frozen]` parameter is automatically frozen and reused:

```csharp
[Theory]
[InlineAutoNSubstituteData(42)]
public void Positional_Frozen_reuses_inline_value(
    [Frozen] int number,
    SomeConsumer consumer)
    => consumer.Value.Should().Be(42);
```

### Exact-type promotion

With `[MemberAutoNSubstituteData]`, Atc.Test adds a promotion feature: if member data supplies a value of type `T`, then a later `[Frozen] T` parameter without its own data slot will reuse that same instance.

```csharp
public interface IEmailGateway
{
    Task SendAsync(string to, string subject, string body);
}

public sealed class NotificationService(IEmailGateway gateway)
{
    public IEmailGateway Gateway => gateway;
}

public static IEnumerable<object?[]> EmailScenarios()
{
    yield return [Substitute.For<IEmailGateway>()];
}

[Theory]
[MemberAutoNSubstituteData(nameof(EmailScenarios))]
public void Exact_type_promotion_reuses_earlier_supplied_instance(
    IEmailGateway supplied,
    [Frozen] IEmailGateway frozen,
    NotificationService sut)
{
    frozen.Should().BeSameAs(supplied);
    sut.Gateway.Should().BeSameAs(supplied);
}
```

This is exact-type matching only. If the supplied value is a class implementing two interfaces, it is not promoted to unrelated interface parameters. This avoids accidental cross-interface bleed.

## Fixture customization with `[AutoRegister]`

When your team establishes conventions for how certain types should be generated, you can encode those once and have them applied automatically everywhere. Atc.Test does this with the `[AutoRegister]` attribute.

Any `ICustomization` or `ISpecimenBuilder` marked with `[AutoRegister]` is picked up by `FixtureFactory.Create()`, which the data attributes use internally.

```csharp
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

[AutoRegister]
public sealed class FixedDateTimeCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var provider = Substitute.For<IDateTimeProvider>();
        provider.UtcNow.Returns(new DateTime(2025, 7, 15, 10, 30, 0, DateTimeKind.Utc));
        fixture.Inject(provider);
    }
}

public sealed class DateTimeTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Fixed_datetime_provider_returns_configured_time(
        IDateTimeProvider provider)
        => provider.UtcNow.Should().Be(
            new DateTime(2025, 7, 15, 10, 30, 0, DateTimeKind.Utc));
}
```

Every test that needs an `IDateTimeProvider` now gets the fixed instance. You write the customization once, and it applies to the entire suite.

Another example is generating stable Guids:

```csharp
[AutoRegister]
public sealed class SequentialGuidCustomization : ICustomization
{
    private int counter;

    public void Customize(IFixture fixture)
        => fixture.Register(() => Guid.Parse($"00000000-0000-0000-0000-{++counter:D12}"));
}
```

## Using `FixtureFactory` directly

The data attributes cover most cases, but sometimes you need a bit more control. `FixtureFactory.Create()` gives you an `IFixture` with all your `[AutoRegister]` customizations already applied.

```csharp
public sealed class ManualFixtureTests
{
    [Fact]
    public async Task Create_order_and_process()
    {
        var fixture = FixtureFactory.Create();
        var repository = fixture.Freeze<IOrderRepository>();
        var sut = fixture.Create<OrderService>();
        var order = fixture.Create<Order>();

        await sut.ProcessAsync(order, CancellationToken.None);

        await repository.Received(1).SaveAsync(order, CancellationToken.None);
    }
}
```

This is useful for `[Fact]`-style tests where you want more explicit arrangement code, or when the test setup is complex enough that a hand-written Arrange section reads better.

## Helper extensions

Atc.Test ships with a set of extensions that complement FluentAssertions and NSubstitute.

### JSON and content comparison

String comparisons in tests often fail on whitespace or formatting that is not semantically significant. Atc.Test adds helpers for this:

```csharp
var actual = """{"name":"Atc.Test","version":"1.0"}""";
var expected = """
{
  "name": "Atc.Test",
  "version": "1.0"
}
""";

actual.Should().HaveSimilarJsonAs(expected);
```

Equivalent helpers exist for XML and plain text:

```csharp
actualXml.Should().HaveSimilarXmlAs(expectedXml);
actualText.Should().HaveSimilarContentAs(expectedText);
```

### Equivalency options for date handling

When comparing objects that contain `DateTime` or `DateTimeOffset`, small precision differences cause unnecessary test failures. Atc.Test provides equivalency options to handle this:

```csharp
actual.Should().BeEquivalentTo(
    expected,
    options => options.CompareDateTimeUsingCloseTo(TimeSpan.FromMilliseconds(500)));
```

There is also `CompareJsonElementUsingJson()` for comparing `JsonElement` values by their JSON representation.

### Waiting for substitute calls

Asynchronous tests that involve event-driven code sometimes need to wait for a call to happen before asserting. Atc.Test adds helpers for this:

```csharp
await messageBus.WaitForCall(
    x => x.PublishAsync(
        Arg.Any<InvoiceCreated>(),
        Arg.Any<CancellationToken>()));

var published = messageBus.ReceivedCallWithArgument<InvoiceCreated>();
published.CustomerEmail.Should().Be(customer.Email);
```

There are overloads for waiting on calls with specific arguments, plus helpers for retrieving all arguments of a given type from received calls.

### Task timeouts

For tests where an operation must complete within a time limit:

```csharp
await backgroundOperation.AddTimeout(TimeSpan.FromSeconds(5));
```

If the task does not finish within the timeout, an exception is thrown with a clear message.

### Protected member access

When you need to test a protected method in a class you cannot easily refactor:

```csharp
var result = sut.InvokeProtectedMethod<int>("CalculateChecksum", "INV-001");
result.Should().BeGreaterThan(0);
```

This uses reflection and is not something I reach for often, but it is useful when testing sealed or legacy classes.

## When to use Atc.Test

Atc.Test shines when:

- Services have constructor-based dependencies that evolve over time
- You write many theory-based tests with shared data sources
- You want your tests to express intent rather than setup
- You prefer consistent conventions applied across the whole suite

For very small projects or situations where every detail needs to be explicit, plain xUnit with hand-wired mocks may still be the better choice. Atc.Test is most valuable when the test suite grows and the boilerplate starts to outweigh the signal.

## Wrapping up

Atc.Test does not replace any of the tools it builds on. It makes the combination easier to use. The biggest win is not that it adds new functionality but that it removes the repetitive parts that do not add meaning to your tests.

If you are already using xUnit, AutoFixture, NSubstitute, and FluentAssertions, start with `[AutoNSubstituteData]` and `[Frozen]`. Add inline or member data when you have specific values to cover. Introduce `[AutoRegister]` customizations once you have conventions worth sharing.

The source is on GitHub at [https://github.com/atc-net/atc-test](https://github.com/atc-net/atc-test), and the package is on NuGet at [https://www.nuget.org/packages/Atc.Test/](https://www.nuget.org/packages/Atc.Test/).
