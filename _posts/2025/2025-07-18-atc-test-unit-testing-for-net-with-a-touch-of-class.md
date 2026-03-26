---
layout: post
title: Atc.Test - Unit testing for .NET with A Touch of Class
date: 2025-07-18
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
  - /2025/atc-test-unit-testing-for-net-with-a-touch-of-class/
  - /2025/atc-test-unit-testing-for-net-with-a-touch-of-class
---

## Introduction

Unit testing is a cornerstone of robust software development, but setting up tests with proper mocking and data generation can often feel repetitive and cumbersome. Atc.Test is a .NET testing library that simplifies this process by integrating xUnit v3, AutoFixture, NSubstitute, and FluentAssertions into a cohesive, ergonomic testing framework. It reduces boilerplate code while maintaining clarity and focus on what matters most: verifying your code's behavior.

In this comprehensive guide, we'll explore Atc.Test from the ground up. Whether you're new to testing or looking to streamline an existing test suite, this post will walk you through installation, core concepts, and advanced patterns with practical examples.

## Getting Started

### Installation

To start using Atc.Test, add it to your test project along with the necessary dependencies:

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
    <PackageReference Include="Atc.Test" Version="1.0.0" />
  </ItemGroup>
</Project>
```

Note that Atc.Test requires xUnit v3 explicitly referenced in your project. It doesn't include it transitively to give you control over versioning and to avoid potential compatibility issues.

### Why xUnit v3?

Atc.Test is built specifically for xUnit v3, which introduced significant improvements over v2:

- Enhanced extensibility APIs for data attributes
- Better async support
- Improved metadata handling

If you're still using xUnit v2, you'll need to upgrade to take advantage of Atc.Test. The migration is generally straightforward, and the benefits are worth it.

### Your First Test with Atc.Test

Let's start with a simple example. Suppose we have a calculator service:

```csharp
public interface ILogger
{
    void Log(string message);
}

public class Calculator
{
    private readonly ILogger _logger;

    public Calculator(ILogger logger)
    {
        _logger = logger;
    }

    public int Add(int a, int b)
    {
        var result = a + b;
        _logger.Log($"Calculated: {a} + {b} = {result}");
        return result;
    }
}
```

Here's how you'd test it with Atc.Test:

```csharp
public class CalculatorTests
{
    [Theory]
    [AutoNSubstituteData]
    public void Add_returns_correct_sum(
        int a, int b, 
        Calculator sut)
    {
        var result = sut.Add(a, b);
        result.Should().Be(a + b);
    }
}
```

That's it! The `[AutoNSubstituteData]` attribute automatically:

- Generates random values for `a` and `b`
- Creates a mock for `ILogger` using NSubstitute
- Instantiates the `Calculator` with the mock
- Provides everything to your test method

## Core Data Attributes

Atc.Test provides several data attributes to suit different testing scenarios:

### AutoNSubstituteData

The most basic attribute that auto-generates all parameters:

```csharp
[Theory]
[AutoNSubstituteData]
public void Method_under_test_auto_generates_all_parameters(
    SomeClass sut, 
    IDependency mock1, 
    IDependency mock2, 
    int value, 
    string text)
{
    // Test implementation
}
```

### InlineAutoNSubstituteData

When you need specific values for some parameters:

```csharp
[Theory]
[InlineAutoNSubstituteData(5, 10)]
public void Add_with_specific_values(int a, int b, Calculator sut)
{
    var result = sut.Add(a, b);
    result.Should().Be(15);
}
```

### MemberAutoNSubstituteData

For reusing test data across multiple tests:

```csharp
public static IEnumerable<object?[]> CalculatorTestCases()
{
    yield return new object?[] { 2, 3, 5 };
    yield return new object?[] { -1, 1, 0 };
    yield return new object?[] { 0, 0, 0 };
}

[Theory]
[MemberAutoNSubstituteData(nameof(CalculatorTestCases))]
public void Add_with_multiple_test_cases(int a, int b, int expected, Calculator sut)
{
    var result = sut.Add(a, b);
    result.Should().Be(expected);
}
```

### ClassAutoNSubstituteData

Similar to member data but defined in a separate class:

```csharp
public class CalculatorTestData : TheoryData<int, int, int>
{
    public CalculatorTestData()
    {
        Add(1, 2, 3);
        Add(4, 5, 9);
        Add(10, 20, 30);
    }
}

[Theory]
[ClassAutoNSubstituteData(typeof(CalculatorTestData))]
public void Add_with_class_data(int a, int b, int expected, Calculator sut)
{
    var result = sut.Add(a, b);
    result.Should().Be(expected);
}
```

## Frozen Dependencies

One of Atc.Test's most powerful features is the ability to freeze dependencies, ensuring the same mock instance is reused across multiple parameters. This is crucial when you need to verify interactions.

### Basic Freezing

```csharp
[Theory]
[AutoNSubstituteData]
public void Service_logs_messages(
    [Frozen] ILogger logger,
    Calculator sut)
{
    sut.Add(1, 2);
    
    logger.Received(1).Log("Calculated: 1 + 2 = 3");
}
```

The `[Frozen]` attribute ensures that the `ILogger` instance passed to the constructor is the same one available in the test for verification.

### Advanced Freezing Scenarios

#### Multiple Frozen Dependencies

```csharp
public interface IRepository
{
    Task SaveAsync(Entity entity);
}

