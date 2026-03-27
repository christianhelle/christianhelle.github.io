---
layout: post
title: Atc.Test - Unit testing for .NET with A Touch of Class
date: "2025-07-22"
author: Christian Helle
tags:
  - .NET
  - Unit Testing
  - xUnit
  - AutoFixture
  - NSubstitute
  - FluentAssertions
redirect_from:
  - /2025/07/22/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /2025/07/22/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
---

I have been writing unit tests in .NET for a long time. Over the years, I have tried many combinations of testing frameworks, mocking libraries, and assertion libraries. For a long time, my go-to stack was xUnit, AutoFixture, NSubstitute, and FluentAssertions. Each of these tools does its job well, but putting them all together in a consistent way across a growing test suite always required a surprising amount of repetitive setup code.

Every test class looked roughly the same: create a fixture, configure it, freeze a dependency, mock something, create the system under test, act, and assert. The pattern was fine, but the ceremony was not. When a constructor changes or a new dependency gets added, you end up touching dozens of test files just to fix the wiring.

[Atc.Test](https://github.com/atc-net/atc-test) is a library from the [ATC](https://github.com/atc-net) organization that solves exactly this problem. It wraps xUnit v3, AutoFixture, NSubstitute, and FluentAssertions into a cohesive, attribute-driven experience that eliminates boilerplate and lets you focus on the behavior you are actually testing.

This post is a comprehensive guide to using Atc.Test. I will walk through everything from getting started to advanced frozen reuse patterns, auto-registration of customizations, and the helper extensions that ship with the library.

## Why Atc.Test

Before diving into the details, it is worth understanding what problem Atc.Test solves and when it is worth adopting.

Without a library like this, your tests tend to accumulate a lot of noise. Every test method needs to wire up its own fixture, freeze the right dependencies, manually create substitutes, and construct the system under test. When your services have evolving constructor graphs, this becomes a maintenance burden. Adding a single constructor parameter to a class can cascade into touching dozens or even hundreds of test files.

Atc.Test addresses this with a simple principle: **you only list the parameters that matter to your test**. Everything else is automatically generated, mocked, and wired up by the library.

Here is a quick comparison:

- **Fragile refactors** become a thing of the past. When you add a new constructor dependency, the fixture auto-supplies it. No test changes needed.
- **Divergent mock styles** across the team get replaced with a central factory and consistent frozen reuse semantics.
- **Duplicate substitutes** for logically single collaborators are prevented by the `[Frozen]` attribute and its exact-type promotion logic.
- **Shared customization rules** (recursion handling, custom generators) are registered once and inherited by every test automatically.

This delivers the most value in mid-to-large test suites with hundreds or more theory cases, domain services with complex constructor graphs, and teams that care about refactor safety and consistent test style.

## Getting Started

### Installing the Package

Add the `Atc.Test` NuGet package to your test project. Since the library is built on xUnit v3, you also need to explicitly reference xUnit and the test SDK:

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

### Why xUnit Must Be Referenced Directly

This is a deliberate design decision worth understanding. Atc.Test depends on `xunit.v3.extensibility.core` (the extensibility surface) but intentionally does not bring in the full `xunit.v3` meta-package. There are a few reasons for this:

1. **No NU1701 warnings.** Runner assets do not always target `netstandard2.1`, which is one of Atc.Test's target frameworks. Pulling in the full meta-package can cause warnings.
2. **Version independence.** You can pin or float the xUnit version independently. If you want a different patch or minor version, just change your `<PackageReference>` line. No changes to Atc.Test are required.
3. **Separation of concerns.** The library provides attributes and utilities. You own the test infrastructure and runner decisions.

### xUnit v3 Only

Atc.Test relies on xUnit v3 extensibility APIs that do not exist in v2. Specifically:

- The async data attribute signature: `ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(...)`
- `ITheoryDataRow` with metadata preservation (Label, Explicit, Timeout)
- The `DisposalTracker` parameter passed to data attributes

This means Atc.Test is **not compatible with xUnit v2**. If you try to swap `xunit.v3` for the v2 `xunit` package, you will get build errors for missing types. If you run with a legacy v2 runner, test discovery will fail.

That said, you can mix v2 and v3 projects in the same solution. They just must not share v3-based base test classes.

### First Test

Here is the simplest possible test using Atc.Test:

```csharp
public class CalculatorTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Add_ShouldReturnSumOfTwoNumbers(int a, int b, Calculator sut)
        => sut.Add(a, b).Should().Be(a + b);
}
```

Three parameters, zero setup. `a` and `b` are random integers generated by AutoFixture. `Calculator` is automatically instantiated. If `Calculator` had constructor dependencies, they would be automatically mocked using NSubstitute. No constructor calls, no fixture setup, no mock declarations.

## Data Attributes

Atc.Test ships with four data attributes, each designed for a different scenario. Understanding when to use each one is the key to getting the most out of the library.

### AutoNSubstituteData

The `[AutoNSubstituteData]` attribute is the workhorse. Use it when you want every parameter to be auto-generated. Interfaces and abstract classes are automatically substituted using NSubstitute.

```csharp
[Theory]
[AutoNSubstituteData]
public void GetValue_ShouldReturnDataFromService(
    IMyService mockService,
    MyController sut)
{
    mockService.GetValue().Returns(42);

    var result = sut.Get();

    result.Should().Be(42);
}
```

In this example, `mockService` is an NSubstitute substitute for `IMyService`. The `MyController` is constructed with that same substitute injected automatically. You never have to write `Substitute.For<IMyService>()` or `new Fixture()` ever again.

### InlineAutoNSubstituteData

Use `[InlineAutoNSubstituteData]` when you want to provide specific values for some parameters while letting AutoFixture generate the rest. This is the auto-mocking equivalent of xUnit's `[InlineData]`.

```csharp
[Theory]
[InlineAutoNSubstituteData(10, 20)]
[InlineAutoNSubstituteData(5, 5)]
[InlineAutoNSubstituteData(0, -1)]
public void Add_ShouldWorkWithSpecificValues(
    int a,
    int b,
    Calculator sut)
{
    var result = sut.Add(a, b);

    result.Should().Be(a + b);
}
```

The inline values are assigned to the first parameters in order. Any remaining parameters are auto-generated. This is useful when you need to test specific edge cases or boundary conditions.

### MemberAutoNSubstituteData

Use `[MemberAutoNSubstituteData]` when you need more complex test data that cannot be expressed inline. This works like xUnit's `[MemberData]` but augments the provided rows with auto-generated specimens.

```csharp
public static IEnumerable<object?[]> TestCases()
{
    yield return new object?[] { 1, 2, 3 };
    yield return new object?[] { 10, 20, 30 };
    yield return new object?[] { -1, 1, 0 };
}

[Theory]
[MemberAutoNSubstituteData(nameof(TestCases))]
public void Add_ShouldReturnExpectedResult(
    int a,
    int b,
    int expected,
    Calculator sut)
{
    var result = sut.Add(a, b);

    result.Should().Be(expected);
}
```

The member data provides the first N values (in this case `a`, `b`, and `expected`). The remaining parameters (`sut`) are auto-generated. This is especially powerful when combined with the `[Frozen]` attribute, which I will cover later.

### ClassAutoNSubstituteData

Use `[ClassAutoNSubstituteData]` when your test data is complex enough to warrant its own class. This works like xUnit's `[ClassData]` but with auto-mocking.

```csharp
public class CalculatorTestCases : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, 2, 3 };
        yield return new object[] { 10, 20, 30 };
        yield return new object[] { -1, 1, 0 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassAutoNSubstituteData(typeof(CalculatorTestCases))]
public void Add_ShouldReturnExpectedResult(
    int a,
    int b,
    int expected,
    Calculator sut)
{
    var result = sut.Add(a, b);

    result.Should().Be(expected);
}
```

This is useful when your test data requires construction logic, conditional rows, or shared state between the test cases.

## The [Frozen] Attribute

One of the most important concepts in AutoFixture-based testing is **freezing**. When you freeze a value, the same instance is reused for every other parameter in the same test that requires that type. This is critical when you need to set up a mock and then verify that the same mock was used by the system under test.

### Basic Frozen Reuse

```csharp
[Theory]
[AutoNSubstituteData]
public void Handle_ShouldUseFrozenDependency(
    [Frozen] IMyService mockService,
    MyHandler sut)
{
    mockService.DoWork().Returns(true);

    var result = sut.Handle();

    result.Should().BeTrue();
    mockService.Received(1).DoWork();
}
```

Without `[Frozen]`, `mockService` and the `IMyService` injected into `MyHandler` would be different instances. With `[Frozen]`, they are the same object. This means the setup on `mockService` is visible to the system under test.

### Positional Frozen Reuse with Inline Data

When you use `[InlineAutoNSubstituteData]` or `[ClassAutoNSubstituteData]`, the `[Frozen]` attribute supports **positional reuse**. If a value is supplied at the same index as a `[Frozen]` parameter, that supplied instance is frozen and reused for all other parameters of the same type.

```csharp
[Theory]
[InlineAutoNSubstituteData(42)]
public void Positional_Frozen_Reuses_Inline_Value(
    [Frozen] int number,
    SomeConsumer consumer)
{
    consumer.NumberDependency.Should().Be(number);
}
```

Here, the inline value `42` is at index 0, which maps to the `[Frozen] int number` parameter. That value is then frozen and reused when constructing `SomeConsumer`.

### Exact-Type Promotion with Member Data

`MemberAutoNSubstituteData` adds an additional feature called **exact-type promotion**. If the member data supplies a value for an earlier parameter, and a later parameter is marked `[Frozen]` with the same exact type, the earlier supplied value is promoted and reused.

```csharp
public static IEnumerable<object?[]> ServiceRow()
{
    yield return new object?[] { Substitute.For<IMyService>() };
}

[Theory]
[MemberAutoNSubstituteData(nameof(ServiceRow))]
public void Promotion_Reuses_Earlier_Same_Type(
    IMyService supplied,
    [Frozen] IMyService frozenLater,
    NeedsService consumer)
{
    frozenLater.Should().BeSameAs(supplied);
    consumer.Service.Should().BeSameAs(supplied);
}
```

The member data supplies `IMyService` at index 0. The `[Frozen] IMyService` at index 1 was not part of the member row, so the library promotes the earlier supplied value instead of creating a new substitute.

### What Is NOT Promoted

It is important to understand that promotion only works for **exact type matches**. If you supply an instance of a concrete class that implements two different interfaces, the library will not cross-promote across interfaces.

```csharp
public interface IFoo {}
public interface IBar {}
public class DualImpl : IFoo, IBar {}

public static IEnumerable<object?[]> DualRow()
{
    yield return new object?[] { new DualImpl() };
}

[Theory]
[MemberAutoNSubstituteData(nameof(DualRow))]
public void Different_Interface_Not_Promoted(
    IFoo foo,
    [Frozen] IBar bar,
    UsesBar consumer)
{
    bar.Should().NotBeSameAs(foo);
    consumer.Bar.Should().BeSameAs(bar);
}
```

Even though `DualImpl` implements both `IFoo` and `IBar`, the frozen `IBar` parameter does not reuse the `DualImpl` instance because the declared types do not match exactly. This is a deliberate design choice to prevent cross-interface bleed and subtle bugs.

## Auto-Registration of Customizations

When you need to customize how AutoFixture generates certain types, you normally have to configure the fixture manually for every test. Atc.Test provides a way to avoid this with the `[AutoRegister]` attribute.

Any class that implements `ICustomization` or `ISpecimenBuilder` and is decorated with `[AutoRegister]` is automatically discovered and applied to every fixture created by the library.

```csharp
[AutoRegister]
public class GuidCustomization : ICustomization
{
    public void Customize(IFixture fixture)
        => fixture.Register(() => Guid.NewGuid());
}
```

With this in place, every call to `FixtureFactory.Create()` will include your customization. Any test that needs a `Guid` will get one automatically, without any per-test configuration.

This is particularly useful for:

- **Registering default values** for domain types that AutoFixture cannot construct by default
- **Applying recursion guards** to prevent infinite object graphs
- **Configuring string generators** to produce realistic test data instead of random gibberish
- **Setting up custom builders** for types with complex construction logic

The `[AutoRegister]` attribute is a powerful way to establish project-wide testing conventions. Put these customizations in your test project and every test inherits them automatically.

## The FixtureFactory

Under the hood, every data attribute in Atc.Test uses `FixtureFactory.Create()` to produce a configured `IFixture` instance. The factory applies three default customizations:

1. **RecursionCustomization** - Handles circular references gracefully instead of throwing `ObjectCreationException`.
2. **AutoRegisterCustomization** - Discovers and applies all `[AutoRegister]`-decorated customizations and specimen builders.
3. **AutoNSubstituteCustomization** - Configures NSubstitute to generate substitutes for interfaces and abstract classes, with `ConfigureMembers = false` and `GenerateDelegates = true`.

You can use `FixtureFactory.Create()` directly in your own test setup code if you need a fixture outside of the data attributes:

```csharp
[Fact]
public void Manual_Fixture_Example()
{
    var fixture = FixtureFactory.Create();
    var service = fixture.Create<IMyService>();
    var sut = fixture.Create<MyHandler>();

    var result = sut.Handle();

    result.Should().NotBeNull();
}
```

## Helper Extensions

Atc.Test ships with several convenience extension classes that reduce boilerplate when working with FluentAssertions and NSubstitute.

### EquivalencyAssertionOptionsExtensions

When comparing objects with `BeEquivalentTo`, you often run into problems with `DateTime` and `DateTimeOffset` precision, or with `JsonElement` comparisons. The equivalency extensions provide convenient configuration methods.

**DateTime precision:**

```csharp
[Theory]
[AutoNSubstituteData]
public void Mapping_ShouldPreserveTimestamp(
    SourceModel source,
    IMapper sut)
{
    var result = sut.Map(source);

    result.Should().BeEquivalentTo(source, opts => opts
        .CompareDateTimeUsingCloseTo());
}
```

By default, `CompareDateTimeUsingCloseTo` uses a precision of 1000 milliseconds. You can override this:

```csharp
result.Should().BeEquivalentTo(source, opts => opts
    .CompareDateTimeUsingCloseTo(precision: 500));
```

Or pass a `TimeSpan`:

```csharp
result.Should().BeEquivalentTo(source, opts => opts
    .CompareDateTimeUsingCloseTo(TimeSpan.FromSeconds(2)));
```

**JsonElement comparison:**

```csharp
result.Should().BeEquivalentTo(expected, opts => opts
    .CompareJsonElementUsingJson());
```

This compares `JsonElement` values by their underlying JSON string representation instead of trying to compare the struct directly.

### SubstituteExtensions

The `SubstituteExtensions` class provides helpers for inspecting substitute calls and waiting for asynchronous interactions.

**Getting call arguments:**

```csharp
[Theory]
[AutoNSubstituteData]
public void Process_ShouldPassCorrectArgument(
    IMyService mockService,
    MyHandler sut)
{
    sut.Process("hello");

    var argument = mockService.ReceivedCallWithArgument<string>();
    argument.Should().Be("hello");
}
```

If you expect multiple calls:

```csharp
var arguments = mockService.ReceivedCallsWithArguments<string>();
arguments.Should().Contain("hello");
```

**Waiting for async calls:**

When testing asynchronous code where the call happens on a background thread or after an event, you can wait for the call with a timeout:

```csharp
[Theory]
[AutoNSubstituteData]
public async Task ProcessAsync_ShouldCallService(
    IMyService mockService,
    MyHandler sut)
{
    sut.StartBackgroundProcessing();

    await mockService.WaitForCall(x => x.DoWork());
}
```

By default, this waits up to 5 seconds. You can specify a custom timeout:

```csharp
await mockService.WaitForCall(x => x.DoWork(), TimeSpan.FromSeconds(10));
```

There are overloads for `Action<T>`, `Func<T, Task>`, and `Func<T, ValueTask<TResult>>`. You can also wait for any arguments with `WaitForCallForAnyArgs`.

### TaskExtensions

The `TaskExtensions` class provides helpers for managing asynchronous operations in tests.

**Awaiting with a timeout:**

```csharp
var result = await someTask.AddTimeout(TimeSpan.FromSeconds(5));
```

This throws a `TimeoutException` if the task does not complete within the specified time. The default timeout is 5 seconds. When a debugger is attached, the timeout is ignored to make debugging easier.

**Awaiting multiple tasks:**

```csharp
var tasks = new[] { task1, task2, task3 };
var results = await tasks.AwaitTasks();
```

This calls `Task.WhenAll` under the hood but reads more naturally as an extension method.

### ObjectExtensions

The `ObjectExtensions` class provides reflection helpers for cases where you need to access protected members in tests.

**Invoking a protected method:**

```csharp
var result = sut.InvokeProtectedMethod<string>("DoSomethingInternal", arg1, arg2);
```

**Invoking with a typed return value:**

```csharp
var result = sut.InvokeProtectedMethod<int>("CalculateInternal", input);
```

**Checking for properties:**

```csharp
if (obj.HasProperties())
{
    obj.Should().BeEquivalentTo(expected);
}
```

The `HasProperties` method is useful as a guard before calling `BeEquivalentTo`, which will throw for objects with no properties.

## Requirements

Atc.Test targets `netstandard2.1`, `net8.0`, and `net9.0`, giving it broad compatibility across .NET versions. It requires xUnit v3 as the test framework and uses NSubstitute transitively for mocking. FluentAssertions is recommended for assertions.

The library is multi-targeted to support teams that are not always on the latest .NET version, while still taking advantage of newer APIs when available.

## Conclusion

Atc.Test brings together the best testing tools in the .NET ecosystem and makes them work as a cohesive unit. The data attributes eliminate boilerplate, the `[Frozen]` attribute ensures consistent mock wiring, and the `[AutoRegister]` attribute lets you establish project-wide testing conventions with a single attribute.

What I like most about this library is that it does not try to replace the tools you already know. It does not hide xUnit, AutoFixture, NSubstitute, or FluentAssertions behind an abstraction. You still use those tools directly when you need to. What Atc.Test does is remove the repetitive setup code that makes tests noisy and fragile to refactoring.

If you are maintaining a test suite of any meaningful size and you are already using xUnit, AutoFixture, and NSubstitute, this library is a straightforward way to make your tests cleaner and your refactoring safer.

You can find the full source code, documentation, and examples on the [GitHub repository](https://github.com/atc-net/atc-test). The library is available on [NuGet](https://www.nuget.org/packages/Atc.Test).
