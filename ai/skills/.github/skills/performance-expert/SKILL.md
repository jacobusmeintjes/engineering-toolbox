---
name: dotnet-performance-expert
description: Expert guidance for performance testing and optimization using k6 for load testing, BenchmarkDotNet for microbenchmarks, profiling tools, and .NET performance best practices
---

# .NET Performance Expert

## When to use this skill

Use this skill when:
- Writing performance benchmarks with BenchmarkDotNet
- Creating load tests with k6
- Profiling .NET applications
- Optimizing code performance
- Identifying performance bottlenecks
- Testing scalability and throughput
- Measuring latency and response times
- Conducting stress and spike tests

## BenchmarkDotNet - Microbenchmarking

### Basic Benchmark Setup
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace MyApp.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class StringOperationsBenchmark
{
    private const int Iterations = 1000;
    
    [Benchmark(Baseline = true)]
    public string StringConcatenation()
    {
        var result = "";
        for (int i = 0; i < Iterations; i++)
        {
            result += i.ToString();
        }
        return result;
    }
    
    [Benchmark]
    public string StringBuilderAppend()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Iterations; i++)
        {
            sb.Append(i.ToString());
        }
        return sb.ToString();
    }
    
    [Benchmark]
    public string StringBuilderWithCapacity()
    {
        var sb = new StringBuilder(capacity: Iterations * 4);
        for (int i = 0; i < Iterations; i++)
        {
            sb.Append(i.ToString());
        }
        return sb.ToString();
    }
}

// Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StringOperationsBenchmark>();
    }
}
```

### Parameterized Benchmarks
```csharp
[MemoryDiagnoser]
public class CollectionBenchmarks
{
    [Params(10, 100, 1000, 10000)]
    public int ItemCount { get; set; }
    
    private int[] _data = Array.Empty<int>();
    
    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, ItemCount).ToArray();
    }
    
    [Benchmark]
    public List<int> ListAdd()
    {
        var list = new List<int>();
        foreach (var item in _data)
        {
            list.Add(item);
        }
        return list;
    }
    
    [Benchmark]
    public List<int> ListWithCapacity()
    {
        var list = new List<int>(capacity: ItemCount);
        foreach (var item in _data)
        {
            list.Add(item);
        }
        return list;
    }
    
    [Benchmark]
    public int[] ArrayCopy()
    {
        var array = new int[ItemCount];
        Array.Copy(_data, array, ItemCount);
        return array;
    }
    
    [Benchmark]
    public Span<int> StackAllocSpan()
    {
        Span<int> span = stackalloc int[ItemCount];
        _data.AsSpan().CopyTo(span);
        return span;
    }
}
```

### Async Benchmarks
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class AsyncBenchmarks
{
    private HttpClient _httpClient = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }
    
    [GlobalCleanup]
    public void Cleanup()
    {
        _httpClient.Dispose();
    }
    
    [Benchmark]
    public async Task<string> HttpClientGetAsync()
    {
        return await _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");
    }
    
    [Benchmark]
    public async Task<byte[]> HttpClientGetByteArrayAsync()
    {
        return await _httpClient.GetByteArrayAsync("https://jsonplaceholder.typicode.com/posts/1");
    }
    
    [Benchmark]
    public async ValueTask<string> ValueTaskReturn()
    {
        var result = await GetFromCacheOrDbAsync("key-123");
        return result;
    }
    
    private async ValueTask<string> GetFromCacheOrDbAsync(string key)
    {
        // Simulate cache hit (synchronous path)
        if (key.Contains("123"))
        {
            return "cached-value";
        }
        
        // Simulate cache miss (async path)
        await Task.Delay(10);
        return "db-value";
    }
}
```

### Database Benchmarks
```csharp
[MemoryDiagnoser]
public class DatabaseBenchmarks
{
    private ApplicationDbContext _context = null!;
    private IQueryable<Customer> _customers = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=BenchmarkDb;")
            .Options;
        
        _context = new ApplicationDbContext(options);
        _customers = _context.Customers.AsNoTracking();
    }
    
    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }
    
    [Benchmark(Baseline = true)]
    public async Task<List<Customer>> ToListAsync()
    {
        return await _customers.ToListAsync();
    }
    
    [Benchmark]
    public async Task<List<CustomerDto>> ProjectToDto()
    {
        return await _customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            })
            .ToListAsync();
    }
    
    [Benchmark]
    public async Task<List<Customer>> WithSplitQuery()
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .Include(c => c.Addresses)
            .AsSplitQuery()
            .ToListAsync();
    }
    
    [Benchmark]
    public async Task<List<Customer>> CompiledQuery()
    {
        var query = EF.CompileAsyncQuery(
            (ApplicationDbContext ctx) => ctx.Customers.AsNoTracking());
        
        return await query(_context).ToListAsync();
    }
}
```

