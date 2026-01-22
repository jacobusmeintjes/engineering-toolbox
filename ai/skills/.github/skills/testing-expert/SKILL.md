---
name: dotnet-testing-expert
description: Expert guidance for comprehensive .NET testing using xUnit, FluentAssertions, Testcontainers, SpecFlow/BDD, test patterns, mocking strategies, and integration testing best practices
---

# .NET Testing Expert

## When to use this skill

Use this skill when:
- Writing unit tests with xUnit
- Creating integration tests with Testcontainers
- Implementing BDD scenarios with SpecFlow
- Using FluentAssertions for readable assertions
- Mocking dependencies with NSubstitute/Moq
- Testing async code patterns
- Setting up test fixtures and data builders
- Implementing test doubles and test patterns

## Testing Philosophy

### Test Pyramid
```
         ╱╲
        ╱E2E╲         <- Few (Slow, Expensive)
       ╱──────╲
      ╱Integration╲   <- Some (Moderate)
     ╱────────────╲
    ╱  Unit Tests  ╲  <- Many (Fast, Cheap)
   ╱────────────────╲
```

**Guidelines:**
- 70% Unit tests
- 20% Integration tests  
- 10% E2E tests

## xUnit Fundamentals

### Basic Test Structure
```csharp
using Xunit;
using FluentAssertions;

namespace MyApp.Tests;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCustomer()
    {
        // Arrange
        var service = new CustomerService();
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com"
        };
        
        // Act
        var result = await service.CreateCustomerAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateCustomer_WithInvalidName_ThrowsException(string invalidName)
    {
        // Arrange
        var service = new CustomerService();
        var request = new CreateCustomerRequest { Name = invalidName };
        
        // Act
        Func<Task> act = async () => await service.CreateCustomerAsync(request);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*")
            .Where(ex => ex.ParamName == "Name");
    }
}
```

### Test Data Patterns

#### ClassData Pattern
```csharp
public class CustomerTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] 
        { 
            new CreateCustomerRequest { Name = "John", Email = "john@test.com" },
            true 
        };
        yield return new object[] 
        { 
            new CreateCustomerRequest { Name = "", Email = "invalid" },
            false 
        };
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(CustomerTestData))]
public async Task CreateCustomer_WithVariousInputs_ReturnsExpectedResult(
    CreateCustomerRequest request,
    bool shouldSucceed)
{
    // Test implementation
}
```

#### MemberData Pattern
```csharp
public class OrderTests
{
    public static IEnumerable<object[]> GetOrderTestData()
    {
        yield return new object[] { 100m, 10, 1000m };
        yield return new object[] { 50m, 5, 250m };
        yield return new object[] { 0m, 10, 0m };
    }
    
    [Theory]
    [MemberData(nameof(GetOrderTestData))]
    public void CalculateTotal_WithQuantityAndPrice_ReturnsCorrectTotal(
        decimal price,
        int quantity,
        decimal expectedTotal)
    {
        // Arrange
        var order = new Order();
        
        // Act
        var total = order.CalculateTotal(price, quantity);
        
        // Assert
        total.Should().Be(expectedTotal);
    }
}
```

### Test Fixtures and Shared Context

#### Class Fixture (Shared across test class)
```csharp
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext DbContext { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        
        DbContext = new ApplicationDbContext(options);
        SeedDatabase();
    }
    
    private void SeedDatabase()
    {
        DbContext.Customers.Add(new Customer { Id = "1", Name = "Test Customer" });
        DbContext.SaveChanges();
    }
    
    public void Dispose()
    {
        DbContext.Dispose();
    }
}

public class CustomerRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public CustomerRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetCustomer_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var repository = new CustomerRepository(_fixture.DbContext);
        
        // Act
        var customer = await repository.GetByIdAsync("1");
        
        // Assert
        customer.Should().NotBeNull();
        customer!.Name.Should().Be("Test Customer");
    }
}
```

#### Collection Fixture (Shared across multiple test classes)
```csharp
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition]
}

[Collection("Database collection")]
public class CustomerRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    
    public CustomerRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    // Tests...
}

[Collection("Database collection")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    
    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    // Tests...
}
```

