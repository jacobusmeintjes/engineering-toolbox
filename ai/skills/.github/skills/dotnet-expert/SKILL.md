---
name: dotnet-expert-engineer
description: Expert guidance for modern .NET development including C# best practices, ASP.NET Core, dependency injection, async patterns, nullable reference types, and enterprise architecture patterns
---

# .NET Expert Software Engineer

## When to use this skill

Use this skill when:
- Writing or refactoring C# code
- Building ASP.NET Core applications (Web APIs, Blazor, minimal APIs)
- Implementing dependency injection patterns
- Working with async/await patterns
- Designing microservices architecture
- Implementing domain-driven design (DDD)
- Using Entity Framework Core or Dapper
- Working with .NET 8+ features

## Core Principles

### 1. Modern C# Practices

**Always use nullable reference types**:
```csharp
#nullable enable

public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository repository,
        ILogger<CustomerService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Customer?> GetCustomerAsync(string customerId, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        
        return await _repository.GetByIdAsync(customerId, ct);
    }
}
```

**Use modern pattern matching**:
```csharp
public decimal CalculateDiscount(Customer customer) => customer switch
{
    { Tier: CustomerTier.Premium, YearsActive: > 5 } => 0.25m,
    { Tier: CustomerTier.Premium } => 0.15m,
    { Tier: CustomerTier.Standard, YearsActive: > 3 } => 0.10m,
    { Tier: CustomerTier.Standard } => 0.05m,
    _ => 0m
};
```

**Prefer primary constructors (.NET 8+)**:
```csharp
public class OrderProcessor(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    ILogger<OrderProcessor> logger)
{
    public async Task<OrderResult> ProcessOrderAsync(Order order, CancellationToken ct)
    {
        logger.LogInformation("Processing order {OrderId}", order.Id);
        // Use injected dependencies directly
        return await orderRepository.SaveAsync(order, ct);
    }
}
```

### 2. Async/Await Best Practices

**Always use ConfigureAwait(false) in library code**:
```csharp
// In application code (ASP.NET Core)
public async Task<IActionResult> GetCustomer(string id)
{
    var customer = await _service.GetCustomerAsync(id); // No ConfigureAwait needed
    return Ok(customer);
}

// In library/infrastructure code
public async Task<Customer?> GetCustomerAsync(string id, CancellationToken ct)
{
    var result = await _dbContext.Customers
        .FirstOrDefaultAsync(c => c.Id == id, ct)
        .ConfigureAwait(false); // Important in libraries
    
    return result;
}
```

**Use ValueTask for hot paths**:
```csharp
public ValueTask<CachedData?> GetFromCacheAsync(string key)
{
    if (_memoryCache.TryGetValue(key, out CachedData? cached))
    {
        return ValueTask.FromResult(cached); // Synchronous completion, no allocation
    }
    
    return GetFromDatabaseAsync(key); // Async path
}
```

**Proper cancellation token handling**:
```csharp
public async Task<ProcessResult> ProcessWithTimeoutAsync(
    string data,
    CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(30));
    
    try
    {
        return await DoWorkAsync(data, cts.Token);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        throw new TimeoutException("Operation timed out after 30 seconds");
    }
}
```

### 3. Dependency Injection Patterns

**Service registration patterns**:
```csharp
// Startup/Program.cs
builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Options pattern
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection("OrderProcessing"));

// Factory pattern
builder.Services.AddSingleton<IOrderProcessorFactory, OrderProcessorFactory>();

// Keyed services (.NET 8+)
builder.Services.AddKeyedSingleton<IPaymentProcessor, StripePaymentProcessor>("stripe");
builder.Services.AddKeyedSingleton<IPaymentProcessor, PayPalPaymentProcessor>("paypal");
```

**Keyed service usage**:
```csharp
public class PaymentService(
    [FromKeyedServices("stripe")] IPaymentProcessor stripeProcessor,
    [FromKeyedServices("paypal")] IPaymentProcessor paypalProcessor)
{
    public async Task<PaymentResult> ProcessAsync(Payment payment)
    {
        var processor = payment.Provider switch
        {
            PaymentProvider.Stripe => stripeProcessor,
            PaymentProvider.PayPal => paypalProcessor,
            _ => throw new ArgumentException("Invalid provider")
        };
        
        return await processor.ProcessAsync(payment);
    }
}
```

### 4. ASP.NET Core Patterns

**Minimal APIs with proper validation**:
```csharp
var app = builder.Build();

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IOrderService orderService,
    IValidator<CreateOrderRequest> validator,
    CancellationToken ct) =>
{
    var validationResult = await validator.ValidateAsync(request, ct);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    
    var order = await orderService.CreateOrderAsync(request, ct);
    return Results.Created($"/api/orders/{order.Id}", order);
})
.WithName("CreateOrder")
.WithOpenApi()
.RequireAuthorization()
.Produces<Order>(StatusCodes.Status201Created)
.ProducesValidationProblem();
```

**Controller-based APIs with ActionResults**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType<Customer>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> GetCustomer(
        string id,
        CancellationToken ct)
    {
        var customer = await _customerService.GetCustomerAsync(id, ct);
        
        return customer is null
            ? NotFound()
            : Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType<Customer>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Customer>> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken ct)
    {
        var customer = await _customerService.CreateCustomerAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            customer);
    }
}
```

### 5. Entity Framework Core Best Practices

**DbContext configuration**:
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

// Separate entity configuration
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(c => c.Email)
            .IsUnique();
        
        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Efficient queries**:
```csharp
// Good: Use AsNoTracking for read-only queries
public async Task<List<CustomerDto>> GetCustomersAsync(CancellationToken ct)
{
    return await _context.Customers
        .AsNoTracking()
        .Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email
        })
        .ToListAsync(ct);
}