### Serialization Benchmarks
```csharp
[MemoryDiagnoser]
[ShortRunJob]
public class SerializationBenchmarks
{
    private Customer _customer = null!;
    private string _json = null!;
    private JsonSerializerOptions _options = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Performance Test Customer",
            Email = "perf@test.com",
            Orders = Enumerable.Range(1, 10)
                .Select(i => new Order { Id = i.ToString(), Total = i * 100 })
                .ToList()
        };
        
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        _json = JsonSerializer.Serialize(_customer, _options);
    }
    
    [Benchmark(Baseline = true)]
    public string SystemTextJsonSerialize()
    {
        return JsonSerializer.Serialize(_customer, _options);
    }
    
    [Benchmark]
    public Customer SystemTextJsonDeserialize()
    {
        return JsonSerializer.Deserialize<Customer>(_json, _options)!;
    }
    
    [Benchmark]
    public string SystemTextJsonWithSourceGen()
    {
        return JsonSerializer.Serialize(_customer, CustomerJsonContext.Default.Customer);
    }
    
    [Benchmark]
    public string NewtonsoftJsonSerialize()
    {
        return JsonConvert.SerializeObject(_customer);
    }
    
    [Benchmark]
    public Customer NewtonsoftJsonDeserialize()
    {
        return JsonConvert.DeserializeObject<Customer>(_json)!;
    }
}

// Source-generated JSON serializer context
[JsonSerializable(typeof(Customer))]
internal partial class CustomerJsonContext : JsonSerializerContext
{
}
```

### Running Benchmarks
```bash
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark
dotnet run -c Release --filter "*StringOperations*"

# Export results
dotnet run -c Release --exporters json html

# With specific runtime
dotnet run -c Release --runtimes net8.0 net7.0

# Memory diagnosis
dotnet run -c Release --memory

# Disassembly
dotnet run -c Release --disassembler
```

## k6 Load Testing

### Basic Load Test
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
    stages: [
        { duration: '30s', target: 10 },  // Ramp-up to 10 users
        { duration: '1m', target: 10 },   // Stay at 10 users
        { duration: '30s', target: 50 },  // Ramp-up to 50 users
        { duration: '2m', target: 50 },   // Stay at 50 users
        { duration: '30s', target: 0 },   // Ramp-down to 0 users
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],  // 95% of requests must complete below 500ms
        http_req_failed: ['rate<0.01'],    // Error rate must be below 1%
        errors: ['rate<0.05'],              // Custom error rate below 5%
    },
};

const BASE_URL = __ENV.BASE_URL || 'https://api.example.com';

export default function () {
    // GET request
    const getResponse = http.get(`${BASE_URL}/api/customers`);
    
    check(getResponse, {
        'GET status is 200': (r) => r.status === 200,
        'GET response time < 500ms': (r) => r.timings.duration < 500,
        'GET has customers': (r) => JSON.parse(r.body).length > 0,
    }) || errorRate.add(1);
    
    sleep(1);
    
    // POST request
    const payload = JSON.stringify({
        name: 'Load Test Customer',
        email: `customer-${__VU}-${__ITER}@test.com`,
    });
    
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${__ENV.API_TOKEN}`,
        },
    };
    
    const postResponse = http.post(`${BASE_URL}/api/customers`, payload, params);
    
    check(postResponse, {
        'POST status is 201': (r) => r.status === 201,
        'POST response time < 1000ms': (r) => r.timings.duration < 1000,
        'POST returns customer ID': (r) => JSON.parse(r.body).id !== undefined,
    }) || errorRate.add(1);
    
    sleep(2);
}
```

### Spike Test
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '10s', target: 10 },   // Normal load
        { duration: '10s', target: 100 },  // Spike to 100 users
        { duration: '30s', target: 100 },  // Stay at spike
        { duration: '10s', target: 10 },   // Recover
        { duration: '30s', target: 10 },   // Normal load
    ],
    thresholds: {
        http_req_duration: ['p(99)<1500'], // 99% under 1.5s during spike
        http_req_failed: ['rate<0.05'],    // Less than 5% errors
    },
};