public interface INotifier
{
    Task NotifyAsync(string message);
}

public class OrderService
{
    private readonly IRepository _repository;
    private readonly INotifier _notifier;

    public OrderService(IRepository repository, INotifier notifier)
    {
        _repository = repository;
        _notifier = notifier;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        await _repository.SaveAsync(order);
        await _notifier.NotifyAsync($"Order {order.Id} processed");
    }
}

[Theory]
[AutoNSubstituteData]
public async Task ProcessOrderAsync_saves_and_notifies(
    [Frozen] IRepository repository,
    [Frozen] INotifier notifier,
    OrderService sut,
    Order order)
{
    await sut.ProcessOrderAsync(order);
    
    await repository.Received(1).SaveAsync(order);
    await notifier.Received(1).NotifyAsync($"Order {order.Id} processed");
}
```

#### Frozen with Specific Values

```csharp
[Theory]
[InlineAutoNSubstituteData("test message")]
public void Process_with_specific_input(
    string message,
    [Frozen] IMessageProcessor processor,
    MessageService sut)
{
    sut.Process(message);
    
    processor.Received(1).Process(message);
}
```

## Customizations and Auto Registration

Atc.Test allows you to customize how AutoFixture generates data and creates mocks through the `[AutoRegister]` attribute.

### Basic Customization

```csharp
[AutoRegister]
public class EmailCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<string>(c => c.FromFactory(() => 
            $"{Guid.NewGuid()}@example.com"));
    }
}

[Theory]
[AutoNSubstituteData]
public void User_registration_generates_valid_email(
    UserRegistrationService sut,
    string email) // Will be a valid email format
{
    // Test implementation
}
```

### Interface Mocking Customization

```csharp
[AutoRegister]
public class RepositoryCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<IRepository>(c => c.FromFactory(() =>
        {
            var mock = Substitute.For<IRepository>();
            mock.SaveAsync(Arg.Any<Entity>())
                .Returns(Task.CompletedTask);
            return mock;
        }));
    }
}
```

### Specimen Builders

For more complex customization needs:

```csharp
[AutoRegister]
public class ComplexEntityBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(ComplexEntity))
        {
            return new ComplexEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
        }
        
        return new NoSpecimen();
    }
}
```

## Helper Extensions

Atc.Test includes several helpful extension methods to make testing easier.

### Substitute Extensions

```csharp
[Theory]
[AutoNSubstituteData]
public void Service_calls_external_api(
    [Frozen] IExternalApi api,
    ExternalService sut)
{
    // Set up mock behavior
    api.GetDataAsync().Returns(new ApiResponse { Success = true });
    
    // Act
    var result = sut.GetData();
    
    // Verify calls
    api.Received().GetDataAsync();
    
    // Get call arguments
    var calls = api.ReceivedCalls();
    var call = calls.First();
    var args = call.GetArguments();
}
```

### Task Extensions for Timeouts

```csharp
[Theory]
[AutoNSubstituteData]
public async Task Long_running_operation_completes_quickly(
    [Frozen] ISlowService slowService,
    FastService sut)
{
    slowService.ProcessAsync().Returns(Task.Delay(100)); // Simulate 100ms work
    
    // This will throw TimeoutException if it takes longer than 1 second
    await sut.ProcessAsync().AddTimeout(TimeSpan.FromSeconds(1));
}
```

### Object Extensions for Protected Members

```csharp
public class LegacyService
{
    protected virtual bool ValidateInput(string input)
    {
        return !string.IsNullOrEmpty(input) && input.Length > 3;
    }
    
    public void Process(string input)
    {
        if (ValidateInput(input))
        {
            // Process input
        }
    }
}

[Theory]
[InlineAutoNSubstituteData("valid")]
[InlineAutoNSubstituteData("x")]
public void ValidateInput_works_correctly(string input, LegacyService sut)
{
    var isValid = sut.InvokeProtectedMethod<bool>("ValidateInput", input);
    isValid.Should().Be(input.Length > 3);
}
```

## Testing Asynchronous Code

Atc.Test provides excellent support for testing async operations:

```csharp
public class AsyncService
{
    private readonly IDependency _dependency;
    
    public AsyncService(IDependency dependency)
    {
        _dependency = dependency;
    }
    
    public async Task<string> ProcessAsync()
    {
        var data = await _dependency.GetDataAsync();
        return $"Processed: {data}";
    }
}