// Good: Use Include/ThenInclude for related data
public async Task<Order?> GetOrderWithDetailsAsync(string orderId, CancellationToken ct)
{
    return await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
        .FirstOrDefaultAsync(o => o.Id == orderId, ct);
}

// Good: Use AsSplitQuery for multiple collections
public async Task<Customer?> GetCustomerWithDataAsync(string customerId, CancellationToken ct)
{
    return await _context.Customers
        .AsSplitQuery()
        .Include(c => c.Orders)
        .Include(c => c.Addresses)
        .FirstOrDefaultAsync(c => c.Id == customerId, ct);
}
```

### 6. Error Handling and Resilience

**Global exception handling**:
```csharp
// Middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        
        var problemDetails = new ValidationProblemDetails(ex.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred"
        };
        
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

**Result pattern for business logic**:
```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct);
    if (customer is null)
    {
        return Result<Order>.Failure("Customer not found");
    }
    
    if (!customer.IsActive)
    {
        return Result<Order>.Failure("Customer account is inactive");
    }
    
    var order = Order.Create(customer, request.Items);
    await _orderRepository.SaveAsync(order, ct);
    
    return Result<Order>.Success(order);
}
```

### 7. Configuration and Options Pattern

**Strongly-typed configuration**:
```csharp
public class OrderProcessingOptions
{
    public const string SectionName = "OrderProcessing";
    
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; set; } = 100;
    public bool EnableNotifications { get; set; } = true;
}

// Registration
builder.Services.Configure<OrderProcessingOptions>(
    builder.Configuration.GetSection(OrderProcessingOptions.SectionName));

builder.Services.AddOptionsWithValidateOnStart<OrderProcessingOptions>()
    .Validate(options => options.MaxRetries > 0, "MaxRetries must be positive")
    .Validate(options => options.BatchSize > 0, "BatchSize must be positive");

// Usage
public class OrderProcessor
{
    private readonly OrderProcessingOptions _options;
    
    public OrderProcessor(IOptions<OrderProcessingOptions> options)
    {
        _options = options.Value;
    }
}
```

### 8. Background Services and Hosted Services

**Proper background service pattern**:
```csharp
public class OrderProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public OrderProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OrderProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Processing Background Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing orders");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Order Processing Background Service stopping");
    }

    private async Task ProcessPendingOrdersAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
        
        await orderService.ProcessPendingOrdersAsync(ct);
    }
}
```

## Advanced Patterns

### Source Generators
```csharp
// Use source generators for repetitive code
[JsonSerializable(typeof(Customer))]
[JsonSerializable(typeof(Order))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

// Usage
var json = JsonSerializer.Serialize(order, AppJsonSerializerContext.Default.Order);
```

### Interceptors (.NET 8+)
```csharp
// For high-performance scenarios, use interceptors to modify method calls at compile time
[InterceptsLocation("Program.cs", line: 42, column: 5)]
public static class LoggingInterceptors
{
    public static void LogInformation(this ILogger logger, string message)
    {
        // Optimized logging implementation
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.Log(LogLevel.Information, message);
        }
    }
}
```

### Channels for Producer/Consumer Patterns
```csharp
public class MessageProcessor
{
    private readonly Channel<Message> _channel;
    
    public MessageProcessor()
    {
        _channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }
    
    public async ValueTask EnqueueAsync(Message message, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(message, ct);
    }
    
    public async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(ct))
        {
            await ProcessMessageAsync(message, ct);
        }
    }
}
```

## Common Anti-Patterns to Avoid

### ❌ Don't: Blocking on async code
```csharp
// BAD
var result = SomeAsyncMethod().Result; // Deadlock risk
var result = SomeAsyncMethod().GetAwaiter().GetResult(); // Still blocks

// GOOD
var result = await SomeAsyncMethod();
```

### ❌ Don't: Async void methods (except event handlers)
```csharp
// BAD
public async void ProcessOrder(Order order) // Can't catch exceptions
{
    await _service.ProcessAsync(order);
}

// GOOD
public async Task ProcessOrderAsync(Order order)
{
    await _service.ProcessAsync(order);
}
```

### ❌ Don't: Over-use Task.Run
```csharp
// BAD - Unnecessary thread pool usage in ASP.NET Core
public async Task<IActionResult> Get()
{
    var data = await Task.Run(() => _service.GetData()); // Don't do this
    return Ok(data);
}

// GOOD - Let ASP.NET Core manage threads
public async Task<IActionResult> Get()
{
    var data = await _service.GetDataAsync(); // Service is already async
    return Ok(data);
}
```

### ❌ Don't: Use string concatenation for SQL
```csharp
// BAD - SQL injection risk
var sql = $"SELECT * FROM Users WHERE Id = {userId}";

// GOOD - Use parameters
var user = await _context.Users
    .FromSqlInterpolated($"SELECT * FROM Users WHERE Id = {userId}")
    .FirstOrDefaultAsync();
```

## Testing Considerations

When writing production code, always consider:
- **Testability**: Use interfaces and dependency injection
- **Separation of concerns**: Keep business logic separate from infrastructure
- **SOLID principles**: Single responsibility, dependency inversion
- **Domain-driven design**: Rich domain models, not anemic entities

## Resources

- [Microsoft .NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [.NET Performance Tips](https://learn.microsoft.com/en-us/dotnet/core/runtime/performance-tips)