const BASE_URL = __ENV.BASE_URL || 'https://api.example.com';

export default function () {
    const response = http.get(`${BASE_URL}/api/orders`);
    
    check(response, {
        'status is 200': (r) => r.status === 200,
        'response time acceptable': (r) => r.timings.duration < 2000,
    });
    
    sleep(Math.random() * 2); // Random think time
}
```

### Soak/Endurance Test
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '5m', target: 50 },    // Ramp-up
        { duration: '4h', target: 50 },    // Sustained load for 4 hours
        { duration: '5m', target: 0 },     // Ramp-down
    ],
    thresholds: {
        http_req_duration: ['p(95)<1000'],
        http_req_failed: ['rate<0.01'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'https://api.example.com';

export default function () {
    const response = http.get(`${BASE_URL}/api/health`);
    
    check(response, {
        'health check passed': (r) => r.status === 200,
    });
    
    sleep(5);
}
```

### Stress Test
```javascript
import http from 'k6/http';
import { check } from 'k6';

export const options = {
    stages: [
        { duration: '2m', target: 100 },   // Below normal load
        { duration: '5m', target: 100 },
        { duration: '2m', target: 200 },   // Normal load
        { duration: '5m', target: 200 },
        { duration: '2m', target: 300 },   // Above normal load
        { duration: '5m', target: 300 },
        { duration: '2m', target: 400 },   // Stress level
        { duration: '5m', target: 400 },
        { duration: '10m', target: 0 },    // Recovery
    ],
};

const BASE_URL = __ENV.BASE_URL || 'https://api.example.com';

export default function () {
    const response = http.get(`${BASE_URL}/api/customers`);
    
    check(response, {
        'status is 200 or 503': (r) => r.status === 200 || r.status === 503,
    });
}
```

### API Workflow Test
```javascript
import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// Load test data once, shared across all VUs
const testData = new SharedArray('customers', function () {
    return JSON.parse(open('./test-data.json')).customers;
});

export const options = {
    vus: 50,
    duration: '5m',
    thresholds: {
        'group_duration{group:::01_login}': ['p(95)<1000'],
        'group_duration{group:::02_browse}': ['p(95)<500'],
        'group_duration{group:::03_order}': ['p(95)<2000'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'https://api.example.com';

export function setup() {
    // Setup code - runs once before test
    const adminToken = login('admin', 'password');
    return { adminToken };
}

export default function (data) {
    const customer = testData[__VU % testData.length];
    let token;
    
    // 1. Login
    group('01_login', function () {
        token = login(customer.email, customer.password);
        check(token, { 'logged in successfully': (t) => t !== null });
    });
    
    sleep(1);
    
    // 2. Browse products
    group('02_browse', function () {
        const params = {
            headers: { 'Authorization': `Bearer ${token}` },
        };
        
        const response = http.get(`${BASE_URL}/api/products`, params);
        check(response, {
            'products loaded': (r) => r.status === 200,
            'has products': (r) => JSON.parse(r.body).length > 0,
        });
    });
    
    sleep(2);
    
    // 3. Create order
    group('03_order', function () {
        const order = {
            customerId: customer.id,
            items: [
                { productId: 'prod-123', quantity: 2 },
                { productId: 'prod-456', quantity: 1 },
            ],
        };
        
        const params = {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
        };
        
        const response = http.post(
            `${BASE_URL}/api/orders`,
            JSON.stringify(order),
            params
        );
        
        check(response, {
            'order created': (r) => r.status === 201,
            'has order ID': (r) => JSON.parse(r.body).id !== undefined,
        });
    });
    
    sleep(2);
}

export function teardown(data) {
    // Cleanup code - runs once after test
}

function login(email, password) {
    const payload = JSON.stringify({ email, password });
    const params = { headers: { 'Content-Type': 'application/json' } };
    
    const response = http.post(`${BASE_URL}/api/auth/login`, payload, params);
    
    if (response.status === 200) {
        const body = JSON.parse(response.body);
        return body.token;
    }
    
    return null;
}
```

