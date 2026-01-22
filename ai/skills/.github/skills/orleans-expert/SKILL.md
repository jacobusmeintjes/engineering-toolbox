---
name: orleans-distributed-systems
description: Expert guidance for building distributed systems with Microsoft Orleans, including grain design, virtual actors, state management, streaming, clustering, and production deployment patterns
---

# Orleans Distributed Systems Expert

## When to use this skill

Use this skill when:
- Designing Orleans grain architectures
- Implementing virtual actor patterns
- Working with grain state and persistence
- Implementing Orleans streaming
- Setting up Orleans clusters
- Troubleshooting Orleans applications
- Optimizing Orleans performance
- Deploying Orleans to production (Azure, AWS, Kubernetes)

## Core Orleans Concepts

### Virtual Actor Model
- **Grains** are the building blocks - virtual actors that exist when called
- **Automatic activation**: Grains activate on first call, deactivate when idle
- **Location transparency**: Call any grain without knowing its physical location
- **Single-threaded execution**: Orleans guarantees turn-based concurrency
- **Distributed method calls**: Grain methods are async and can be called across nodes

## Grain Design Patterns

### 1. Basic Grain Structure

```csharp
public interface ICustomerGrain : IGrainWithStringKey
{
    Task<CustomerState> GetStateAsync();
    Task UpdateEmailAsync(string email);
    Task<bool> TryPlaceOrderAsync(OrderRequest order);
}

[GenerateSerializer]
public class CustomerState
{
    [Id(0)] public string CustomerId { get; set; } = string.Empty;
    [Id(1)] public string Email { get; set; } = string.Empty;
    [Id(2)] public CustomerTier Tier { get; set; }
    [Id(3)] public decimal TotalSpent { get; set; }
    [Id(4)] public int OrderCount { get; set; }
}

public class CustomerGrain : Grain, ICustomerGrain
{
    private readonly IPersistentState<CustomerState> _state;
    private readonly ILogger<CustomerGrain> _logger;

    public CustomerGrain(
        [PersistentState("customer", "customerStore")] IPersistentState<CustomerState> state,
        ILogger<CustomerGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating customer grain {CustomerId}", this.GetPrimaryKeyString());
        
        // Initialize state if new
        if (string.IsNullOrEmpty(_state.State.CustomerId))
        {
            _state.State.CustomerId = this.GetPrimaryKeyString();
        }
        
        return base.OnActivateAsync(cancellationToken);
    }

    public Task<CustomerState> GetStateAsync()
    {
        return Task.FromResult(_state.State);
    }

    public async Task UpdateEmailAsync(string email)
    {
        _state.State.Email = email;
        await _state.WriteStateAsync();
    }

    public async Task<bool> TryPlaceOrderAsync(OrderRequest order)
    {
        // Business logic here
        _state.State.OrderCount++;
        _state.State.TotalSpent += order.Total;
        
        // Update tier based on spending
        if (_state.State.TotalSpent > 10000)
        {
            _state.State.Tier = CustomerTier.Premium;
        }
        
        await _state.WriteStateAsync();
        
        // Notify order grain
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(order.OrderId);
        await orderGrain.CreateAsync(order);
        
        return true;
    }
}
```

### 2. Stateless Worker Grains

For CPU-intensive operations or stateless services:

```csharp
[StatelessWorker]
[Reentrant] // Allow concurrent calls
public class PriceCalculationGrain : Grain, IPriceCalculationGrain
{
    private readonly IPricingService _pricingService;
    
    public PriceCalculationGrain(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }
    
    public async Task<decimal> CalculatePriceAsync(PriceRequest request)
    {
        // This grain can have multiple instances across the cluster
        // Orleans manages pooling and distribution
        return await _pricingService.CalculateAsync(request);
    }
}
```

### 3. Grain State Management

```csharp
// Multiple state persistence
public class OrderGrain : Grain, IOrderGrain
{
    private readonly IPersistentState<OrderState> _orderState;
    private readonly IPersistentState<OrderHistory> _historyState;
    
    public OrderGrain(
        [PersistentState("order", "orderStore")] IPersistentState<OrderState> orderState,
        [PersistentState("history", "historyStore")] IPersistentState<OrderHistory> historyState)
    {
        _orderState = orderState;
        _historyState = historyState;
    }
    
    public async Task UpdateStatusAsync(OrderStatus status)
    {
        var oldStatus = _orderState.State.Status;
        _orderState.State.Status = status;
        
        // Update history
        _historyState.State.Events.Add(new StatusChangeEvent
        {
            Timestamp = DateTime.UtcNow,
            OldStatus = oldStatus,
            NewStatus = status
        });
        
        // Write both states
        await Task.WhenAll(
            _orderState.WriteStateAsync(),
            _historyState.WriteStateAsync());
    }
}
```