## FluentAssertions Patterns

### Object Assertions
```csharp
[Fact]
public void Customer_Properties_ShouldBeSetCorrectly()
{
    // Arrange & Act
    var customer = new Customer
    {
        Id = "123",
        Name = "John Doe",
        Email = "john@example.com",
        CreatedAt = DateTime.UtcNow
    };
    
    // Assert
    customer.Should().NotBeNull();
    customer.Id.Should().Be("123");
    customer.Name.Should().Be("John Doe");
    customer.Email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    
    // Or use BeEquivalentTo for property-by-property comparison
    customer.Should().BeEquivalentTo(new
    {
        Id = "123",
        Name = "John Doe",
        Email = "john@example.com"
    }, options => options.Excluding(c => c.CreatedAt));
}
```

### Collection Assertions
```csharp
[Fact]
public async Task GetCustomers_WithMultipleRecords_ReturnsCorrectCollection()
{
    // Arrange
    var repository = CreateRepository();
    
    // Act
    var customers = await repository.GetAllAsync();
    
    // Assert
    customers.Should().NotBeEmpty()
        .And.HaveCount(3)
        .And.OnlyContain(c => !string.IsNullOrEmpty(c.Email))
        .And.Contain(c => c.Name == "John Doe");
    
    customers.Should().BeInAscendingOrder(c => c.Name);
    
    customers.Should().SatisfyRespectively(
        first => first.Name.Should().Be("Alice"),
        second => second.Name.Should().Be("Bob"),
        third => third.Name.Should().Be("Charlie")
    );
}
```

### Exception Assertions
```csharp
[Fact]
public async Task ProcessPayment_WithInsufficientFunds_ThrowsPaymentException()
{
    // Arrange
    var service = CreatePaymentService();
    var payment = new Payment { Amount = 1000m };
    
    // Act
    Func<Task> act = async () => await service.ProcessAsync(payment);
    
    // Assert
    await act.Should().ThrowAsync<PaymentException>()
        .WithMessage("Insufficient funds*")
        .Where(ex => ex.ErrorCode == PaymentErrorCode.InsufficientFunds)
        .Which.Amount.Should().Be(1000m);
}

[Fact]
public void ValidateOrder_WithValidOrder_ShouldNotThrow()
{
    // Arrange
    var validator = new OrderValidator();
    var order = CreateValidOrder();
    
    // Act
    Action act = () => validator.Validate(order);
    
    // Assert
    act.Should().NotThrow();
}
```

### Async Assertions
```csharp
[Fact]
public async Task GetCustomerAsync_WithValidId_ShouldCompleteQuickly()
{
    // Arrange
    var repository = CreateRepository();
    
    // Act
    Func<Task<Customer?>> act = async () => await repository.GetByIdAsync("123");
    
    // Assert
    await act.Should().CompleteWithinAsync(TimeSpan.FromMilliseconds(100));
    
    var result = await act();
    result.Should().NotBeNull();
}
```

## Testcontainers for Integration Tests

### Basic PostgreSQL Setup
```csharp
using Testcontainers.PostgreSql;
using Xunit;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private ApplicationDbContext _dbContext = null!;
    
    public DatabaseIntegrationTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
    
    [Fact]
    public async Task Repository_SaveAndRetrieve_WorksCorrectly()
    {
        // Arrange
        var repository = new CustomerRepository(_dbContext);
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Integration Test Customer",
            Email = "integration@test.com"
        };
        
        // Act
        await repository.AddAsync(customer);
        var retrieved = await repository.GetByIdAsync(customer.Id);
        
        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(customer);
    }
}
```

### Redis Integration Tests
```csharp
using Testcontainers.Redis;

public class CacheIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redis;
    private IConnectionMultiplexer _connection = null!;
    
    public CacheIntegrationTests()
    {
        _redis = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
    }
    
    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _redis.DisposeAsync();
    }
    
    [Fact]
    public async Task Cache_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var cache = new RedisCache(_connection);
        var key = "test:key";
        var value = new CustomerDto { Id = "123", Name = "Test" };
        
        // Act
        await cache.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var retrieved = await cache.GetAsync<CustomerDto>(key);
        
        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(value);
    }
}
```