### WebSocket Load Test
```javascript
import ws from 'k6/ws';
import { check } from 'k6';

export const options = {
    vus: 100,
    duration: '5m',
};

const WS_URL = __ENV.WS_URL || 'wss://api.example.com/ws';

export default function () {
    const url = `${WS_URL}?userId=${__VU}`;
    const params = { tags: { name: 'WebSocketTest' } };
    
    const response = ws.connect(url, params, function (socket) {
        socket.on('open', function () {
            console.log('Connected');
            
            // Send a message every 5 seconds
            socket.setInterval(function () {
                socket.send(JSON.stringify({
                    action: 'ping',
                    timestamp: Date.now(),
                }));
            }, 5000);
        });
        
        socket.on('message', function (message) {
            const data = JSON.parse(message);
            check(data, {
                'message received': (d) => d !== null,
                'valid timestamp': (d) => d.timestamp > 0,
            });
        });
        
        socket.on('error', function (e) {
            console.log('Error: ' + e.error());
        });
        
        socket.on('close', function () {
            console.log('Disconnected');
        });
        
        // Keep connection open for 1 minute
        socket.setTimeout(function () {
            socket.close();
        }, 60000);
    });
    
    check(response, { 'status is 101': (r) => r && r.status === 101 });
}
```

### Running k6 Tests
```bash
# Run basic test
k6 run script.js

# Run with environment variables
k6 run --env BASE_URL=https://api.staging.example.com script.js

# Run with custom VUs and duration
k6 run --vus 100 --duration 30s script.js

# Run and output results to InfluxDB
k6 run --out influxdb=http://localhost:8086/k6 script.js

# Run in cloud
k6 cloud script.js

# Generate HTML report
k6 run --out json=results.json script.js
k6-html-reporter results.json
```

## .NET Performance Optimization Patterns

### Memory Allocation Optimization
```csharp
// ❌ Bad: Creates new array on each call
public byte[] ProcessData(byte[] input)
{
    var output = new byte[input.Length];
    // Process...
    return output;
}

// ✅ Good: Use ArrayPool
public byte[] ProcessDataOptimized(byte[] input)
{
    var output = ArrayPool<byte>.Shared.Rent(input.Length);
    try
    {
        // Process...
        var result = new byte[input.Length];
        Array.Copy(output, result, input.Length);
        return result;
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(output);
    }
}

// ✅ Better: Use Span<T> for zero-copy
public void ProcessDataSpan(ReadOnlySpan<byte> input, Span<byte> output)
{
    // Process directly on spans - no allocation
    for (int i = 0; i < input.Length; i++)
    {
        output[i] = (byte)(input[i] ^ 0xFF);
    }
}
```

### LINQ Optimization
```csharp
// ❌ Bad: Multiple enumerations
public decimal GetTotalForPremiumCustomers(List<Customer> customers)
{
    var premium = customers.Where(c => c.Tier == Tier.Premium);
    var active = premium.Where(c => c.IsActive);
    var total = active.Sum(c => c.TotalSpent);
    return total;
}

// ✅ Good: Single enumeration
public decimal GetTotalForPremiumCustomersOptimized(List<Customer> customers)
{
    return customers
        .Where(c => c.Tier == Tier.Premium && c.IsActive)
        .Sum(c => c.TotalSpent);
}

// ✅ Better: For loop when performance critical
public decimal GetTotalForPremiumCustomersFast(List<Customer> customers)
{
    decimal total = 0;
    for (int i = 0; i < customers.Count; i++)
    {
        var customer = customers[i];
        if (customer.Tier == Tier.Premium && customer.IsActive)
        {
            total += customer.TotalSpent;
        }
    }
    return total;
}
```

### String Optimization
```csharp
// ❌ Bad: String concatenation in loop
public string BuildQuery(List<string> ids)
{
    var query = "SELECT * FROM Customers WHERE Id IN (";
    foreach (var id in ids)
    {
        query += $"'{id}',";
    }
    query = query.TrimEnd(',') + ")";
    return query;
}

// ✅ Good: Use StringBuilder
public string BuildQueryOptimized(List<string> ids)
{
    var sb = new StringBuilder("SELECT * FROM Customers WHERE Id IN (", 
        capacity: 50 + ids.Count * 40);
    
    for (int i = 0; i < ids.Count; i++)
    {
        if (i > 0) sb.Append(',');
        sb.Append('\'').Append(ids[i]).Append('\'');
    }
    
    sb.Append(')');
    return sb.ToString();
}

// ✅ Better: Use string.Join
public string BuildQueryBetter(List<string> ids)
{
    return $"SELECT * FROM Customers WHERE Id IN ({string.Join(',', ids.Select(id => $"'{id}'"))})";
}
```