### 4. Grain Timers and Reminders

```csharp
public class SubscriptionGrain : Grain, ISubscriptionGrain, IRemindable
{
    private readonly IPersistentState<SubscriptionState> _state;
    private IGrainTimer? _healthCheckTimer;
    
    public SubscriptionGrain(
        [PersistentState("subscription")] IPersistentState<SubscriptionState> state)
    {
        _state = state;
    }
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // Timer: Cancelled when grain deactivates, good for caching/monitoring
        _healthCheckTimer = RegisterGrainTimer(
            callback: CheckHealthAsync,
            state: null,
            dueTime: TimeSpan.FromMinutes(5),
            period: TimeSpan.FromMinutes(5));
        
        // Reminder: Persisted, survives grain deactivation, good for workflows
        await RegisterOrUpdateReminder(
            reminderName: "ProcessSubscription",
            dueTime: TimeSpan.FromHours(1),
            period: TimeSpan.FromDays(30));
        
        await base.OnActivateAsync(cancellationToken);
    }
    
    private async Task CheckHealthAsync(object state)
    {
        // Regular health check logic
        _logger.LogDebug("Health check for subscription {Id}", this.GetPrimaryKeyString());
        await Task.CompletedTask;
    }
    
    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == "ProcessSubscription")
        {
            // Process monthly subscription
            await ProcessMonthlyBillingAsync();
        }
    }
    
    private async Task ProcessMonthlyBillingAsync()
    {
        _logger.LogInformation("Processing subscription billing");
        
        var paymentGrain = GrainFactory.GetGrain<IPaymentGrain>(_state.State.CustomerId);
        await paymentGrain.ProcessSubscriptionPaymentAsync(_state.State.Amount);
        
        _state.State.LastBilledDate = DateTime.UtcNow;
        await _state.WriteStateAsync();
    }
}
```

## Orleans Streaming

### 1. Stream Producer

```csharp
public class OrderGrain : Grain, IOrderGrain
{
    private IAsyncStream<OrderEvent>? _orderStream;
    
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("OrderStreamProvider");
        _orderStream = streamProvider.GetStream<OrderEvent>(
            streamId: StreamId.Create("orders", this.GetPrimaryKeyString()));
        
        return base.OnActivateAsync(cancellationToken);
    }
    
    public async Task PlaceOrderAsync(OrderRequest request)
    {
        // Process order...
        var orderId = Guid.NewGuid().ToString();
        
        // Publish to stream
        await _orderStream!.OnNextAsync(new OrderEvent
        {
            OrderId = orderId,
            CustomerId = this.GetPrimaryKeyString(),
            Type = OrderEventType.Created,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### 2. Stream Consumer

```csharp
public class OrderAnalyticsGrain : Grain, IOrderAnalyticsGrain
{
    private StreamSubscriptionHandle<OrderEvent>? _subscription;
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("OrderStreamProvider");
        var stream = streamProvider.GetStream<OrderEvent>(
            StreamId.Create("orders", "all"));
        
        // Subscribe with explicit grain reference
        _subscription = await stream.SubscribeAsync(
            onNext: OnOrderEventAsync,
            onError: ex => _logger.LogError(ex, "Stream error"),
            onCompleted: () => _logger.LogInformation("Stream completed"));
        
        await base.OnActivateAsync(cancellationToken);
    }
    
    private async Task OnOrderEventAsync(OrderEvent orderEvent, StreamSequenceToken token)
    {
        _logger.LogInformation("Received order event: {EventType} for {OrderId}", 
            orderEvent.Type, orderEvent.OrderId);
        
        // Process event
        await UpdateAnalyticsAsync(orderEvent);
    }
    
    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (_subscription != null)
        {
            await _subscription.UnsubscribeAsync();
        }
        
        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}