### RabbitMQ Integration Tests
```csharp
using Testcontainers.RabbitMq;

public class MessagingIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitmq;
    private IConnection _connection = null!;
    
    public MessagingIntegrationTests()
    {
        _rabbitmq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _rabbitmq.StartAsync();
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_rabbitmq.GetConnectionString())
        };
        _connection = factory.CreateConnection();
    }
    
    public async Task DisposeAsync()
    {
        _connection.Dispose();
        await _rabbitmq.DisposeAsync();
    }
    
    [Fact]
    public async Task Publisher_PublishMessage_CanBeConsumed()
    {
        // Arrange
        using var channel = _connection.CreateModel();
        var queueName = "test.queue";
        channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: true);
        
        var receivedMessages = new List<string>();
        var consumerReady = new TaskCompletionSource<bool>();
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            receivedMessages.Add(message);
        };
        
        channel.BasicConsume(queueName, autoAck: true, consumer);
        consumerReady.SetResult(true);
        
        // Act
        await consumerReady.Task;
        var testMessage = "Hello, Integration Test!";
        var body = Encoding.UTF8.GetBytes(testMessage);
        channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        
        // Wait for message processing
        await Task.Delay(100);
        
        // Assert
        receivedMessages.Should().ContainSingle()
            .Which.Should().Be(testMessage);
    }
}
```

### Web API Integration Tests
```csharp
using Microsoft.AspNetCore.Mvc.Testing;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly PostgreSqlContainer _postgres;
    private HttpClient _client = null!;
    
    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace DbContext with test container connection
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgres.GetConnectionString());
                });
            });
        }).CreateClient();
    }
    
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _postgres.DisposeAsync();
    }
    
    [Fact]
    public async Task GetCustomers_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");
        
        // Assert
        response.Should().BeSuccessful();
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "test@example.com"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Name.Should().Be(request.Name);
    }
}
```

## BDD with SpecFlow

### Feature File
```gherkin
Feature: Customer Management
    As a customer service representative
    I want to manage customer information
    So that I can provide better service

Background:
    Given the system is running
    And the database is empty

Scenario: Create a new customer
    Given I have customer details:
        | Field | Value              |
        | Name  | John Doe          |
        | Email | john@example.com  |
    When I create the customer
    Then the customer should be created successfully
    And the customer should have an ID
    And the customer name should be "John Doe"

Scenario: Create customer with duplicate email
    Given a customer exists with email "john@example.com"
    When I try to create a customer with email "john@example.com"
    Then the creation should fail
    And I should receive a "DuplicateEmailException"

Scenario Outline: Validate customer email format
    Given I have a customer with email "<email>"
    When I validate the customer
    Then the validation should be "<result>"

    Examples:
        | email              | result  |
        | valid@example.com  | valid   |
        | invalid.email      | invalid |
        | @example.com       | invalid |
        | user@              | invalid |
```