[Theory]
[AutoNSubstituteData]
public async Task ProcessAsync_handles_data_correctly(
    [Frozen] IDependency dependency,
    AsyncService sut)
{
    dependency.GetDataAsync().Returns("test data");
    
    var result = await sut.ProcessAsync();
    
    result.Should().Be("Processed: test data");
    await dependency.Received(1).GetDataAsync();
}
```

## Best Practices

### When to Use Each Attribute

- **AutoNSubstituteData**: Default choice for most tests
- **InlineAutoNSubstituteData**: When you need specific input values
- **MemberAutoNSubstituteData**: When reusing test data across tests
- **ClassAutoNSubstituteData**: For complex test data scenarios
- **Frozen**: When you need to verify interactions with dependencies

### Organizing Tests

```csharp
public class UserServiceTests
{
    // Group related tests
    public class Registration
    {
        [Theory]
        [AutoNSubstituteData]
        public void Valid_user_registers_successfully(/* parameters */)
        {
            // Test implementation
        }
        
        [Theory]
        [AutoNSubstituteData]
        public void Invalid_user_fails_registration(/* parameters */)
        {
            // Test implementation
        }
    }
    
    public class Authentication
    {
        // Authentication tests
    }
}
```

### Performance Considerations

- Avoid over-freezing dependencies you don't need to verify
- Use member/class data for performance-critical tests that need specific values
- Consider fixture reuse for test classes with many similar tests

## Common Patterns and Anti-Patterns

### Good: Focused Tests

```csharp
[Theory]
[AutoNSubstituteData]
public void CalculateTax_excludes_free_items(
    [Frozen] ITaxCalculator taxCalculator,
    ShoppingCart sut,
    Item freeItem,
    Item paidItem)
{
    freeItem.IsTaxable = false;
    paidItem.IsTaxable = true;
    paidItem.Price = 100;
    
    taxCalculator.CalculateTax(100).Returns(10);
    
    var tax = sut.CalculateTax(new[] { freeItem, paidItem });
    
    tax.Should().Be(10);
}
```

### Avoid: Over-Mocking

```csharp
// Anti-pattern: Mocking everything
[Theory]
[AutoNSubstituteData]
public void Complex_test_with_too_many_frozen_dependencies(
    [Frozen] IService1 s1,
    [Frozen] IService2 s2,
    [Frozen] IService3 s3,
    [Frozen] IService4 s4,
    [Frozen] IService5 s5,
    ComplexService sut)
{
    // This test is trying to do too much
}
```

### Good: Clear Intent

```csharp
// Good: Each test has a single, clear responsibility
[Theory]
[AutoNSubstituteData]
public void SaveUser_persists_to_repository(
    [Frozen] IUserRepository repository,
    UserService sut,
    User user)
{
    sut.SaveUser(user);
    repository.Received(1).Save(user);
}

[Theory]
[AutoNSubstituteData]
public void SaveUser_validates_input(
    [Frozen] IValidator validator,
    UserService sut,
    User invalidUser)
{
    validator.Validate(invalidUser).Returns(new ValidationResult { IsValid = false });
    
    sut.Invoking(s => s.SaveUser(invalidUser))
        .Should().Throw<ValidationException>();
}
```

## Integration with CI/CD

Atc.Test works seamlessly with your existing CI/CD pipeline. Since it builds on xUnit, it integrates with all standard .NET testing tools:

```xml
<!-- In your CI pipeline -->
<Exec Command="dotnet test --logger trx --results-directory TestResults" />
```

## Troubleshooting

### Common Issues

1. **"xUnit v3 not found"**: Ensure you have an explicit reference to xUnit v3 in your test project.

2. **Mock not behaving as expected**: Check that you're using `[Frozen]` correctly for verification.

3. **Slow test execution**: Review your customizations - complex builders can impact performance.

4. **Random test failures**: Ensure your customizations are deterministic or use seeded fixtures for consistency.

## Conclusion

Atc.Test transforms unit testing in .NET by eliminating repetitive setup code while maintaining the flexibility and power you need. By integrating AutoFixture's data generation, NSubstitute's mocking capabilities, and FluentAssertions' expressive assertions, it provides a modern, productive testing experience.

The key benefits include:

- **Reduced boilerplate**: Focus on test logic, not setup
- **Consistent patterns**: Standardized approach across your codebase
- **Powerful customization**: Flexible when you need it
- **Modern async support**: Built for contemporary .NET development

Start small by replacing your existing test setup with `[AutoNSubstituteData]`, then gradually incorporate frozen dependencies and customizations as you identify patterns in your codebase. The result will be cleaner, more maintainable tests that clearly express the behavior you're verifying.

Whether you're working on a new project or refactoring an existing test suite, Atc.Test provides the tools to make testing more enjoyable and effective. Give it a try in your next test - you might find yourself wondering how you ever tested without it.

The source code is available on GitHub at [https://github.com/atc-net/atc-test](https://github.com/atc-net/atc-test), and the package is available on NuGet at [https://www.nuget.org/packages/Atc.Test/](https://www.nuget.org/packages/Atc.Test/).