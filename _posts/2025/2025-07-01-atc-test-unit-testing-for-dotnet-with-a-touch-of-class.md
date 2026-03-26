---
layout: post
title: Atc.Test - Unit testing for .NET with A Touch of Class
date: 2025-07-01
author: Christian Helle
tags:
  - .NET
  - Testing
  - xUnit
redirect_from:
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /atc-test-unit-testing-for-dotnet-with-a-touch-of-class
---

I write a lot of unit tests for .NET systems, and my default stack is usually some combination of [xUnit](https://xunit.net/), [FluentAssertions](https://fluentassertions.com/), [NSubstitute](https://nsubstitute.github.io/), and [AutoFixture](https://github.com/AutoFixture/AutoFixture). It is a great combination, but after a while the same patterns start to repeat themselves. I keep creating the same mocks, wiring the same constructor dependencies, and filling test methods with setup code that does not really explain what I am trying to verify.

[Atc.Test](https://github.com/atc-net/atc-test) is a small library that removes a lot of that ceremony. It builds on top of xUnit v3, AutoFixture, NSubstitute, and FluentAssertions, and gives you a clean way to express tests by focusing on the parameters that matter. The library is available on NuGet as [Atc.Test](https://www.nuget.org/packages/Atc.Test/).

This post is a practical guide to using Atc.Test. I'll start with the basics, then work through the different data attributes, frozen dependencies, fixture customization, and the helper extensions that come with the library.

## What Atc.Test gives you

Atc.Test is not a new test framework. It is a helper library for the stack many of us are already using:

- xUnit v3 for the test framework
- AutoFixture for generating test data
- NSubstitute for creating substitutes
- FluentAssertions for readable assertions

The biggest benefit is that your tests become parameter-driven. Instead of manually building the system under test and its dependencies inside each test method, you describe the few collaborators and values you care about and let Atc.Test fill in the rest.

In practice, that means:

- concrete types are created automatically
- interfaces and abstract types are substituted automatically
- `[Frozen]` lets you reuse the same dependency instance across the whole object graph
- inline, member, and class data can be combined with auto-generated data
- fixture conventions can be registered once and applied everywhere

If your test suite is growing and you want less boilerplate without giving up clarity, this is where Atc.Test really shines.

## Getting started

To use Atc.Test, add it to your test project together with an explicit reference to xUnit v3 and the test SDK.

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

The explicit xUnit reference is important. Atc.Test depends on the xUnit v3 extensibility surface, but intentionally does not pull in the xUnit runner package for you. That keeps the library focused and lets you control your test runner version from the test project itself.

If you are still on xUnit v2, then this library is not a drop-in option. Atc.Test is built for xUnit v3.

I also like adding global usings in the test project so individual test files stay very small:

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

## Your first test with `AutoNSubstituteData`

The easiest way to get started is with the `AutoNSubstituteData` attribute. It tells xUnit to generate the test method parameters using AutoFixture, while using NSubstitute for interfaces and abstract types.

Here is a minimal example:

```csharp
public sealed class Calculator
{
    public int Add(int a, int b) => a + b;
}

public sealed class CalculatorTests
{
    [Theory]
    [AutoNSubstituteData]
    public void AutoNSubstituteData_can_generate_values_and_the_sut(
        int a,
        int b,
        Calculator sut)
        => sut.Add(a, b).Should().Be(a + b);
}
```

The two integers are generated automatically, and the `Calculator` instance is created automatically. There is no manual Arrange step because there is nothing interesting to arrange.

That gets more useful when the type under test has dependencies:

```csharp
public interface ICurrencyExchange
{
    decimal Convert(string fromCurrency, string toCurrency, decimal amount);
}

public sealed class PriceFormatter(ICurrencyExchange exchange)
{
    public decimal ConvertToEur(decimal amount)
        => exchange.Convert("USD", "EUR", amount);
}

public sealed class PriceFormatterTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Interface_parameters_are_created_as_substitutes(
        ICurrencyExchange exchange,
        PriceFormatter sut,
        decimal amount,
        decimal convertedAmount)
    {
        exchange
            .Convert("USD", "EUR", amount)
            .Returns(convertedAmount);

        sut.ConvertToEur(amount).Should().Be(convertedAmount);
    }
}
```

The `ICurrencyExchange` parameter is a substitute. The `PriceFormatter` is a real concrete instance, and its constructor dependency is also resolved automatically.

## Mixing fixed values with generated values

Very often you want a few specific inputs in a theory, but you do not want to hand-craft the entire object graph for each case. That is what `InlineAutoNSubstituteData` is for.

```csharp
public sealed class VatCalculator
{
    public decimal AddVat(decimal amount, decimal vatRate)
        => amount * (1 + vatRate);
}

public sealed class VatCalculatorTests
{
    [Theory]
    [InlineAutoNSubstituteData(100d, 0.25d, 125d)]
    [InlineAutoNSubstituteData(200d, 0.10d, 220d)]
    [InlineAutoNSubstituteData(80d, 0.00d, 80d)]
    public void InlineAutoNSubstituteData_combines_known_cases_with_auto_data(
        double amount,
        double vatRate,
        double expected,
        VatCalculator sut)
        => sut.AddVat((decimal)amount, (decimal)vatRate)
            .Should()
            .Be((decimal)expected);
}
```

The first parameters come from the inline values, and the remaining parameters are generated automatically. This is one of my favorite patterns because it keeps theory tests focused on the meaningful values while still avoiding repetitive setup code.

## Reusing the same dependency with `[Frozen]`

If there is one feature that makes AutoFixture-based tests click, it is `[Frozen]`.

`[Frozen]` tells the fixture to reuse the same instance whenever that exact type is needed later. In practice, this means you can freeze a substitute, let Atc.Test inject it into the system under test, and then assert against that same substitute after exercising the code.

```csharp
public record Invoice(Guid Id, string CustomerEmail, decimal Amount);

public interface IInvoiceRepository
{
    Task SaveAsync(Invoice invoice, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}

public sealed class InvoiceService(
    IInvoiceRepository repository,
    IEmailSender emailSender)
{
    public async Task SendAsync(
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        await repository.SaveAsync(invoice, cancellationToken);
        await emailSender.SendAsync(
            invoice.CustomerEmail,
            "Invoice created",
            $"Invoice {invoice.Id} for {invoice.Amount:C}",
            cancellationToken);
    }
}

public sealed class InvoiceServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task Frozen_dependencies_are_the_same_instances_injected_into_the_sut(
        [Frozen] IInvoiceRepository repository,
        [Frozen] IEmailSender emailSender,
        InvoiceService sut,
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        await sut.SendAsync(invoice, cancellationToken);

        await repository
            .Received(1)
            .SaveAsync(invoice, cancellationToken);

        await emailSender
            .Received(1)
            .SendAsync(
                invoice.CustomerEmail,
                "Invoice created",
                Arg.Any<string>(),
                cancellationToken);
    }
}
```

This is the pattern I use most often:

1. freeze the collaborators I want to configure or verify
2. let Atc.Test create the `sut`
3. execute the method under test
4. assert using the frozen substitutes

I also prefer putting frozen dependencies first in the parameter list, followed by the `sut`, and then the rest of the generated data. It makes the test easier to read.

## Feeding theories from member and class data

Atc.Test also provides `MemberAutoNSubstituteData` and `ClassAutoNSubstituteData`. These are useful when some data should come from a shared source, but you still want the rest of the parameters created automatically.

### `MemberAutoNSubstituteData`

Here is a simple member data example:

```csharp
public sealed class DiscountService
{
    public decimal Apply(decimal amount, decimal percentage)
        => amount - (amount * percentage);
}

public sealed class DiscountServiceTests
{
    public static IEnumerable<object?[]> DiscountCases()
    {
        yield return [100m, 0.10m, 90m];
        yield return [250m, 0.20m, 200m];
        yield return [80m, 0.50m, 40m];
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(DiscountCases))]
    public void Member_data_supplies_the_values_you_care_about(
        decimal amount,
        decimal percentage,
        decimal expected,
        DiscountService sut)
        => sut.Apply(amount, percentage).Should().Be(expected);
}
```

The member data supplies the first three parameters. The remaining parameter, `sut`, is generated automatically.

### `ClassAutoNSubstituteData`

If you prefer class-based theory data, use `ClassAutoNSubstituteData`:

```csharp
public sealed class PercentageCases : TheoryData<decimal, decimal, decimal>
{
    public PercentageCases()
    {
        Add(100m, 0.25m, 125m);
        Add(400m, 0.05m, 420m);
        Add(50m, 0.00m, 50m);
    }
}

public sealed class PercentageCalculator
{
    public decimal AddPercentage(decimal amount, decimal percentage)
        => amount * (1 + percentage);
}

public sealed class PercentageCalculatorTests
{
    [Theory]
    [ClassAutoNSubstituteData(typeof(PercentageCases))]
    public void Class_data_can_be_augmented_with_auto_generated_parameters(
        decimal amount,
        decimal percentage,
        decimal expected,
        PercentageCalculator sut)
        => sut.AddPercentage(amount, percentage).Should().Be(expected);
}
```

This works the same way as the member data variant, except the rows come from a dedicated data class.

### Exact-type promotion for later frozen parameters

One particularly nice detail in `MemberAutoNSubstituteData` is that it can promote an earlier supplied value to a later `[Frozen]` parameter of the same exact type.

```csharp
public sealed class NotificationService(IEmailSender emailSender)
{
    public IEmailSender EmailSender => emailSender;
}

public sealed class NotificationServiceTests
{
    public static IEnumerable<object?[]> SuppliedEmailSender()
    {
        yield return [Substitute.For<IEmailSender>()];
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(SuppliedEmailSender))]
    public void Member_data_can_reuse_an_earlier_value_for_a_later_frozen_parameter(
        IEmailSender supplied,
        [Frozen] IEmailSender frozen,
        NotificationService sut)
    {
        frozen.Should().BeSameAs(supplied);
        sut.EmailSender.Should().BeSameAs(supplied);
    }
}
```

That is a small feature, but it removes duplication in member-data-heavy tests and keeps the intent very clear.

## Customizing fixture behavior once with `[AutoRegister]`

Most teams eventually end up with fixture conventions they want to apply everywhere. Maybe you always want a specific value object generated in a certain way, or maybe there is a common interface that should always return stable test data.

Atc.Test supports this with `[AutoRegister]`. Any `ICustomization` or `ISpecimenBuilder` marked with that attribute is picked up automatically by `FixtureFactory.Create()`, which is also what the data attributes use internally.

```csharp
using Atc.Test.Customizations;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

[AutoRegister]
public sealed class ClockCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2025, 7, 1, 8, 0, 0, TimeSpan.Zero));

        fixture.Inject(clock);
    }
}

public sealed class ClockCustomizationTests
{
    [Theory]
    [AutoNSubstituteData]
    public void AutoRegister_applies_customizations_automatically(
        IClock clock)
        => clock.UtcNow.Should().Be(
            new DateTimeOffset(2025, 7, 1, 8, 0, 0, TimeSpan.Zero));
}
```

This is a great way to create test-wide conventions without repeating setup in every test class.

Behind the scenes, `FixtureFactory.Create()` also configures AutoFixture with recursion handling and AutoNSubstitute integration. That means you get a reasonably sensible default fixture even before adding your own customizations.

## Using `FixtureFactory` directly

Even if you mostly prefer `[Theory]`-based tests, there are times when an explicit fixture is useful. Maybe you need a little more arrangement code, or maybe the test reads better as a normal `[Fact]`.

For those cases, you can use `FixtureFactory.Create()` directly:

```csharp
public sealed class InvoiceServiceFixtureTests
{
    [Fact]
    public async Task FixtureFactory_can_be_used_directly_in_arrange_act_assert_tests()
    {
        var fixture = FixtureFactory.Create();
        var repository = fixture.Freeze<IInvoiceRepository>();
        var emailSender = fixture.Freeze<IEmailSender>();
        var sut = fixture.Create<InvoiceService>();
        var invoice = fixture.Create<Invoice>();

        await sut.SendAsync(invoice, CancellationToken.None);

        await repository
            .Received(1)
            .SaveAsync(invoice, CancellationToken.None);

        await emailSender
            .Received(1)
            .SendAsync(
                invoice.CustomerEmail,
                "Invoice created",
                Arg.Any<string>(),
                CancellationToken.None);
    }
}
```

This gives you the same conventions as the attributes, but with the flexibility of a hand-written Arrange block.

## Helper extensions worth knowing

Atc.Test also includes a collection of helper extensions that are useful even outside the data attributes.

### Comparing `DateTime`, `DateTimeOffset`, and `JsonElement`

If you use FluentAssertions' equivalency API a lot, these helpers are handy:

```csharp
actual.Should().BeEquivalentTo(
    expected,
    options => options
        .CompareDateTimeUsingCloseTo(TimeSpan.FromSeconds(1))
        .CompareJsonElementUsingJson());
```

The `CompareDateTimeUsingCloseTo` extension is especially useful for tests where timestamps differ by a few milliseconds. The `CompareJsonElementUsingJson` extension compares `JsonElement` values by their JSON content instead of by their internal structure.

### Comparing JSON, XML, or formatted text

String comparisons often fail for reasons that are not important to the test, such as whitespace or formatting differences. Atc.Test includes helpers for that too:

```csharp
var actualJson = """{"name":"Atc.Test","kind":"library"}""";
var expectedJson =
    """
    {
      "name": "Atc.Test",
      "kind": "library"
    }
    """;

actualJson.Should().HaveSimilarJsonAs(expectedJson);
```

There are equivalent helpers for XML and for plain text content:

- `HaveSimilarContentAs`
- `HaveSimilarXmlAs`
- `HaveSimilarJsonAs`

### Waiting for substitute calls and inspecting arguments

The `SubstituteExtensions` helpers are useful for asynchronous tests and event-driven code:

```csharp
public record InvoiceCreated(Guid InvoiceId, string CustomerEmail);

public interface IMessageBus
{
    Task PublishAsync(InvoiceCreated message, CancellationToken cancellationToken);
}

await messageBus.WaitForCall(
    x => x.PublishAsync(
        Arg.Any<InvoiceCreated>(),
        Arg.Any<CancellationToken>()));

var publishedMessage = messageBus.ReceivedCallWithArgument<InvoiceCreated>();
publishedMessage.CustomerEmail.Should().Be(invoice.CustomerEmail);
```

There are also overloads for waiting on calls with any arguments, plus helpers for retrieving all arguments of a specific type from received calls.

### Adding a timeout to a task

Sometimes the cleanest assertion in an async test is simply to say that an operation must complete within a reasonable amount of time:

```csharp
await backgroundOperation.AddTimeout(TimeSpan.FromSeconds(2));
```

If the task does not complete before the timeout, the helper throws a `TimeoutException`.

### Invoking protected methods in legacy code

I do not reach for this very often, but it can be practical when testing legacy classes that still hide logic behind protected members:

```csharp
var checksum = sut.InvokeProtectedMethod<int>("CalculateChecksum", "INV-42");
checksum.Should().BeGreaterThan(0);
```

It is not something I would build a whole testing strategy around, but it is useful when you need it.

## When Atc.Test is at its best

I think Atc.Test provides the most value when:

- your services have constructor-heavy dependency graphs
- you write a lot of theory-based unit tests
- you already use NSubstitute and AutoFixture
- you want less boilerplate and more intent in each test
- you want shared customization rules applied consistently across the whole test suite

For very small projects or extremely explicit tests, plain xUnit with manually wired substitutes may still be enough. But once the test suite starts growing, the reduction in repetition becomes very noticeable.

## Conclusion

Atc.Test takes a testing stack that is already popular in .NET and makes it much nicer to use day to day. The biggest win is not that it does something magical. The win is that it removes repetitive setup and lets the test method describe the scenario much more directly.

If you are already using xUnit, AutoFixture, NSubstitute, and FluentAssertions, then Atc.Test is a very easy library to adopt. Start with `AutoNSubstituteData`, add `[Frozen]` where you need shared collaborators, and then bring in the other helpers when they solve a real problem in your tests.

The source code is available on GitHub at [https://github.com/atc-net/atc-test](https://github.com/atc-net/atc-test), and the package is available on NuGet at [https://www.nuget.org/packages/Atc.Test/](https://www.nuget.org/packages/Atc.Test/).