### Step Definitions
```csharp
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;

[Binding]
public class CustomerManagementSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ICustomerService _customerService;
    private CreateCustomerRequest? _customerRequest;
    private Customer? _createdCustomer;
    private Exception? _caughtException;
    
    public CustomerManagementSteps(
        ScenarioContext scenarioContext,
        ICustomerService customerService)
    {
        _scenarioContext = scenarioContext;
        _customerService = customerService;
    }
    
    [Given(@"the database is empty")]
    public async Task GivenTheDatabaseIsEmpty()
    {
        await _customerService.DeleteAllCustomersAsync();
    }
    
    [Given(@"I have customer details:")]
    public void GivenIHaveCustomerDetails(Table table)
    {
        _customerRequest = table.CreateInstance<CreateCustomerRequest>();
    }
    
    [Given(@"a customer exists with email ""(.*)""")]
    public async Task GivenACustomerExistsWithEmail(string email)
    {
        await _customerService.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = "Existing Customer",
            Email = email
        });
    }
    
    [When(@"I create the customer")]
    public async Task WhenICreateTheCustomer()
    {
        try
        {
            _createdCustomer = await _customerService.CreateCustomerAsync(_customerRequest!);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }
    
    [When(@"I try to create a customer with email ""(.*)""")]
    public async Task WhenITryToCreateACustomerWithEmail(string email)
    {
        try
        {
            _customerRequest = new CreateCustomerRequest
            {
                Name = "Test Customer",
                Email = email
            };
            _createdCustomer = await _customerService.CreateCustomerAsync(_customerRequest);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }
    
    [Then(@"the customer should be created successfully")]
    public void ThenTheCustomerShouldBeCreatedSuccessfully()
    {
        _createdCustomer.Should().NotBeNull();
        _caughtException.Should().BeNull();
    }
    
    [Then(@"the customer should have an ID")]
    public void ThenTheCustomerShouldHaveAnID()
    {
        _createdCustomer!.Id.Should().NotBeNullOrEmpty();
    }
    
    [Then(@"the customer name should be ""(.*)""")]
    public void ThenTheCustomerNameShouldBe(string expectedName)
    {
        _createdCustomer!.Name.Should().Be(expectedName);
    }
    
    [Then(@"the creation should fail")]
    public void ThenTheCreationShouldFail()
    {
        _caughtException.Should().NotBeNull();
        _createdCustomer.Should().BeNull();
    }
    
    [Then(@"I should receive a ""(.*)""")]
    public void ThenIShouldReceiveA(string exceptionType)
    {
        _caughtException.Should().NotBeNull();
        _caughtException!.GetType().Name.Should().Be(exceptionType);
    }
}
```

### SpecFlow Hooks
```csharp
[Binding]
public class TestHooks
{
    private readonly IServiceProvider _serviceProvider;
    private PostgreSqlContainer? _postgres;
    
    public TestHooks()
    {
        // Setup will be done in BeforeTestRun
    }
    
    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        // Start containers once for all tests
        var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
        
        await postgres.StartAsync();
        
        // Store for later use
        TestContext.Current.Set("PostgresContainer", postgres);
    }
    
    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        // Setup for each scenario
        var postgres = TestContext.Current.Get<PostgreSqlContainer>("PostgresContainer");
        
        // Create fresh DbContext for scenario
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        
        var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        
        scenarioContext.ScenarioContainer.RegisterInstanceAs(dbContext);
    }
    
    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext)
    {
        // Cleanup after scenario
        var dbContext = scenarioContext.ScenarioContainer.Resolve<ApplicationDbContext>();
        await dbContext.DisposeAsync();
    }
    
    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        // Cleanup containers
        var postgres = TestContext.Current.Get<PostgreSqlContainer>("PostgresContainer");
        await postgres.DisposeAsync();
    }
}
```

## Test Data Builders

### Builder Pattern for Test Data
```csharp
public class CustomerBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _name = "Test Customer";
    private string _email = "test@example.com";
    private CustomerTier _tier = CustomerTier.Standard;
    private DateTime _createdAt = DateTime.UtcNow;
    
    public CustomerBuilder WithId(string id)
    {
        _id = id;
        return this;
    }
    
    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public CustomerBuilder WithPremiumTier()
    {
        _tier = CustomerTier.Premium;
        return this;
    }
    
    public CustomerBuilder CreatedDaysAgo(int days)
    {
        _createdAt = DateTime.UtcNow.AddDays(-days);
        return this;
    }
    
    public Customer Build()
    {
        return new Customer
        {
            Id = _id,
            Name = _name,
            Email = _email,
            Tier = _tier,
            CreatedAt = _createdAt
        };
    }
    
    public static implicit operator Customer(CustomerBuilder builder) => builder.Build();
}

// Usage in tests
[Fact]
public async Task ProcessOrder_ForPremiumCustomer_AppliesDiscount()
{
    // Arrange
    var customer = new CustomerBuilder()
        .WithName("Premium Customer")
        .WithPremiumTier()
        .CreatedDaysAgo(365)
        .Build();
    
    var service = CreateOrderService();
    
    // Act
    var order = await service.CreateOrderAsync(customer, items);
    
    // Assert
    order.DiscountApplied.Should().BeTrue();
}
```