```

### 3. Implicit Stream Subscriptions

```csharp
[ImplicitStreamSubscription("notifications")]
public class NotificationGrain : Grain, INotificationGrain, IStreamSubscriptionObserver
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("NotificationStreamProvider");
        var streams = await streamProvider.GetAllSubscriptions<NotificationEvent>();
        
        foreach (var subscription in streams)
        {
            await subscription.ResumeAsync(OnNotificationAsync);
        }
        
        await base.OnActivateAsync(cancellationToken);
    }
    
    private Task OnNotificationAsync(NotificationEvent notification, StreamSequenceToken token)
    {
        // Handle notification
        return Task.CompletedTask;
    }
    
    public Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
    {
        var handle = handleFactory.Create<NotificationEvent>();
        return handle.ResumeAsync(OnNotificationAsync);
    }
}
```

## Cluster Configuration

### 1. Silo Host Configuration (.NET 8+)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, siloBuilder) =>
{
    siloBuilder
        // Clustering
        .UseAdoNetClustering(options =>
        {
            options.ConnectionString = context.Configuration.GetConnectionString("OrleansDB");
            options.Invariant = "System.Data.SqlClient";
        })
        // Or use Azure Table Storage
        .UseAzureStorageClustering(options =>
        {
            options.ConfigureTableServiceClient(
                context.Configuration.GetConnectionString("AzureStorage"));
        })
        
        // Grain Storage
        .AddAdoNetGrainStorage("orderStore", options =>
        {
            options.ConnectionString = context.Configuration.GetConnectionString("OrleansDB");
            options.Invariant = "System.Data.SqlClient";
            options.UseJsonFormat = true;
        })
        
        // Streaming
        .AddMemoryStreams("OrderStreamProvider")
        .AddMemoryGrainStorage("PubSubStore")
        
        // Or Azure Event Hubs
        .AddEventHubStreams("EventHubStreamProvider", configurator =>
        {
            configurator.ConfigureEventHub(builder => builder.Configure(options =>
            {
                options.ConnectionString = context.Configuration["EventHub:ConnectionString"];
                options.ConsumerGroup = "orleans-consumer";
                options.EventHubName = "orders";
            }));
            configurator.UseAzureTableCheckpointer(builder => builder.Configure(options =>
            {
                options.ConfigureTableServiceClient(
                    context.Configuration["AzureStorage:ConnectionString"]);
            }));
        })
        
        // Dashboard (development)
        .UseDashboard(options =>
        {
            options.Port = 8080;
            options.BasePath = "/dashboard";
        })
        
        // Configure endpoints
        .ConfigureEndpoints(
            siloPort: 11111,
            gatewayPort: 30000,
            advertisedIP: IPAddress.Parse("10.0.0.4"),
            listenOnAnyHostAddress: true)
        
        // Application parts
        .ConfigureApplicationParts(parts =>
        {
            parts.AddApplicationPart(typeof(CustomerGrain).Assembly)
                 .WithReferences();
        });
});

var app = builder.Build();
app.Run();
```

### 2. Client Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient((context, clientBuilder) =>
{
    clientBuilder
        .UseAdoNetClustering(options =>
        {
            options.ConnectionString = context.Configuration.GetConnectionString("OrleansDB");
            options.Invariant = "System.Data.SqlClient";
        })
        .ConfigureApplicationParts(parts =>
        {
            parts.AddApplicationPart(typeof(ICustomerGrain).Assembly);
        });
});

// Or for standalone clients
var client = new ClientBuilder()
    .UseAdoNetClustering(options =>
    {
        options.ConnectionString = connectionString;
        options.Invariant = "System.Data.SqlClient";
    })
    .ConfigureApplicationParts(parts =>
    {
        parts.AddApplicationPart(typeof(ICustomerGrain).Assembly);
    })
    .Build();

await client.Connect();
```

## Advanced Patterns

### 1. Grain Observers (Callbacks)

```csharp
// Observer interface
public interface IOrderObserver : IGrainObserver
{
    void OnOrderStatusChanged(string orderId, OrderStatus newStatus);
}

// Grain that accepts observers
public interface IOrderGrain : IGrainWithStringKey
{
    Task SubscribeAsync(IOrderObserver observer);
    Task UnsubscribeAsync(IOrderObserver observer);
    Task UpdateStatusAsync(OrderStatus status);
}

public class OrderGrain : Grain, IOrderGrain
{
    private readonly ObserverManager<IOrderObserver> _observers;
    
    public OrderGrain()
    {
        _observers = new ObserverManager<IOrderObserver>(
            TimeSpan.FromMinutes(5),
            _logger);
    }
    
    public Task SubscribeAsync(IOrderObserver observer)
    {
        _observers.Subscribe(observer, observer);
        return Task.CompletedTask;
    }
    
    public Task UnsubscribeAsync(IOrderObserver observer)
    {
        _observers.Unsubscribe(observer);
        return Task.CompletedTask;
    }
    
