---
name: dotnet-aspire-distributed-apps
description: Expert guidance for building distributed applications using .NET Aspire, including service defaults, orchestration, and cloud-native patterns
---

# .NET Aspire Distributed Applications

## When to use this skill

Use this skill when:
- Building microservices with .NET Aspire
- Configuring service orchestration
- Implementing observability patterns
- Setting up distributed application testing
- Working with Aspire components (Redis, RabbitMQ, PostgreSQL, etc.)

## Project Structure
```
src/
├── AspireSample.AppHost/          # Orchestrator project
│   └── Program.cs
├── AspireSample.ServiceDefaults/  # Shared defaults
│   └── Extensions.cs
├── AspireSample.ApiService/       # API project
└── AspireSample.Web/              # Frontend project
```

## Core Patterns

### 1. AppHost Configuration
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add resources
var cache = builder.AddRedis("cache");
var db = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("catalogdb");

// Add projects with references
var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithReference(cache)
    .WithReference(db);

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(cache);
```

### 2. Service Defaults Setup

Always add service defaults in Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (telemetry, health checks, etc.)
builder.AddServiceDefaults();

// Your services
builder.Services.AddControllers();

var app = builder.Build();

// Map defaults
app.MapDefaultEndpoints();

app.Run();
```

### 3. Component Integration
```csharp
// Redis caching
builder.AddRedisClient("cache");

// PostgreSQL
builder.AddNpgsqlDataSource("catalogdb");

// RabbitMQ messaging
builder.AddRabbitMQClient("messaging");

// Azure Service Bus
builder.AddAzureServiceBusClient("messaging");
```

## Testing Distributed Apps

### Integration Testing Pattern
```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccess()
    {
        // Arrange
        var factory = new DistributedApplicationTestingBuilder()
            .WithReference<Projects.AspireSample_AppHost>()
            .Build();
        
        await using var app = await factory.BuildAsync();
        await app.StartAsync();
        
        // Act
        var client = app.CreateHttpClient("apiservice");
        var response = await client.GetAsync("/weatherforecast");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## Best Practices

1. **Always use service defaults** for consistency
2. **Leverage built-in telemetry** - it's configured automatically
3. **Use resource builders** for typed configuration
4. **Test with the AppHost** to ensure orchestration works
5. **Use connection strings from configuration** - never hardcode

## Common Gotchas

- Don't forget `.WithExternalHttpEndpoints()` for public-facing services
- Service defaults must be added before other services
- Resource names in AppHost must match service references
- Use `WaitFor()` for dependent resources

## Resources

- [Examples directory](./examples/) - Complete working samples
- [Health check template](./templates/health-check.cs)
- [Integration test template](./templates/integration-test.cs)