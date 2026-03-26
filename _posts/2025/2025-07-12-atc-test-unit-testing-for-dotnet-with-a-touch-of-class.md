---
layout: post
title: Atc.Test - Unit testing for .NET with A Touch of Class
date: 2025-07-12
author: Christian Helle
tags:
  - .NET
  - Testing
  - xUnit
  - AutoFixture
  - NSubstitute
  - FluentAssertions
redirect_from:
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/07/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /2025/atc-test-unit-testing-for-dotnet-with-a-touch-of-class
  - /atc-test-unit-testing-for-dotnet-with-a-touch-of-class/
  - /atc-test-unit-testing-for-dotnet-with-a-touch-of-class
---

Atc.Test has become an essential part of my .NET testing toolkit, significantly reducing boilerplate while maintaining test clarity. After using it extensively across multiple projects, I've discovered several patterns and advanced techniques that maximize its benefits. This post shares those insights, focusing on practical applications and real-world scenarios where Atc.Test truly shines.

## Beyond the Basics: Advanced Frozen Scenarios

While the basic `[Frozen]` attribute is powerful, understanding its nuances unlocks even greater testing efficiency. Let's explore some advanced scenarios that commonly arise in complex service hierarchies.

### Nested Dependency Freezing

Consider a service with dependencies that themselves have dependencies:

```csharp
public interface INotificationTemplateProvider
{
    string GetTemplate(NotificationType type);
}

public interface IMessageQueue
{
    Task EnqueueAsync(Message message);
}

public interface IEmailSender
{
    Task SendAsync(EmailMessage message);
}

public class NotificationService(
    INotificationTemplateProvider templateProvider,
    IMessageQueue messageQueue,
    IEmailSender emailSender)
{
    public async Task SendWelcomeAsync(User user)
    {
        var template = templateProvider.GetTemplate(NotificationType.Welcome);
        var email = new EmailMessage(
            user.Email,
            $"Welcome {user.Name}!",
            template.Replace("{name}", user.Name));

        await emailSender.SendAsync(email);
        await messageQueue.EnqueueAsync(new Message(
            "notification-sent",
            JsonSerializer.Serialize(new { user.Id, timestamp: DateTime.UtcNow })));
    }
}
```

Testing this service requires coordinating multiple frozen dependencies:

```csharp
public class NotificationServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task SendWelcomeAsync_uses_correct_template_and_channels(
        [Frozen] INotificationTemplateProvider templateProvider,
        [Frozen] IMessageQueue messageQueue,
        [Frozen] IEmailSender emailSender,
        NotificationService sut,
        User user)
    {
        // Arrange
        templateProvider.GetTemplate(NotificationType.Welcome)
            .Returns("Hello {name}, welcome to our platform!");

        // Act
        await sut.SendWelcomeAsync(user);

        // Assert
        await emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(e =>
                e.To == user.Email &&
                e.Subject == $"Welcome {user.Name}!"));

        await messageQueue.Received(1).EnqueueAsync(
            Arg.Is<Message>(m =>
                m.Type == "notification-sent" &&
                m.Payload.Contains($"\"Id\":\"{user.Id}\"")));
    }
}
```

Notice how we freeze all three dependencies at once, allowing us to verify interactions with each after exercising the service under test.

### Conditional Freezing with Factory Patterns

Sometimes you need different instances of the same dependency based on test conditions. Atc.Test handles this elegantly through factory patterns:

```csharp
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessAsync(PaymentDetails details);
}

public enum PaymentProvider
{
    Stripe,
    PayPal,
    BankTransfer
}

public class PaymentService(
    IPaymentProcessor stripeProcessor,
    IPaymentProcessor paypalProcessor,
    IPaymentProcessor bankProcessor)
{
    public async Task<PaymentResult> ProcessAsync(
        PaymentDetails details,
        PaymentProvider provider)
    {
        IPaymentProcessor processor = provider switch
        {
            PaymentProvider.Stripe => stripeProcessor,
            PaymentProvider.PayPal => paypalProcessor,
            PaymentProvider.BankTransfer => bankProcessor,
            _ => throw new NotSupportedException()
        };

        return await processor.ProcessAsync(details);
    }
}
```

Testing this requires freezing different processors based on the test scenario:

```csharp
public class PaymentServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task ProcessAsync_routes_to_correct_processor(
        [Frozen] IPaymentProcessor stripeProcessor,
        [Frozen] IPaymentProcessor paypalProcessor,
        [Frozen] IPaymentProcessor bankProcessor,
        PaymentService sut,
        PaymentDetails details)
    {
        // Test Stripe routing
        stripeProcessor.ProcessAsync(details)
            .Returns(Task.FromResult(PaymentResult.Success()));

        var resultStripe = await sut.ProcessAsync(details, PaymentProvider.Stripe);
        resultStripe.Should().Be(PaymentResult.Success());
        await stripeProcessor.Received(1).ProcessAsync(details);
        
        // Clear received calls for next test
        stripeProvider.ClearReceivedCalls();

        // Test PayPal routing
        paypalProcessor.ProcessAsync(details)
            .Returns(Task.FromResult(PaymentResult.Success()));

        var resultPayPal = await sut.ProcessAsync(details, PaymentProvider.PayPal);
        resultPayPal.Should().Be(PaymentResult.Success());
        await paypalProcessor.Received(1).ProcessAsync(details);
    }
}
```

This pattern ensures each processor receives calls only when it should be active, making verification precise and readable.

## Combining Multiple Data Sources

Real-world testing often requires data from multiple sources. Atc.Test makes it seamless to combine inline, member, and class data with auto-generated values.

### Multi-Source Theory Data

Imagine testing a tax calculation service that depends on multiple variable inputs:

```csharp
public interface ITaxRateProvider
{
    decimal GetRate(string region, ProductCategory category);
}

public interface IDiscountCalculator
{
    decimal CalculateDiscount(decimal amount, CustomerTier tier);
}

public class PricingService(
    ITaxRateProvider taxRateProvider,
    IDiscountCalculator discountCalculator)
{
    public decimal CalculateFinalPrice(
        decimal basePrice,
        string region,
        ProductCategory category,
        CustomerTier tier)
    {
        var discount = discountCalculator.CalculateDiscount(basePrice, tier);
        var discountedPrice = basePrice - discount;
        var taxRate = taxRateProvider.GetRate(region, category);
        var taxAmount = discountedPrice * taxRate;
        
        return discountedPrice + taxAmount;
    }
}
```

We can combine specific test cases with auto-generated supporting data:

```csharp
public class PricingServiceTests
{
    public static IEnumerable<object?[]> BoundaryCases()
    {
        // Test boundary conditions for pricing tiers
        yield return new object?[] { 0m, "US", ProductCategory.Essentials, CustomerTier.Regular, 0m };
        yield return new object?[] { 0.01m, "US", ProductCategory.Luxury, CustomerTier.Premium, 0.01m * 1.2m };
        yield return new object?[] { 1000m, "EU", ProductCategory.Essentials, CustomerTier.Regular, 900m }; // After tax
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(BoundaryCases))]
    [InlineAutoNSubstituteData(50m, "CA", ProductCategory.Books, CustomerTier.Regular, 42.5m)]
    [ClassAutoNSubstituteData(typeof(RandomPricingScenarios))]
    public void CalculateFinalPrice_handles_various_scenarios_correctly(
        decimal basePrice,
        string region,
        ProductCategory category,
        CustomerTier tier,
        decimal expected,
        PricingService sut)
    {
        sut.CalculateFinalPrice(basePrice, region, category, tier)
            .Should().Be(expected);
    }
}

public class RandomPricingScenarios : TheoryData<decimal, string, ProductCategory, CustomerTier, decimal>
{
    public RandomPricingScenarios()
    {
        Add(75.50m, "TX", ProductCategory.Electronics, CustomerTier.Regular, 80.18m);
        Add(200m, "FL", ProductCategory.Clothing, CustomerTier.VIP, 156m);
        Add(15.99m, "NY", ProductCategory.Food, CustomerTier.New, 13.59m);
    }
}
```

This approach gives us:
1. Precise boundary condition testing from member data
2. Specific inline test cases for known calculations
3. Randomized scenarios from class data for broader coverage
4. Auto-generated mocks for all dependencies

### Dynamic Data Generation with Customizers

For even more flexibility, combine data attributes with custom fixture generators:

