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

    //siloBuilder.AddMemoryGrainStorage("Default");
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapOrleansDashboard("/dashboard");

app.Run();