## Mocking with NSubstitute

### Basic Mocking
```csharp
[Fact]
public async Task OrderService_CallsRepositoryCorrectly()
{
    // Arrange
    var repository = Substitute.For<IOrderRepository>();
    var service = new OrderService(repository);
    
    var order = new Order { Id = "123" };
    repository.GetByIdAsync("123").Returns(order);
    
    // Act
    var result = await service.GetOrderAsync("123");
    
    // Assert
    result.Should().Be(order);
    await repository.Received(1).GetByIdAsync("123");
}
```

### Argument Matching
```csharp
[Fact]
public async Task EmailService_SendsEmailWithCorrectRecipient()
{
    // Arrange
    var emailService = Substitute.For<IEmailService>();
    var notificationService = new NotificationService(emailService);
    
    // Act
    await notificationService.NotifyCustomerAsync("customer@example.com", "Hello");
    
    // Assert
    await emailService.Received(1).SendAsync(
        Arg.Is<string>(email => email == "customer@example.com"),
        Arg.Any<string>(),
        Arg.Any<string>());
}
```

### Throwing Exceptions
```csharp
[Fact]
public async Task Service_HandlesRepositoryException()
{
    // Arrange
    var repository = Substitute.For<ICustomerRepository>();
    repository.GetByIdAsync(Arg.Any<string>())
        .ThrowsAsync(new DatabaseException("Connection failed"));
    
    var service = new CustomerService(repository);
    
    // Act
    Func<Task> act = async () => await service.GetCustomerAsync("123");
    
    // Assert
    await act.Should().ThrowAsync<ServiceException>()
        .WithInnerException<DatabaseException>();
}
```

## Testing Async Patterns

### Testing with CancellationToken
```csharp
[Fact]
public async Task LongRunningOperation_CancelsCorrectly()
{
    // Arrange
    var service = new DataProcessingService();
    var cts = new CancellationTokenSource();
    
    // Act
    var task = service.ProcessDataAsync(cts.Token);
    cts.Cancel();
    
    // Assert
    await task.Should().ThrowAsync<OperationCanceledException>();
}
```

### Testing with Timeouts
```csharp
[Fact]
public async Task SlowOperation_CompletesWithinTimeout()
{
    // Arrange
    var service = new ExternalApiService();
    
    // Act
    Func<Task> act = async () =>
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await service.FetchDataAsync(cts.Token);
    };
    
    // Assert
    await act.Should().NotThrowAsync();
}
```

## Best Practices

### 1. Test Naming
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task CreateOrder_WithInvalidCustomer_ThrowsValidationException()

// Or: Given_When_Then
[Fact]
public async Task GivenInvalidCustomer_WhenCreatingOrder_ThenThrowsValidationException()
```

### 2. One Assert Per Test (when possible)
```csharp
// Prefer
[Fact]
public void Customer_ShouldHaveValidEmail()
{
    var customer = new Customer { Email = "test@example.com" };
    customer.Email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
}

// Over
[Fact]
public void Customer_ShouldBeValid()
{
    var customer = new Customer { Name = "Test", Email = "test@example.com" };
    customer.Name.Should().NotBeEmpty();
    customer.Email.Should().MatchRegex(...);
    customer.Id.Should().NotBeEmpty();
}
```

### 3. Avoid Testing Implementation Details
```csharp
// Bad: Testing internal implementation
[Fact]
public void OrderCalculator_UsesCorrectFormula()
{
    var calculator = new OrderCalculator();
    calculator.Formula.Should().Be("price * quantity * (1 - discount)");
}

// Good: Testing behavior
[Fact]
public void OrderCalculator_CalculatesTotalWithDiscount_ReturnsCorrectAmount()
{
    var calculator = new OrderCalculator();
    var result = calculator.Calculate(price: 100, quantity: 2, discount: 0.1m);
    result.Should().Be(180m);
}
```

### 4. Test Isolation
```csharp
// Each test should be independent
public class OrderTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    
    public async Task InitializeAsync()
    {
        // Fresh database for each test
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new ApplicationDbContext(options);
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [SpecFlow Documentation](https://docs.specflow.org/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