```csharp
[AutoRegister]
public sealed class RealisticPriceCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<decimal>(c => c.FromFactory(() => 
            new decimal(new Random().Next(100, 10000)) / 100)); // $1.00 to $99.99
        
        fixture.Customize<string>(c => c.FromFactory(() => 
            $"US-{new Random().Next(10000, 99999):D5}")); // US-ZIPCODE format
    }
}

public class PricingServiceDynamicTests
{
    [Theory]
    [AutoNSubstituteData]
    public void CalculateFinalPrice_produces_reasonable_results(
        [Frozen] ITaxRateProvider taxRateProvider,
        [Frozen] IDiscountCalculator discountCalculator,
        PricingService sut,
        decimal basePrice, // Auto-generated realistic price ($1.00-$99.99)
        string region,     // Auto-generated realistic region (US-ZIPCODE format)
        ProductCategory category,
        CustomerTier tier)
    {
        // Arrange realistic dependencies
        taxRateProvider.GetRate(Arg.Any<string>(), Arg.Any<ProductCategory>())
            .Returns(0.08m); // 8% tax rate
        
        discountCalculator.CalculateDiscount(Arg.Any<decimal>(), Arg.Any<CustomerTier>())
            .Returns(0m); // No discount for simplicity

        // Act
        var result = sut.CalculateFinalPrice(basePrice, region, category, tier);

        // Assert - result should be base price plus 8% tax
        result.Should().BeCloseTo(basePrice * 1.08m, 0.001m);
    }
}
```

This creates tests with realistic, domain-appropriate data while keeping the test intent clear.

## Advanced [AutoRegister] Patterns

The `[AutoRegister]` attribute enables powerful, suite-wide testing conventions that go beyond simple dependency injection.

### Protocol-Based Customizations

Instead of customizing individual types, create customizations based on interfaces or behaviors:

```csharp
public interface IIdentifiable
{
    Guid Id { get; }
}

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    string CreatedBy { get; }
    string? UpdatedBy { get; }
}

[AutoRegister]
public sealed class AuditableCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<IAuditable>(c => c.FromFactory(() =>
        {
            var auditable = Substitute.For<IAuditable>();
            auditable.CreatedAt.Returns(DateTime.UtcNow.AddDays(-30));
            auditable.UpdatedAt.Returns(DateTime.UtcNow.AddDays(-15));
            auditable.CreatedBy.Returns("system");
            auditable.UpdatedBy.Returns("system");
            return auditable;
        }));
    }
}

[AutoRegister]
public sealed class IdentifiableCustomization : ICustomization
{
    private static readonly GuidSequence _guidSequence = new();

    public void Customize(IFixture fixture)
    {
        fixture.Customize<IIdentifiable>(c => c.FromFactory(() =>
        {
            var identifiable = Substitute.For<IIdentifiable>();
            identifiable.Id.Returns(_guidSequence.Next());
            return identifiable;
        }));
    }
}

// Simple thread-safe GUID sequencer for repeatable tests
public class GuidSequence
{
    private int _counter;
    public Guid Next() => Guid.Parse($"00000000-0000-0000-0000-{Interlocked.Increment(ref _counter):D12}");
}
```

Now any test requiring `IIdentifiable` or `IAuditable` automatically gets properly configured instances without additional setup.

### Context-Specific Customizations

Sometimes you need different customizations based on test context. Combine `[AutoRegister]` with conditional logic:

```csharp
[AutoRegister]
public sealed class EnvironmentAwareDatabaseCustomization : ICustomization
{
    private readonly bool _isIntegrationTest;

    public EnvironmentAwareDatabaseCustomization(bool isIntegrationTest = false)
    {
        _isIntegrationTest = isIntegrationTest;
    }

    public void Customize(IFixture fixture)
    {
        if (_isIntegrationTest)
        {
            // Use real database connection for integration tests
            fixture.Register<IDbConnection>(() => 
                new SqlConnection("Server=testdb;Database=test;Trusted_Connection=True;"));
        }
        else
        {
            // Use substitute for unit tests
            var connection = Substitute.For<IDbConnection>();
            fixture.Register(connection);
        }
    }
}
```

Then in your test projects, register the appropriate version:

```csharp
// In unit test project
[AutoRegister]
public sealed class UnitTestDatabaseCustomization : EnvironmentAwareDatabaseCustomization
{
    public UnitTestDatabaseCustomization() : base(false) { }
}

// In integration test project  
[AutoRegister]
public sealed class IntegrationTestDatabaseCustomization : EnvironmentAwareDatabaseCustomization
{
    public IntegrationTestDatabaseCustomization() : base(true) { }
}
```

## Testing Asynchronous Code Effectively

Asynchronous testing presents unique challenges. Atc.Test's helper extensions make it much more manageable.

### Waiting for Specific Conditions

Instead of arbitrary delays, wait for actual conditions to be met:

```csharp
public class OrderProcessingService
{
    private readonly IMessageBus _messageBus;
    private readonly IOrderRepository _repository;
    private readonly SemaphoreSlim _processingLock = new(1, 1);

    public OrderProcessingService(IMessageBus messageBus, IOrderRepository repository)
    {
        _messageBus = messageBus;
        _repository = repository;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        await _processingLock.WaitAsync();
        try
        {
            order.Status = OrderStatus.Processing;
            await _repository.UpdateAsync(order);
            
            await _messageBus.PublishAsync(new OrderProcessed(
                order.Id, 
                DateTime.UtcNow));
            
            order.Status = OrderStatus.Completed;
            await _repository.UpdateAsync(order);
        }
        finally
        {
            _processingLock.Release();
        }
    }
}

public class OrderProcessingServiceTests
{
    [Fact]
    public async Task ProcessOrderAsync_publishes_event_when_complete()
    {
        var fixture = FixtureFactory.Create();
        var messageBus = fixture.Freeze<IMessageBus>();
        var repository = fixture.Freeze<IOrderRepository>();
        var sut = fixture.Create<OrderProcessingService>();
        var order = fixture.Build<Order>()
            .With(o => o.Status, OrderStatus.Pending)
            .Create();

        // Act
        var processTask = sut.ProcessOrderAsync(order);

        // Wait for the processing to begin (but not necessarily complete)
        await repository
            .Received(1)
            .UpdateAsync(Arg.Is<Order>(o => o.Status == OrderStatus.Processing));

        // Continue waiting for completion
        await processTask;

        // Assert - verify the event was published
        await messageBus.Received(1).PublishAsync(
            Arg.Is<OrderProcessed>(op => op.OrderId == order.Id));
    }
}
```

### Combining Timeouts with Async Waiting

Prevent hanging tests while still allowing sufficient time for async operations:

```csharp
public class BatchProcessingServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task ProcessBatchAsync_completes_within_time_limit(
        [Frozen] IBatchJobExecutor executor,
        BatchProcessingService sut,
        BatchJob job)
    {
        // Arrange - job takes 1.5 seconds to complete
        executor.ExecuteAsync(Arg.Any<BatchJob>())
            .Returns(Task.Delay(1500)); // Simulate work

        // Act & Assert - should complete within 3 seconds
        await sut.ProcessBatchAsync(job)
            .AddTimeout(TimeSpan.FromSeconds(3)); // Atc.Test helper

        await executor.Received(1).ExecuteAsync(job);
    }
}
```

If the operation exceeds the timeout, Atc.Test throws a clear `TimeoutException` with details about what was waiting.

## Testing Legacy Code with Protected Members

When working with legacy codebases, you often need to test protected methods without refactoring. Atc.Test provides safe, readable access to these members.

### Testing Complex Protected Logic

Consider a legacy calculator with complex protected validation logic:

```csharp
public class LegacyCalculator
{
    protected virtual bool IsValidInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        
        // Complex validation logic we want to test directly
        return input.Length >= 3 && 
               input.All(char.IsLetterOrDigit) &&
               !input.Contains("bad");
    }

    public int Calculate(string input)
    {
        if (!IsValidInput(input))
            throw new ArgumentException("Invalid input");

        return input.Length * 2; // Simplified calculation
    }
}

public class LegacyCalculatorTests
{
    [Theory]
    [InlineAutoNSubstituteData("abc", true)]
    [InlineAutoNSubstituteData("ab", false)]
    [InlineAutoNSubstituteData("a b", false)]
    [InlineAutoNSubstituteData("abc-def", true)]
    [InlineAutoNSubstituteData("abc bad", false)]
    public void IsValidInput_handles_various_inputs_correctly(
        string input,
        bool expectedIsValid,
        LegacyCalculator sut)
    {
        // Access protected method via Atc.Test helper
        var isValid = sut.InvokeProtectedMethod<bool>("IsValidInput", input);
        isValid.Should().Be(expectedIsValid);
    }

    [Theory]
    [InlineAutoNSubstituteData("abc", 6)]
    [InlineAutoNSubstituteData("abcd", 8)]
    public void Calculate_returns_correct_result_for_valid_input(
        string input,
        int expectedResult,
        LegacyCalculator sut)
    {
        // For valid input, we can test the public method directly
        var result = sut.Calculate(input);
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Calculate_throws_for_invalid_input()
    {
        var sut = new LegacyCalculator();
        sut.Invoking(c => c.Calculate("")) // Invalid input
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid input");
    }
}
```

