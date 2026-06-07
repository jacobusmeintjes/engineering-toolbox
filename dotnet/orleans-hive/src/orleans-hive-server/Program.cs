using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Orleans.Concurrency;
using Orleans.Dashboard;
using orleans_hive_server;
using orleans_hive_shared;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddKeyedRedisClient("redis");
builder.UseOrleans(siloBuilder =>
{
    var connStr = builder.Configuration.GetConnectionString("redis");

    siloBuilder.AddDashboard(opts =>
    {
        opts.CounterUpdateIntervalMs = 1000;
        opts.HistoryLength = 100;
    });


    siloBuilder.UseRedisClustering(options =>
    {
        options.ConfigurationOptions = ConfigurationOptions.Parse(connStr!);
    });

    siloBuilder.AddRedisGrainStorageAsDefault(options =>
    {
        options.ConfigurationOptions = ConfigurationOptions.Parse(connStr!);
    });
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapOrleansDashboard("/dashboard");

app.Run();



public class CounterState
{
    [Id(0)]
    public int Count { get; set; }
}


[StatelessWorker]
public class CounterGrain : Grain, ICounterGrain
{

    private readonly IPersistentState<CounterState> _state;
    public CounterGrain([PersistentState("count", "Default")] IPersistentState<CounterState> state)
    {
        _state = state;
    }
    public async Task<int> Increment()
    {
        _state.State.Count++;
        //await _state.WriteStateAsync();
        return _state.State.Count;
    }
    public Task<int> GetCount()
    {
        return Task.FromResult(_state.State.Count);
    }
}