    public async Task UpdateStatusAsync(OrderStatus status)
    {
        _state.State.Status = status;
        await _state.WriteStateAsync();
        
        // Notify all observers
        _observers.Notify(observer => 
            observer.OnOrderStatusChanged(this.GetPrimaryKeyString(), status));
    }
}

// Client-side observer
public class OrderMonitor : IOrderObserver
{
    public void OnOrderStatusChanged(string orderId, OrderStatus newStatus)
    {
        Console.WriteLine($"Order {orderId} status changed to {newStatus}");
    }
}

// Usage
var orderGrain = client.GetGrain<IOrderGrain>("order-123");
var observer = await client.CreateObjectReference<IOrderObserver>(new OrderMonitor());
await orderGrain.SubscribeAsync(observer);
```

### 2. Grain Extensions

```csharp
public interface IOrderStatistics : IGrainExtension
{
    Task<OrderStats> GetStatisticsAsync();
}

public class OrderStatisticsExtension : IOrderStatistics
{
    private readonly OrderGrain _grain;
    
    public OrderStatisticsExtension(OrderGrain grain)
    {
        _grain = grain;
    }
    
    public async Task<OrderStats> GetStatisticsAsync()
    {
        // Access grain's private state
        return new OrderStats
        {
            TotalOrders = _grain.OrderCount,
            AverageValue = _grain.AverageOrderValue
        };
    }
}

// In the grain
public class OrderGrain : Grain, IOrderGrain
{
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetOrSetExtension<IOrderStatistics, OrderStatisticsExtension>(
            () => new OrderStatisticsExtension(this));
        
        return base.OnActivateAsync(cancellationToken);
    }
}

// Usage
var orderGrain = client.GetGrain<IOrderGrain>("order-123");
var stats = await orderGrain.AsReference<IOrderStatistics>().GetStatisticsAsync();
```

### 3. Grain Versioning and Updates

```csharp
[Version(1)]
public interface ICustomerGrainV1 : IGrainWithStringKey
{
    Task<string> GetNameAsync();
}

[Version(2)]
public interface ICustomerGrainV2 : IGrainWithStringKey
{
    Task<CustomerInfo> GetInfoAsync(); // New method signature
}

public class CustomerGrain : Grain, ICustomerGrainV1, ICustomerGrainV2
{
    // Support both versions
    public Task<string> GetNameAsync()
    {
        return Task.FromResult(_state.State.Name);
    }
    
    public Task<CustomerInfo> GetInfoAsync()
    {
        return Task.FromResult(new CustomerInfo
        {
            Name = _state.State.Name,
            Email = _state.State.Email,
            Tier = _state.State.Tier
        });
    }
}
```

## Production Patterns

### 1. Health Checks

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<OrleansHealthCheck>("orleans");

public class OrleansHealthCheck : IHealthCheck
{
    private readonly IGrainFactory _grainFactory;
    
    public OrleansHealthCheck(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var grain = _grainFactory.GetGrain<IHealthCheckGrain>(Guid.Empty);
            await grain.PingAsync();
            return HealthCheckResult.Healthy("Orleans cluster is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Orleans cluster is not responsive", ex);
        }
    }
}
```

### 2. Graceful Shutdown

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Host.UseOrleans((context, siloBuilder) =>
        {
            // Configuration...
        });
        
        var app = builder.Build();
        
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        
        lifetime.ApplicationStopping.Register(() =>
        {
            // Stop accepting new requests
            Console.WriteLine("Stopping Orleans silo...");
        });
        
        await app.RunAsync();
    }
}
```

### 3. Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: orleans-silo
spec:
  serviceName: orleans-silo
  replicas: 3
  selector:
    matchLabels:
      app: orleans-silo
  template:
    metadata:
      labels:
        app: orleans-silo
    spec:
      containers:
      - name: orleans-silo
        image: myapp/orleans-silo:latest
        ports:
        - containerPort: 11111
          name: orleans-silo
        - containerPort: 30000
          name: orleans-gateway
        env:
        - name: ORLEANS_SERVICE_ID
          value: "MyOrleansApp"
        - name: ORLEANS_CLUSTER_ID
          value: "Production"
        - name: ORLEAN_DB_CONNECTION
          valueFrom:
            secretKeyRef:
              name: orleans-secrets
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 60
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: orleans-gateway
spec:
  type: LoadBalancer
  ports:
  - port: 30000
    targetPort: 30000
    name: gateway
  selector:
    app: orleans-silo
```

## Common Patterns for Financial Services

### 1. Event Sourcing with Orleans