This approach lets you test complex protected logic directly while still verifying public method behavior through normal channels.

### Testing Protected Properties and Fields

Atc.Test also supports accessing protected properties and fields:

```csharp
public class GameCharacter
{
    protected int _health;
    protected int MaxHealth => 100;

    protected virtual void RegenerateHealth()
    {
        _health = Math.Min(_health + 10, MaxHealth);
    }

    public int Health => _health;
    
    public void TakeDamage(int amount)
    {
        _health = Math.Max(_health - amount, 0);
    }
}

public class GameCharacterTests
{
    [Fact]
    public void RegenerateHealth_increases_health_correctly()
    {
        var fixture = FixtureFactory.Create();
        var sut = fixture.Create<GameCharacter>();
        
        // Set up initial state via protected field
        sut.SetProtectedField("_health", 50);
        
        // Act
        sut.InvokeProtectedMethod("RegenerateHealth");
        
        // Assert via public property
        sut.Health.Should().Be(60);
    }
    
    [Fact]
    public void MaxHealth_returns_correct_value()
    {
        var sut = new GameCharacter();
        var maxHealth = sut.GetProtectedProperty<int>("MaxHealth");
        maxHealth.Should().Be(100);
    }
}
```

## Performance Considerations and Best Practices

As your test suite grows, consider these performance optimizations and best practices.

### Fixture Reuse Across Tests

While `FixtureFactory.Create()` is convenient, creating a new fixture for every test can be expensive in large suites. Consider sharing fixtures when appropriate:

```csharp
public class CalculatorServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly ICalculatorRepository _repositoryMock;
    private readonly ILoggingService _loggerMock;
    private readonly CalculatorService _sut;

    public CalculatorServiceTests()
    {
        _fixture = FixtureFactory.Create();
        _repositoryMock = _fixture.Freeze<ICalculatorRepository>();
        _loggerMock = _fixture.Freeze<ILoggingService>();
        _sut = _fixture.Create<CalculatorService>();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public void Add_ReturnsSumOfTwoNumbers()
    {
        // Arrange
        _repositoryMock.GetNumberAsync(Arg.Any<int>())
            .Returns(5);
            
        _repositoryMock.GetNumberAsync(Arg.Any<int>())
            .Returns(3);

        // Act
        var result = _sut.Add(1, 2);

        // Assert
        result.Should().Be(8);
        _loggerMock.Received(1).LogInfo("Addition performed");
    }
    
    // More tests using the same fixture...
}
```

### Selective Data Generation

Be mindful of what data you actually need in each test. Over-generating can slow down tests unnecessarily:

```csharp
// Less efficient - generates all properties even if not used
[Theory]
[AutoNSubstituteData]
public void TestUsesOnlyTwoParameters(
    int unused1,
    string unused2,
    Guid unused3,
    decimal unused4,
    MyService sut,
    string actuallyUsedParameter)
{
    // Only uses sut and actuallyUsedParameter
}

// More efficient - only requests what's needed
[Theory]
[AutoNSubstituteData]
public void TestUsesOnlyTwoParameters(
    MyService sut,
    string actuallyUsedParameter)
{
    // Uses exactly what we need
}
```

### Combining Atc.Test with Benchmarking

For performance-sensitive code, combine Atc.Test with benchmarking tools:

```csharp
public class PerformanceTests
{
    [Fact]
    public void CalculateDiscount_PerformsWithinLimits()
    {
        var fixture = FixtureFactory.Create();
        var discountCalculator = fixture.Create<IDiscountCalculator>();
        var sut = fixture.Create<PricingService>();
        
        // Warm up
        sut.CalculateFinalPrice(100m, "US", ProductCategory.Books, CustomerTier.Regular);
        
        // Time multiple iterations
        var iterations = 10000;
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            sut.CalculateFinalPrice(100m, "US", ProductCategory.Books, CustomerTier.Regular);
        }
        
        sw.Stop();
        var averageMs = sw.Elapsed.TotalMilliseconds / iterations;
        
        // Should average less than 0.1ms per call
        averageMs.Should().BeLessThan(0.1);
    }
}
```

## When to Combine Different Approaches

Understanding when to use each Atc.Test feature leads to more effective tests.

### Decision Matrix for Test Data Attributes

| Scenario | Best Approach | Why |
|----------|---------------|-----|
| Simple test with no special requirements | `[AutoNSubstituteData]` | Maximum automation, minimal ceremony |
| Need specific values for key parameters | `[InlineAutoNSubstituteData]` | Precise control where it matters most |
| Reusing test data across multiple test classes | `[MemberAutoNSubstituteData]` / `[ClassAutoNSubstituteData]` | DRY principle for test data |
| Testing boundary conditions and edge cases | Member/Class data + AutoNSubstitute | Specific cases + auto-generated support |
| Need to verify interactions with specific dependencies | `[Frozen]` on key interfaces | Enables precise verification |
| Establishing test-wide conventions | `[AutoRegister]` customizations | Consistent setup across entire suite |
| Need more control over test setup | `FixtureFactory.Create()` | Full explicit arrangement when needed |
| Testing asynchronous operations | Combine with `WaitForCall()`/`AddTimeout()` | Reliable async testing without flaky delays |
| Working with legacy code | `InvokeProtectedMethod()` helpers | Access protected members safely |

### Example: Choosing the Right Combination

Consider testing a complex workflow service:

```csharp
public class WorkflowServiceTests
{
    // Use AutoRegister for suite-wide conventions
    // (Registered elsewhere in test project)
    
    [Theory]
    // Member data for specific workflow scenarios
    [MemberAutoNSubstituteData(nameof(WorkflowScenarios))]
    // Frozen dependencies we need to verify
    public async Task ExecuteWorkflowAsync_handles_various_scenarios(
        [Frozen] IStepExecutor stepExecutor,
        [Frozen] IWorkflowStatePersister statePersister,
        WorkflowService sut,
        WorkflowDefinition workflow,
        InitialState initialState,
        ExpectedFinalState expectedFinalState)
    {
        // Arrange - set up specific step outcomes
        foreach (var step in workflow.Steps)
        {
            stepExecutor.ExecuteStepAsync(Arg.Any<StepContext>())
                .Returns(Task.FromResult(step.ExpectedOutcome));
        }

        // Act
        var result = await sut.ExecuteWorkflowAsync(workflow, initialState);

        // Assert - verify final state and interactions
        result.State.Should().BeEquivalentTo(expectedFinalState.State);
        result.StepsCompleted.Should().Be(expectedFinalState.StepsCompleted);
        
        // Verify persistence happened
        await statePersister.Received(1).SaveAsync(
            Arg.Is<WorkflowState>(s => s.WorkflowId == workflow.Id));
    }
}
```

This test uses:
- `[MemberAutoNSubstituteData]` for specific workflow scenarios
- `[Frozen]` on dependencies we need to verify
- Auto-generated `WorkflowDefinition`, `InitialState`, and `ExpectedFinalState` objects
- Suite-wide `[AutoRegister]` customizations (elsewhere) for consistent mock setup

## Real-World Testing Patterns

Here are some patterns I've found particularly effective in production codebases.

### The Arrange-Act-Assert (AAA) with Atc.Test

Even with Atc.Test's automation, maintaining clear AAA structure improves readability:

```csharp
public class ReservationServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public void CreateReservationAsync_follows_aaa_pattern_clearly(
        [Frozen] ISeatAvailabilityChecker availabilityChecker,
        [Frozen] IPaymentProcessor paymentProcessor,
        [Frozen] INotificationService notificationService,
        ReservationService sut,
        ReservationRequest request,
        AvailableSeats availableSeats,
        PaymentResult paymentResult)
    {
        // ARRANGE - Set up preconditions and inputs
        availabilityChecker.CheckAvailabilityAsync(
                Arg.Any<ShowtimeId>(),
                Arg.Any<int>())
            .Returns(availableSeats);
            
        paymentProcessor.ProcessPaymentAsync(
                Arg.Any<PaymentDetails>())
            .Returns(paymentResult);

        // ACT - Execute the behavior under test
        var reservationTask = sut.CreateReservationAsync(request);

        // ASSERT - Verify outcomes
        var reservation = await reservationTask;
        reservation.Seats.Should().EquivalentTo(availableSeats.Seats);
        
        // Verify side effects
        await notificationService.Received(1).SendConfirmationAsync(
            Arg.Is<Reservation>(r => r.Id == reservation.Id));
            
        // Verify payment was processed if seats were available
        if (availableSeats.Seats.Any())
        {
            await paymentProcessor.Received(1).ProcessPaymentAsync(
                Arg.Is<PaymentDetails>(pd => 
                    pd.Amount == request.SeatCount * 15.00m));
        }
    }
}
```

### Testing Error Conditions and Edge Cases

Atc.Test makes it easy to test both happy paths and error conditions:

```csharp
public class AuthenticationServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task AuthenticateAsync_handles_successful_case(
        [Frozen] IUserRepository userRepository,
        [Frozen] IPasswordHasher passwordHasher,
        AuthenticationService sut,
        User user,
        LoginRequest loginRequest)
    {
        // Arrange - successful authentication scenario
        userRepository.GetByUsernameAsync(loginRequest.Username)
            .Returns(user);
            
        passwordHasher.VerifyPassword(
                loginRequest.Password,
                user.PasswordHash)
            .Returns(true);

        // Act
        var result = await sut.AuthenticateAsync(loginRequest);

        // Assert
        result.IsAuthenticated.Should().BeTrue();
        result.User.Should().BeSameAs(user);
    }

    [Theory]
    [InlineAutoNSubstituteData("nonexistent", "password")]
    [InlineAutoNSubstituteData("user", "wrongpassword")]
    public async Task AuthenticateAsync_handles_failure_cases(
        string username,
        string password,
        [Frozen] IUserRepository userRepository,
        [Frozen] IPasswordHasher passwordHasher,
        AuthenticationService sut)
    {
        // Arrange - failure scenarios
        userRepository.GetByUsernameAsync(username)
            .Returns((User)null); // User not found
            
        // For wrong password case, we still need a user to verify against
        if (username == "user")
        {
            var user = new User { Username = "user", PasswordHash = "hash" };
            userRepository.GetByUsernameAsync(username).Returns(user);
            passwordHasher.VerifyPassword(password, "hash").Returns(false);
        }

        // Act
        var result = await sut.AuthenticateAsync(new LoginRequest(username, password));

        // Assert
        result.IsAuthenticated.Should().BeFalse();
        result.User.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
```

## Conclusion

Atc.Test has evolved from a convenience library into a foundational testing tool that shapes how I approach .NET testing. Its real power comes not from individual features, but from how they combine to eliminate testing friction while maintaining test clarity and reliability.

The key to effective Atc.Test usage is understanding that it's not about removing all test code—it's about focusing your test code on what truly matters: the specific behaviors you're verifying and the conditions under which you verify them. By handling the repetitive mechanics of test setup, Atc.Test lets you spend more time thinking about test scenarios and less time wiring up mocks.

As your test suite grows, you'll find that the initial investment in learning Atc.Test's patterns pays dividends in:
- Reduced maintenance when constructors change
- More consistent test style across teams
- Faster test writing for common scenarios
- Clearer tests that express intent rather than setup
- Reliable asynchronous testing without flaky timing dependencies

Start with `[AutoNSubstituteData]` and `[Frozen]` in your next test, then gradually incorporate the other patterns as you encounter scenarios where they provide clear benefits. The goal isn't to use every feature in every test, but to have the right tool available when you need it.

Remember: the best tests are those that future-you (or a teammate) can understand at a glance. Atc.Test helps achieve that by removing the noise and letting the signal—the actual behavior being tested—shine through clearly.

The source code is available on GitHub at [https://github.com/atc-net/atc-test](https://github.com/atc-net/atc-test), and the package is available on NuGet at [https://www.nuget.org/packages/Atc.Test/](https://www.nuget.org/packages/Atc.Test/).