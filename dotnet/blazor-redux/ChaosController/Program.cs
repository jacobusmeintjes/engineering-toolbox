
using ChaosController.HttpHandler;
using ChaosController.Infrastructure;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Logging.AddConsole();

// Add Toxiproxy client
var toxiProxyBaseUrl = builder.Configuration["TOXIPROXY_BASEURL"];
if (!string.IsNullOrEmpty(toxiProxyBaseUrl))
{
    builder.Services
        .AddRefitClient<IToxiProxyClient>()
                               .ConfigureHttpClient(c =>
                               {
                                   c.BaseAddress = new Uri(toxiProxyBaseUrl);
                                   c.DefaultRequestHeaders.Clear();
                               })
                               .AddHttpMessageHandler<ConsoleLoggingHttpMessageHandler>();
}
// Add hosted service to initialize proxies
//builder.Services.AddHostedService<ToxiproxyInitializer>();

// Add chaos controller
//builder.Services.AddSingleton<ChaosOrchestrator>();

builder.Services.AddScoped<ConsoleLoggingHttpMessageHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapPost("/proxy", async (Proxy proxy) =>
{
    var toxiProxyClient = app.Services.GetRequiredService<IToxiProxyClient>();
    await toxiProxyClient.Add(proxy);
    return Results.Ok($"Proxy {proxy.Name} added");
});

app.MapGet("/proxy", async () => {
    var toxiProxyClient = app.Services.GetRequiredService<IToxiProxyClient>();
    var list = await toxiProxyClient.List();
    return Results.Ok(list);
});

// HTTP endpoints for external control
//app.MapPost("/chaos/break-redis", async (ChaosOrchestrator orchestrator) =>
//{
//    await orchestrator.BreakRedisConnectionAsync();
//    return Results.Ok("Redis connection broken");
//});

//app.MapPost("/chaos/add-latency", async (ChaosOrchestrator orchestrator, int milliseconds) =>
//{
//    await orchestrator.AddLatencyAsync("redis_proxy", milliseconds);
//    return Results.Ok($"Added {milliseconds}ms latency");
//});

//app.MapPost("/chaos/restore", async (ChaosOrchestrator orchestrator) =>
//{
//    await orchestrator.RestoreAllAsync();
//    return Results.Ok("All toxics removed");
//});


app.Run();