```csharp
public class AccountGrain : Grain<AccountState>, IAccountGrain
{
    private readonly List<AccountEvent> _uncommittedEvents = new();
    
    public async Task<bool> DebitAsync(decimal amount, string reference)
    {
        if (State.Balance < amount)
        {
            return false;
        }
        
        var evt = new AccountDebitedEvent
        {
            Amount = amount,
            Reference = reference,
            Timestamp = DateTime.UtcNow
        };
        
        Apply(evt);
        _uncommittedEvents.Add(evt);
        
        await CommitEventsAsync();
        return true;
    }
    
    private void Apply(AccountEvent evt)
    {
        switch (evt)
        {
            case AccountDebitedEvent debit:
                State.Balance -= debit.Amount;
                State.Version++;
                break;
            // Other event types...
        }
    }
    
    private async Task CommitEventsAsync()
    {
        // Save events to event store
        foreach (var evt in _uncommittedEvents)
        {
            await _eventStore.AppendAsync(this.GetPrimaryKeyString(), evt);
        }
        
        // Save snapshot
        await WriteStateAsync();
        _uncommittedEvents.Clear();
    }
}
```

### 2. Saga Pattern for Distributed Transactions

```csharp
public class OrderSagaGrain : Grain, IOrderSagaGrain
{
    private readonly IPersistentState<SagaState> _state;
    
    public async Task<bool> ExecuteAsync(OrderRequest request)
    {
        _state.State.CurrentStep = SagaStep.ReserveInventory;
        await _state.WriteStateAsync();
        
        try
        {
            // Step 1: Reserve inventory
            var inventoryGrain = GrainFactory.GetGrain<IInventoryGrain>(request.ProductId);
            if (!await inventoryGrain.ReserveAsync(request.Quantity))
            {
                return false;
            }
            
            _state.State.CurrentStep = SagaStep.ProcessPayment;
            await _state.WriteStateAsync();
            
            // Step 2: Process payment
            var paymentGrain = GrainFactory.GetGrain<IPaymentGrain>(request.CustomerId);
            if (!await paymentGrain.ProcessAsync(request.Amount))
            {
                // Compensate: Release inventory
                await inventoryGrain.ReleaseAsync(request.Quantity);
                return false;
            }
            
            _state.State.CurrentStep = SagaStep.CreateOrder;
            await _state.WriteStateAsync();
            
            // Step 3: Create order
            var orderGrain = GrainFactory.GetGrain<IOrderGrain>(request.OrderId);
            await orderGrain.CreateAsync(request);
            
            _state.State.CurrentStep = SagaStep.Completed;
            await _state.WriteStateAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga failed at step {Step}", _state.State.CurrentStep);
            await CompensateAsync(request);
            return false;
        }
    }
    
    private async Task CompensateAsync(OrderRequest request)
    {
        // Rollback based on current step
        switch (_state.State.CurrentStep)
        {
            case SagaStep.ProcessPayment:
            case SagaStep.CreateOrder:
                var paymentGrain = GrainFactory.GetGrain<IPaymentGrain>(request.CustomerId);
                await paymentGrain.RefundAsync(request.Amount);
                goto case SagaStep.ReserveInventory;
                
            case SagaStep.ReserveInventory:
                var inventoryGrain = GrainFactory.GetGrain<IInventoryGrain>(request.ProductId);
                await inventoryGrain.ReleaseAsync(request.Quantity);
                break;
        }
    }
}
```

## Debugging and Troubleshooting

### Enable detailed logging
```csharp
siloBuilder
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
        logging.AddFilter("Orleans.Runtime.Scheduler", LogLevel.Trace);
    });
```

### Orleans Dashboard
Access at `http://localhost:8080/dashboard` to see:
- Active grains
- Silo status
- Performance metrics
- Stream subscriptions

## Best Practices

1. **Grain Design**: Keep grains focused, single-purpose
2. **State Size**: Keep grain state under 1MB for performance
3. **Idempotency**: Make grain methods idempotent where possible
4. **Timeouts**: Set reasonable timeouts for grain calls
5. **Backpressure**: Use streams for high-throughput scenarios
6. **Clustering**: Use persistent clustering for production
7. **Monitoring**: Implement comprehensive health checks and metrics
8. **Testing**: Use TestCluster for integration testing

## Resources

- [Orleans Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [Orleans Best Practices](https://learn.microsoft.com/en-us/dotnet/orleans/host/configuration-guide/best-practices)
- [Orleans Samples](https://github.com/dotnet/orleans/tree/main/samples)