### Async/Await Optimization
```csharp
// ❌ Bad: Unnecessary async
public async Task<int> GetCountAsync()
{
    return await Task.FromResult(_cache.Count); // Unnecessary allocation
}

// ✅ Good: Return Task directly
public Task<int> GetCountOptimized()
{
    return Task.FromResult(_cache.Count);
}

// ✅ Better: Use ValueTask for hot paths
public ValueTask<int> GetCountValueTask()
{
    return ValueTask.FromResult(_cache.Count); // No allocation if synchronous
}

// ❌ Bad: Awaiting in a loop
public async Task<List<Customer>> GetCustomersAsync(List<string> ids)
{
    var customers = new List<Customer>();
    foreach (var id in ids)
    {
        customers.Add(await _repository.GetByIdAsync(id)); // Sequential!
    }
    return customers;
}

// ✅ Good: Parallel execution
public async Task<List<Customer>> GetCustomersParallelAsync(List<string> ids)
{
    var tasks = ids.Select(id => _repository.GetByIdAsync(id));
    var customers = await Task.WhenAll(tasks);
    return customers.ToList();
}
```

## Profiling Tools

### dotnet-trace
```bash
# Collect trace
dotnet-trace collect --process-id <PID> --format speedscope

# Collect with specific providers
dotnet-trace collect --process-id <PID> \
    --providers Microsoft-Windows-DotNETRuntime:0x1F000000000C:5

# Convert to speedscope format
dotnet-trace convert trace.nettrace --format speedscope
```

### dotnet-counters
```bash
# Monitor performance counters
dotnet-counters monitor --process-id <PID>

# Monitor specific counters
dotnet-counters monitor --process-id <PID> \
    System.Runtime \
    Microsoft.AspNetCore.Hosting
```

### dotnet-dump
```bash
# Create dump
dotnet-dump collect --process-id <PID>

# Analyze dump
dotnet-dump analyze dump_file.dmp

# Common commands in dump analysis
> clrstack           # Show managed call stack
> dumpheap -stat     # Show heap statistics
> gcroot <address>   # Find GC roots
> dumpheap -mt <MT>  # Dump objects of type
```

### Visual Studio Profiler
- CPU Usage
- Memory Usage
- .NET Object Allocation
- Database queries
- File I/O
- Events

### PerfView
- CPU sampling
- Memory allocation
- GC analysis
- Thread time analysis
- ETW events

## Performance Testing Strategy

### 1. Baseline Establishment
```
1. Run benchmarks on clean environment
2. Document hardware specs
3. Record baseline metrics
4. Version control benchmark code
5. Automate benchmark runs in CI/CD
```

### 2. Load Test Scenarios
```
- Normal Load: Expected daily traffic
- Peak Load: Expected peak times (2-3x normal)
- Stress Test: Beyond peak until system breaks
- Spike Test: Sudden traffic increase
- Soak Test: Extended duration (hours/days)
```

### 3. Performance Budgets
```
API Response Times:
- p50: < 200ms
- p95: < 500ms
- p99: < 1000ms

Throughput:
- Minimum: 1000 req/sec
- Target: 5000 req/sec

Error Rate:
- Maximum: 0.1%

Resource Usage:
- CPU: < 70% average
- Memory: < 80% of available
- Database connections: < 80% of pool
```

## Best Practices

### 1. Performance Testing
- Always run in Release mode
- Test on production-like hardware
- Warm up before measurements
- Run multiple iterations
- Account for GC pauses
- Isolate tests from external factors

### 2. Benchmark Design
- Keep benchmarks focused and simple
- Use [MemoryDiagnoser] to track allocations
- Set baseline for comparisons
- Use parameters to test different scenarios
- Document expected results

### 3. Load Testing
- Start with smoke tests (1-2 users)
- Gradually increase load
- Monitor all system components
- Test realistic user journeys
- Include think time between requests
- Test failure scenarios

### 4. Optimization
- Measure before optimizing
- Focus on hot paths first
- Consider readability vs performance trade-offs
- Use profiler to identify bottlenecks
- Test after each optimization

## Resources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [k6 Documentation](https://k6.io/docs/)
- [.NET Performance Tips](https://learn.microsoft.com/en-us/dotnet/core/runtime/performance-tips)
- [PerfView Tutorial](https://github.com/microsoft/perfview)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
