using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using orleans_hive_client;
using orleans_hive_shared;

System.Threading.ThreadPool.SetMinThreads(500, 500);
System.Threading.ThreadPool.SetMaxThreads(500, 500);


var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddKeyedRedisClient("redis");
builder.UseOrleansClient();


var app = builder.Build();


app.MapGet("/", async (IGrainFactory grains) =>
{
    var grain = grains.GetGrain<ICounterGrain>("counter");
    var count = await grain.Increment();
    return Results.Ok(new { count });
});

app.Run();
